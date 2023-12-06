using Crash.Common.Communications;
using Crash.Common.Events;
using Crash.Handlers;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Camera;
using Crash.Handlers.Plugins.Geometry;
using Crash.Handlers.Plugins.Initializers;

using Rhino.PlugIns;

namespace Crash
{
	///<summary>The crash plugin for multi user rhino collaboration</summary>
	public sealed class CrashRhinoPlugIn : PlugIn
	{
		private const string CrashPluginExtension = ".mup";

		/// <summary>The Id of the Crash Plugin. DO NOT reuse this!</summary>
		private const string CrashPluginId = "53CB2393-C71F-4079-9CEC-97464FF9D14E";

		/// <summary>Contains all of the Change Definitions of this PlugIn</summary>
		private readonly Stack<IChangeDefinition> Changes;

		#region Crash Plugins

		private void LoadCrashPlugins()
		{
			IEnumerable<Guid> pluginIds = GetInstalledPlugIns().Keys;
			foreach (var pluginId in pluginIds)
			{
				var plugin = Find(pluginId);
				var pluginDirection = Path.GetDirectoryName(plugin.Assembly.Location);
				var crashPluginExtensions = Directory.EnumerateFiles(pluginDirection, $"*.{CrashPluginExtension}");
				if (!crashPluginExtensions?.Any() != true)
				{
					continue;
				}

				foreach (var crashAssembly in crashPluginExtensions)
				{
					LoadCrashPlugin(crashAssembly);
				}
			}
		}

		private void LoadCrashPlugin(string crashAssembly)
		{
			var assembly = AppDomain.CurrentDomain.Load(crashAssembly);

			var changeDefinitionTypes = assembly.ExportedTypes.Where(et => et.IsSubclassOf(typeof(CrashPlugIn)));
			foreach (var changeDefinitionType in changeDefinitionTypes)
			{
				var changeDefinition = Activator.CreateInstance(changeDefinitionType) as IChangeDefinition;
				Changes.Push(changeDefinition);
			}
		}

		#endregion

		#region Crash Plugin Specifics

		private void CrashDocRegistryOnDocumentDisposed(object sender, CrashEventArgs e)
		{
			var dispatcher = e.CrashDoc.Dispatcher as EventDispatcher;
			dispatcher?.DeregisterDefaultServerCalls();
			e.CrashDoc.Dispatcher = null;
			InteractivePipe.Active.Enabled = false;
			InteractivePipe.ClearChangeDefinitions();
		}

		private void CrashDocRegistryOnDocumentRegistered(object sender, CrashEventArgs e)
		{
			var dispatcher = new EventDispatcher();
			dispatcher.RegisterDefaultServerNotifiers();
			RegisterDefinitions(dispatcher);
			dispatcher.RegisterDefaultServerCalls(e.CrashDoc);
			e.CrashDoc.Dispatcher = dispatcher;
			InteractivePipe.Active.Enabled = true;

			e.CrashDoc.LocalClient.OnInit += LocalClientOnOnInit;
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

		private void LocalClientOnOnInit(object sender, CrashClient.CrashInitArgs e)
		{
			e.CrashDoc.LocalClient.OnInit -= LocalClientOnOnInit;
			var dispatcher = e.CrashDoc.Dispatcher as EventDispatcher;
			if (dispatcher is null)
			{
				return;
			}

			e.CrashDoc.DocumentIsBusy = true;
			try
			{
				foreach (var change in e.Changes)
				{
					// TODO : Implement Async
					dispatcher.NotifyClientAsync(e.CrashDoc, change);
				}
				finally

				{
					e.CrashDoc.DocumentIsBusy = false;
				}
			}
			finally
			{
				e.CrashDoc.DocumentIsBusy = false;
			}
		}

		#endregion

		#region Rhino Plugin Overrides

		public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;

		protected override void OnShutdown()
		{
			foreach (var crashDoc in CrashDocRegistry.GetOpenDocuments())
			{
				CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);
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

			LoadCrashPlugins();

			return base.OnLoad(ref errorMessage);
		}

		#endregion
	}
}
