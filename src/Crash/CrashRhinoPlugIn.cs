using System.Reflection;

using Crash.Common.Communications;
using Crash.Common.Events;
using Crash.Handlers;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Camera;
using Crash.Handlers.Plugins.Geometry;
using Crash.Handlers.Plugins.Initializers;
using Crash.UI.ExceptionsAndErrors;
using Crash.UI.UsersView;

using Eto.Forms;

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
		private static readonly Stack<IChangeDefinition> Changes;

		#region Crash Plugins

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
			var assembly = Assembly.LoadFrom(crashAssembly);
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
			LoadCrashPlugins();
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
		}

		private BadChangePipeline badPipe;

		private void RegisterExceptions(CrashClient client)
		{
			client.OnServerClosed += async (sender, args) =>
			                         {
				                         RhinoApp.InvokeOnUiThread(() =>
				                                                   {
					                                                   MessageBox
						                                                   .Show("The server connection has been lost. Nothing can currently be done about this. Your model will be closed.",
								                                                    MessageBoxButtons.OK);

																	   UsersForm.CloseActiveForm();
																   });

				                         await CrashDocRegistry
					                         .DisposeOfDocumentAsync(args.CrashDoc);
			                         };

			client.OnPushChangeFailed += (sender, args) =>
			                             {
				                             badPipe = new BadChangePipeline(args);
				                             RhinoApp.InvokeOnUiThread(() =>
				                                                       {
					                                                       MessageBox
						                                                       .Show("A change failed to send. Any changes highlighted in red will not be communicated",
								                                                        MessageBoxButtons.OK);
				                                                       });
			                             };
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
