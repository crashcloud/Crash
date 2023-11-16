using Crash.Common.Communications;
using Crash.Common.Events;
using Crash.Handlers;
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

		// TODO : Dispatcher needs to get disposed and recreated
		private EventDispatcher Dispatcher;

		protected CrashPluginBase()
		{
			_changes = new Stack<IChangeDefinition>();

			// TODO : All of this stuff needs to be moved outside a reused constructor
			CrashDocRegistry.DocumentRegistered += CrashDocRegistryOnDocumentRegistered;
			CrashDocRegistry.DocumentDisposed += CrashDocRegistryOnDocumentDisposed;
		}

		#region Rhino Plugin Overrides

		public sealed override PlugInLoadTime LoadTime
			=> this is CrashPlugin ? PlugInLoadTime.AtStartup : PlugInLoadTime.WhenNeeded;

		#endregion

		private void CrashDocRegistryOnDocumentDisposed(object sender, CrashEventArgs e)
		{
			Dispatcher = null;
			InteractivePipe.Active.Enabled = false;
		}

		private void CrashDocRegistryOnDocumentRegistered(object sender, CrashEventArgs e)
		{
			Dispatcher = new EventDispatcher();
			RegisterDefinitions();
			Dispatcher.RegisterDefaultServerCalls(e.CrashDoc);
			InteractivePipe.Active.Enabled = true;

			e.CrashDoc.LocalClient.OnInit += LocalClientOnOnInit;
		}

		private void LocalClientOnOnInit(object sender, CrashClient.CrashInitArgs e)
		{
			e.CrashDoc.LocalClient.OnInit -= LocalClientOnOnInit;

			if (Dispatcher is not null)
			{
				e.CrashDoc.IsInit = true;

				// TODO : Handle Async!
				foreach (var change in e.Changes)
				{
					Dispatcher.NotifyClientAsync(e.CrashDoc, change);
				}

				e.CrashDoc.IsInit = false;
			}
		}

		protected virtual void RegisterChangeSchema(IChangeDefinition changeDefinition)
		{
			InteractivePipe.RegisterChangeDefinition(changeDefinition);
			_changes.Push(changeDefinition);
		}

		private void RegisterDefinitions()
		{
			var changeEnuner = _changes.GetEnumerator();

			while (changeEnuner.MoveNext())
			{
				var changeDefinition = changeEnuner.Current;
				Dispatcher.RegisterDefinition(changeDefinition);
			}
		}
	}
}
