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

namespace Crash
{
	///<summary>The crash plugin for multi user rhino collaboration</summary>
	public sealed class CrashRhinoPlugIn : PlugIn
	{
		/// <summary>The Id of the Crash Plugin. DO NOT reuse this!</summary>
		private const string CrashPluginId = "53CB2393-C71F-4079-9CEC-97464FF9D14E";

		private const string CrashPluginExtension = ".op";

		/// <summary>Contains all of the Change Definitions of this PlugIn</summary>
		private static readonly Stack<IChangeDefinition> Changes;

		#region Crash Plugins

		// TODO : https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
		private static void LoadCrashPlugins()
		{
			IEnumerable<Guid> pluginIds = GetInstalledPlugIns().Keys;
			var pluginInfos = pluginIds.Select(p => GetPlugInInfo(p));
			foreach (var pluginInfo in pluginInfos)
			{
				var pluginDirectory = Path.GetDirectoryName(pluginInfo.FileName);
				if (!Directory.Exists(pluginDirectory))
				{
					continue;
				}

				var crashPluginExtensions = Directory.EnumerateFiles(pluginDirectory, $"*{CrashPluginExtension}");
				if (crashPluginExtensions?.Any() != true)
				{
					continue;
				}

				foreach (var pluginAssembly in crashPluginExtensions)
				{
					LoadCrashPlugin(pluginAssembly);
				}
			}
		}

		private static void LoadCrashPlugin(string crashAssembly)
		{
			var assembly = System.Reflection.Assembly.LoadFrom(crashAssembly);
			var changeDefinitionTypes =
				assembly.ExportedTypes.Where(et => et.GetInterfaces().Contains(typeof(IChangeDefinition)));
			foreach (var changeDefinitionType in changeDefinitionTypes)
			{
				var changeDefinition = Activator.CreateInstance(changeDefinitionType) as IChangeDefinition;
				Changes.Push(changeDefinition);
			}
		}

		#endregion

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
											  UsersForm.CloseActiveForm();
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
#pragma warning disable CS4014, VSTHRD110 // Because this call is not awaited, execution of the current method continues before the call is completed
				CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);
#pragma warning restore CS4014, VSTHRD110 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		public CrashRhinoPlugIn()
		{
			Instance = this;

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
			// Add feature flags as advanced settings here!
			InteractivePipe.Active = new InteractivePipe { Enabled = false };

			CrashApp.UserMessage += (_, m) => RhinoApp.WriteLine(m);

			return base.OnLoad(ref errorMessage);
		}

		#endregion
	}
}
