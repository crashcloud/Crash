using Crash.Common.Communications;
using Crash.Common.Events;
using Crash.Handlers;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Camera;
using Crash.Handlers.Plugins.Geometry;
using Crash.Handlers.Plugins.Initializers;
using Crash.Plugins;
using Crash.Handlers.Plugins.Layers;
using Crash.UI.ExceptionsAndErrors;
using Crash.UI.UsersView;

using Eto.Forms;

using Rhino.PlugIns;
using Crash.Common.App;
using Crash.Common.Document;
using Crash.Handlers.Data;
using Rhino.UI;

namespace Crash
{
	///<summary>The crash plugin for multi user rhino collaboration</summary>
	public sealed class CrashRhinoPlugIn : PlugIn
	{
		/// <summary>The Id of the Crash Plugin. DO NOT reuse this!</summary>
		private const string CrashPluginId = "53CB2393-C71F-4079-9CEC-97464FF9D14E";

		/// <summary>Contains all of the Change Definitions of this PlugIn</summary>
		private Stack<IChangeDefinition> Changes { get; set; }

		#region Crash Plugin Specifics

		private void LoadCrashPlugins(object? sender, EventArgs e)
		{
			RhinoApp.Idle -= LoadCrashPlugins;

			IEnumerable<Guid> pluginIds = GetInstalledPlugIns().Keys;
			var pluginInfos = pluginIds.Select(GetPlugInInfo).ToList();
			List<string> pluginLocations = new(pluginInfos.Count);
			foreach (var pluginInfo in pluginInfos)
			{
				var pluginDirectory = Path.GetDirectoryName(pluginInfo.FileName);
				pluginLocations.Add(pluginDirectory);
			}

			var loader = new CrashPluginLoader(pluginLocations);
			var changes = loader.LoadCrashPlugins();
			foreach (var change in changes)
			{
				Changes.Push(change);
			}
		}

		private void CrashDocRegistryOnDocumentDisposed(object? sender, CrashEventArgs e)
		{
			var dispatcher = e.CrashDoc.Dispatcher as EventDispatcher;
			dispatcher?.DeregisterDefaultServerCalls();
			e.CrashDoc.Dispatcher = null;
			var pipe = InteractivePipe.GetActive(e.CrashDoc);
			pipe.Enabled = false;
			pipe.ClearChangeDefinitions();
		}

		private void CrashDocRegistryOnDocumentRegistered(object? sender, CrashEventArgs e)
		{
			var dispatcher = new EventDispatcher(e.CrashDoc);
			dispatcher.RegisterDefaultServerNotifiers();
			dispatcher.RegisterDefaultServerCalls(e.CrashDoc);
			RegisterDefinitions(e.CrashDoc, dispatcher);
			e.CrashDoc.Dispatcher = dispatcher;
			RegisterExceptions(e.CrashDoc.LocalClient as CrashClient);
			var pipe = InteractivePipe.GetActive(e.CrashDoc);
			pipe.Enabled = true;
			badPipe = new BadChangePipeline(e.CrashDoc);
		}

		private BadChangePipeline badPipe;

		private void RegisterExceptions(CrashClient client)
		{
#pragma warning disable CS8622 // Avoid async void methods
			client.OnServerClosed += ClientOnOnServerClosed;
#pragma warning restore CS8622 // Avoid async void methods
			client.OnPushChangeFailed += ClientOnOnPushChangeFailed;
		}

		private void ClientOnOnPushChangeFailed(object? sender, CrashChangeArgs? args)
		{
			if (args is null || !args.Changes.Any())
			{
				return;
			}

			badPipe.Push(args.Changes.Select(c => c.Id));

			var badChangeToast = new Notification
			{
				Title = "Changes failed to send!",
				Message = "Any changes highlighted in red will not be communicated\n" +
												 "It is advised to delete them.\n" +
												 "It may be because the Change is > 1Mb."
			};

			RhinoApp.InvokeOnUiThread(() =>
										{
											badChangeToast.Show();
										});
		}

#pragma warning disable VSTHRD100 // Avoid async void methods
		private async void ClientOnOnServerClosed(object sender, CrashEventArgs args)

		{
			var message = "The server connection has been lost.\n" +
							"Your model will be closed.\n" +
							"Objects highlighted in Red have not transmitted all of their data and will be lost.";
			try
			{
				RhinoApp.InvokeOnUiThread(() =>
											{
												MessageBox.Show(message, MessageBoxButtons.OK);
												UsersForm.CloseActiveForm(args.CrashDoc);
											});

				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.CrashDoc);
				await CrashDocRegistry.DisposeOfDocumentAsync(args.CrashDoc);
				rhinoDoc.Views.Redraw();
			}
			catch
			{

			}
		}
#pragma warning restore VSTHRD100 // Avoid async void methods

		private void RegisterDefinitions(CrashDoc crashDoc, EventDispatcher dispatcher)
		{
			var changeEnuner = Changes.GetEnumerator();

			while (changeEnuner.MoveNext())
			{
				var changeDefinition = changeEnuner.Current;
				dispatcher.RegisterDefinition(changeDefinition);

				var pipe = InteractivePipe.GetActive(crashDoc);
				pipe.Enabled = true;
				pipe.RegisterChangeDefinition(changeDefinition);
			}
		}

		#endregion

		#region Rhino Plugin Overrides

		public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;

		protected override void OnShutdown()
		{
			var openCrashDocs = CrashDocRegistry.GetOpenDocuments().ToArray();
			foreach (var crashDoc in openCrashDocs)
			{
#pragma warning disable CS4014, VSTHRD110 // Because this call is not awaited, execution of the current method continues before the call is completed
				CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);
#pragma warning restore CS4014, VSTHRD110 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		public CrashRhinoPlugIn()
		{
			Instance = this;

			Changes = new Stack<IChangeDefinition>();

			// Register the Defaults!
			Changes.Push(new GeometryChangeDefinition());
			Changes.Push(new CameraChangeDefinition());
			Changes.Push(new DoneDefinition());
			Changes.Push(new LayerChangeDefinition());

			CrashDocRegistry.DocumentRegistered += CrashDocRegistryOnDocumentRegistered;
			CrashDocRegistry.DocumentDisposed += CrashDocRegistryOnDocumentDisposed;
		}

		public static Guid PluginId => new(CrashPluginId);

		protected override string LocalPlugInName => "Crash";

		///<summary>Gets the only instance of the CrashPlugin plug-in.</summary>
		public static CrashRhinoPlugIn Instance { get; private set; }

		protected override LoadReturnCode OnLoad(ref string errorMessage)
		{
			CrashApp.UserMessage += (_, m) => RhinoApp.WriteLine(m);
			RhinoApp.Idle += LoadCrashPlugins;
			SetupScreenshotCaching();

			return base.OnLoad(ref errorMessage);
		}

		private void SetupScreenshotCaching()
		{
			CrashDocRegistry.DocumentRegistered += ScreenshotView;
		}

		private static async void ScreenshotView(object sender, CrashEventArgs e)
		{
			if (e.CrashDoc is null) return;
			try
			{
				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(e.CrashDoc);
				if (rhinoDoc is null) return;

				while (true)
				{
					if (e?.CrashDoc is null) break;
					if (!e.CrashDoc.LocalClient.IsConnected)
					{
						await Task.Delay(TimeSpan.FromSeconds(2));
						continue;
					}

					if (!SharedModelCache.TryGetSharedModelsData(e.CrashDoc, out var models)) return;

					var url = e.CrashDoc?.LocalClient?.Url ?? string.Empty;
					if (string.IsNullOrEmpty(url)) return;

					var bitmap = rhinoDoc.Views.ActiveView.CaptureToBitmap(new System.Drawing.Size(240, 240));
					if (bitmap is null) return;

					var nonNullModels = models.Where(m => m is not null).ToList();
					foreach (var model in nonNullModels)
					{
						if (string.Equals(model.ModelAddress, url)) continue;
						model.Thumbnail = bitmap.ToEto();
					}

					SharedModelCache.TrySaveSharedModels(nonNullModels);

					await Task.Delay(TimeSpan.FromMinutes(10));
				}
			}
			catch { }
		}

		#endregion
	}
}
