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
	public sealed class CrashPlugin : CrashPluginBase
	{
		#region Crash Plugins

		private void LoadCrashPlugins()
		{
			IEnumerable<Guid> pluginIds = GetInstalledPlugIns().Keys;
			foreach (var pluginId in pluginIds)
			{
				var plugin = Find(pluginId);

				// Skip non Crash Plugin
				if (plugin is not CrashPluginBase pluginBase ||
				    plugin is CrashPlugin)
				{
					continue;
				}

				LoadPlugIn(pluginId);

				foreach (var change in pluginBase.Changes)
				{
					RegisterChangeSchema(change);
				}
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
			if (dispatcher is not null)
			{
				e.CrashDoc.DocumentIsBusy = true;
				try
				{
					// TODO : Handle Async!
					foreach (var change in e.Changes)
					{
						dispatcher.NotifyClientAsync(e.CrashDoc, change);
					}
				}
				finally
				{
					e.CrashDoc.DocumentIsBusy = false;
				}
			}
		}

		#endregion

		#region Rhino Plugin Overrides

		protected override void OnShutdown()
		{
			foreach (var crashDoc in CrashDocRegistry.GetOpenDocuments())
			{
				CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);
			}
		}

		public CrashPlugin()
		{
			Instance = this;

			// Register the Defaults!
			RegisterChangeSchema(new GeometryChangeDefinition());
			RegisterChangeSchema(new CameraChangeDefinition());
			RegisterChangeSchema(new DoneDefinition());

			CrashDocRegistry.DocumentRegistered += CrashDocRegistryOnDocumentRegistered;
			CrashDocRegistry.DocumentDisposed += CrashDocRegistryOnDocumentDisposed;
		}

		public static Guid PluginId => new(CrashPluginId);

		protected override string LocalPlugInName => "Crash";

		///<summary>Gets the only instance of the CrashPlugin plug-in.</summary>
		public static CrashPlugin Instance { get; private set; }

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
