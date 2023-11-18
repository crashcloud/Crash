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

		private EventDispatcher _dispatcher;

		private void CrashDocRegistryOnDocumentDisposed(object sender, CrashEventArgs e)
		{
			_dispatcher = null;
			InteractivePipe.Active.Enabled = false;
		}

		private void CrashDocRegistryOnDocumentRegistered(object sender, CrashEventArgs e)
		{
			_dispatcher = new EventDispatcher();
			RegisterDefinitions();
			_dispatcher.RegisterDefaultServerCalls(e.CrashDoc);
			InteractivePipe.Active.Enabled = true;

			e.CrashDoc.LocalClient.OnInit += LocalClientOnOnInit;
		}

		private void RegisterDefinitions()
		{
			var changeEnuner = Changes.GetEnumerator();

			while (changeEnuner.MoveNext())
			{
				var changeDefinition = changeEnuner.Current;
				_dispatcher.RegisterDefinition(changeDefinition);
			}
		}

		private void LocalClientOnOnInit(object sender, CrashClient.CrashInitArgs e)
		{
			e.CrashDoc.LocalClient.OnInit -= LocalClientOnOnInit;

			if (_dispatcher is not null)
			{
				e.CrashDoc.IsInit = true;

				// TODO : Handle Async!
				foreach (var change in e.Changes)
				{
					_dispatcher.NotifyClientAsync(e.CrashDoc, change);
				}

				e.CrashDoc.IsInit = false;
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
