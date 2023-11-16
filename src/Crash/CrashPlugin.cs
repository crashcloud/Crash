using Crash.Handlers;
using Crash.Handlers.Plugins.Camera;
using Crash.Handlers.Plugins.Geometry;
using Crash.Handlers.Plugins.Initializers;
using Crash.Handlers.Server;

using Rhino.PlugIns;

namespace Crash
{
	///<summary>The crash plugin for multi user rhino collaboration</summary>
	public sealed class CrashPlugin : CrashPluginBase, IDisposable
	{
		private const string _id = "53CB2393-C71F-4079-9CEC-97464FF9D14E";

		public CrashPlugin()
		{
			Instance = this;

			// Register the Defaults!
			RegisterChangeSchema(new GeometryChangeDefinition());
			RegisterChangeSchema(new CameraChangeDefinition());
			RegisterChangeSchema(new DoneDefinition());
		}

		public static Guid PluginId => new(_id);

		protected override string LocalPlugInName => "Crash";

		///<summary>Gets the only instance of the CrashPlugin plug-in.</summary>
		public static CrashPlugin Instance { get; private set; }

		public void Dispose()
		{
			// Dispose of Server connections etc. gracefully
		}

		protected override LoadReturnCode OnLoad(ref string errorMessage)
		{
			// Add feature flags as advanced settings here!
			InteractivePipe.Active = new InteractivePipe { Enabled = false };

			// TODO : Move to JoinServer command
			RhinoApp.Idle += DownloadServer;

			return base.OnLoad(ref errorMessage);
		}

		private void DownloadServer(object sender, EventArgs e)
		{
			RhinoApp.Idle -= DownloadServer;
			if (!ServerInstaller.ServerExecutableExists)
			{
				ServerInstaller.EnsureServerExecutableExists();
			}
		}

		private void LoadCrashPlugins()
		{
			IEnumerable<Guid> pluginIds = GetInstalledPlugIns().Keys;
			foreach (var pluginId in pluginIds)
			{
				var plugin = Find(pluginId);
				if (plugin is not CrashPluginBase pluginBase)
				{
					continue;
				}

				LoadPlugIn(pluginId);
			}
		}

		protected override void OnShutdown()
		{
			foreach (var crashDoc in CrashDocRegistry.GetOpenDocuments())
			{
				crashDoc?.LocalServer?.Stop();
				crashDoc?.LocalClient?.StopAsync();
			}
		}
	}
}
