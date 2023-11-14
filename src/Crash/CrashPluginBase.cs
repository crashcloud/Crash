using Crash.Common.Communications;
using Crash.Handlers.Plugins;

using Rhino.PlugIns;

namespace Crash
{
	/// <summary>
	///     All CrashPlugins should inherit from this base
	/// </summary>
	public abstract class CrashPluginBase : PlugIn
	{
		private readonly Stack<IChangeDefinition> _changes;
		internal EventDispatcher Dispatcher;

		protected CrashPluginBase()
		{
			_changes = new Stack<IChangeDefinition>();
			CrashClient.OnInit += CrashClient_OnInit;
		}

		#region Rhino Plugin Overrides

		public sealed override PlugInLoadTime LoadTime
			=> this is CrashPlugin ? PlugInLoadTime.AtStartup : PlugInLoadTime.WhenNeeded;

		#endregion

		private void CrashClient_OnInit(object sender, CrashClient.CrashInitArgs e)
		{
			RhinoApp.WriteLine("Loading Changes ...");

			if (Dispatcher is null)
			{
				Dispatcher = new EventDispatcher();
				Dispatcher.RegisterDefaultServerCalls(e.CrashDoc);
			}

			if (Dispatcher is not null)
			{
				RegisterDefinitions();

				e.CrashDoc.IsInit = true;

				foreach (var change in e.Changes)
				{
					Dispatcher.NotifyClientAsync(e.CrashDoc, change);
				}

				e.CrashDoc.IsInit = false;
			}

			RhinoDoc.ActiveDoc.Views.Redraw();
		}

		protected virtual void RegisterChangeSchema(IChangeDefinition changeDefinition)
		{
			InteractivePipe.RegisterChangeDefinition(changeDefinition);
			_changes.Push(changeDefinition);
		}

		private void RegisterDefinitions()
		{
			while (_changes.Count > 0)
			{
				var changeDefinition = _changes.Pop();
				Dispatcher.RegisterDefinition(changeDefinition);
			}
		}
	}
}
