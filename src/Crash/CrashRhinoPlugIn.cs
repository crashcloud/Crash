using Crash.Common.Communications;
using Crash.Common.Events;
using Crash.Handlers;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Camera;
using Crash.Handlers.Plugins.Geometry;
using Crash.Handlers.Plugins.Initializers;
using Crash.Plugins;
using Crash.UI.ExceptionsAndErrors;
using Crash.UI.UsersView;

using Eto.Forms;

using Rhino.PlugIns;

namespace Crash
{
	///<summary>The crash plugin for multi user rhino collaboration</summary>
	public sealed class CrashRhinoPlugIn : PlugIn
	{
		/// <summary>The Id of the Crash Plugin. DO NOT reuse this!</summary>
		private const string CrashPluginId = "53CB2393-C71F-4079-9CEC-97464FF9D14E";

		/// <summary>Contains all of the Change Definitions of this PlugIn</summary>
		internal static Stack<IChangeDefinition> Changes { get; set; }

		#region Crash Plugin Specifics

		static CrashRhinoPlugIn()
		{
			Changes = new Stack<IChangeDefinition>();
			RhinoApp.Idle += LoadCrashPlugins;
		}

		private static void LoadCrashPlugins(object? sender, EventArgs e)
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
			InteractivePipe.Active.Enabled = false;
			InteractivePipe.ClearChangeDefinitions();
		}

		private void CrashDocRegistryOnDocumentRegistered(object? sender, CrashEventArgs e)
		{
			var dispatcher = new EventDispatcher(e.CrashDoc);
			dispatcher.RegisterDefaultServerNotifiers();
			dispatcher.RegisterDefaultServerCalls(e.CrashDoc);
			RegisterDefinitions(dispatcher);
			e.CrashDoc.Dispatcher = dispatcher;
			RegisterExceptions(e.CrashDoc.LocalClient as CrashClient);
			InteractivePipe.Active.Enabled = true;
			badPipe = new BadChangePipeline(e.CrashDoc);
		}

		private BadChangePipeline badPipe;

		private void RegisterExceptions(CrashClient client)
		{
			client.OnServerClosed += ClientOnOnServerClosed;
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

		private async void ClientOnOnServerClosed(object sender, CrashEventArgs args)
		{
			var message = "The server connection has been lost.\n" +
			              "Your model will be closed.\n" +
			              "Your Data is likely safe.";
			try
			{
				RhinoApp.InvokeOnUiThread(() =>
				                          {
					                          MessageBox.Show(message, MessageBoxButtons.OK);
					                          UsersForm.CloseActiveForm();
				                          });

				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.CrashDoc);
				await CrashDocRegistry.DisposeOfDocumentAsync(args.CrashDoc);
				rhinoDoc.Views.Redraw();
			}
			catch
			{
				RhinoApp.WriteLine(message);
			}
		}

		private void RegisterDefinitions(EventDispatcher dispatcher)
		{
			var changeEnuner = Changes.GetEnumerator();

			while (changeEnuner.MoveNext())
			{
				var changeDefinition = changeEnuner.Current;
				dispatcher.RegisterDefinition(changeDefinition);
				InteractivePipe.RegisterChangeDefinition(changeDefinition);
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
				CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);
			}
		}

		public CrashRhinoPlugIn()
		{
			Instance = this;

			// Register the Defaults!
			Changes.Push(new GeometryChangeDefinition());
			Changes.Push(new CameraChangeDefinition());
			Changes.Push(new DoneDefinition());

			CrashDocRegistry.DocumentRegistered += CrashDocRegistryOnDocumentRegistered;
			CrashDocRegistry.DocumentDisposed += CrashDocRegistryOnDocumentDisposed;
		}

		public static Guid PluginId => new(CrashPluginId);

		protected override string LocalPlugInName => "Crash";

		///<summary>Gets the only instance of the CrashPlugin plug-in.</summary>
		public static CrashRhinoPlugIn Instance { get; private set; }

		protected override LoadReturnCode OnLoad(ref string errorMessage)
		{
			// Add feature flags as advanced settings here!
			InteractivePipe.Active = new InteractivePipe { Enabled = false };

			return base.OnLoad(ref errorMessage);
		}

		#endregion
	}
}
