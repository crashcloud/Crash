using Crash.Common.App;
using Crash.Common.Changes;
using Crash.Common.Collections;
using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Common.Logging;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.InternalEvents.Wrapping;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace Crash.Handlers.Plugins
{
	/// <summary>Handles all events that should be communicated through the server</summary>
	public sealed class EventDispatcher : IEventDispatcher
	{
		private readonly CrashDoc _crashDoc;
		private readonly Dictionary<ChangeAction, List<IChangeCreateAction>> _createActions;
		private readonly Dictionary<string, List<IChangeRecieveAction>> _recieveActions;
		private EventWrapper _eventWrapper;

		/// <summary>Default Constructor</summary>
		public EventDispatcher(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
			_createActions = new Dictionary<ChangeAction, List<IChangeCreateAction>>();
			_recieveActions = new Dictionary<string, List<IChangeRecieveAction>>();
		}

		// TODO : How can we prevent the same events being subscribed multiple times?

		public async Task NotifyServerAsync(IEnumerable<Change> changes)
		{
			try
			{
				await _crashDoc.LocalClient.SendChangesToServerAsync(changes.ToAsyncEnumerable());
			}
			catch (Exception ex)
			{

			}
		}

		internal async Task<List<Change>> TryGetChangeFromEvent(ChangeAction changeAction, object sender, EventArgs args)
		{
			if (!_createActions.TryGetValue(changeAction, out var actionChain))
			{
				if (args is not CrashViewArgs)
				{
					CrashLogger.Logger.LogDebug($"Could not find a CreateAction for {changeAction}");
				}

				return new List<Change>();
			}

			var crashArgs = new CreateRecieveArgs(changeAction, args, _crashDoc);

			var changes = Enumerable.Empty<Change>();
			foreach (var action in actionChain)
			{
				if (!action.CanConvert(sender, crashArgs))
				{
					continue;
				}

				if (action.TryConvert(sender, crashArgs, out changes))
				{
					break;
				}
			}
			changes ??= new List<Change>();

			return changes.ToList();
		}

		/// <summary>Registers a Definition and all of the Create and recieve actions within</summary>
		public void RegisterDefinition(IChangeDefinition definition)
		{
			// TODO : Test this! Are we sure this stack stuff is working?
			foreach (var create in definition.CreateActions)
			{
				if (_createActions.TryGetValue(create.Action, out var actions))
				{
					actions.Add(create);
				}
				else
				{
					_createActions.Add(create.Action, new List<IChangeCreateAction> { create });
				}
			}

			foreach (var recieve in definition.RecieveActions)
			{
				if (_recieveActions.TryGetValue(definition.ChangeName, out var recievers))
				{
					recievers.Add(recieve);
				}
				else
				{
					_recieveActions.Add(definition.ChangeName, new List<IChangeRecieveAction> { recieve });
				}
			}
		}

		/// <summary>
		///     Captures Calls from the Server
		/// </summary>
		/// <param name="doc">The related Crash Doc</param>
		/// <param name="change">The Change from the Server</param>
		public async Task NotifyClientAsync(IEnumerable<Change> changes)
		{
			foreach (var change in changes)
			{

				if (!_recieveActions.TryGetValue(change.Type, out var recievers) ||
				recievers is null)
				{
					CrashLogger.Logger.LogDebug($"Could not find a Recieve Action for {change.Type}, {change.Id}");
					return;
				}

				RegisterUser(_crashDoc, change);

				foreach (var recieveAction in recievers)
				{
					if (!recieveAction.CanRecieve(change)) continue;

					CrashLogger.Logger
						   .LogDebug(
									 $"Calling action {recieveAction.GetType().Name}, {change.Action}, {change.Type}, {change.Id}");

					await recieveAction.OnRecieveAsync(_crashDoc, change);
					return;
				}
			}
		}

		private static void RegisterUser(CrashDoc doc, Change change)
		{
			if (!doc.Users.Add(change.Owner)) return;
			CrashApp.Log($"User {change.Owner} registered.", LogLevel.Trace);
		}

		private async Task NotifyServerOfAddCrashObject(object? sender, CrashObjectEventArgs args)
		{
			await NotifyServerAsync(await TryGetChangeFromEvent(ChangeAction.Add | ChangeAction.Temporary, sender, args));
		}

		private async Task NotifyServerOfDeleteCrashObject(object? sender, CrashObjectEventArgs crashArgs)
		{
			await NotifyServerAsync(await TryGetChangeFromEvent(ChangeAction.Remove, sender, crashArgs));
		}

		private async Task NotifyServerOfTransformCrashObject(object? sender, CrashTransformEventArgs crashArgs)
		{
			await NotifyServerAsync(await TryGetChangeFromEvent(ChangeAction.Transform, sender, crashArgs));
		}

		private async Task NotifyServerOfSelectCrashObjects(object? sender, CrashSelectionEventArgs crashArgs)
		{
			await NotifyServerAsync(await TryGetChangeFromEvent(ChangeAction.Locked, sender, crashArgs));
		}

		private async Task NotifyServerOfDeSelectCrashObjects(object? sender, CrashSelectionEventArgs crashArgs)
		{
			await NotifyServerAsync(await TryGetChangeFromEvent(ChangeAction.Unlocked, sender, crashArgs));
		}

		private async Task NotifyServerOfUpdateCrashObject(object? sender, CrashUpdateArgs args)
		{
			await NotifyServerAsync(await TryGetChangeFromEvent(ChangeAction.Update, sender, args));
		}

		private async Task NotifyServerOfCrashLayerModified(object? sender, CrashLayerArgs args)
		{
			await NotifyServerAsync(await TryGetChangeFromEvent(args.Action, sender, args));
		}

		private async Task NotifyServerOfCrashViewModified(object? sender, CrashViewArgs crashArgs)
		{
			await NotifyServerAsync(await TryGetChangeFromEvent(ChangeAction.Add, sender, crashArgs));
		}

		/// <summary>Registers the default server notifiers</summary>
		public void RegisterDefaultServerNotifiers()
		{
			_eventWrapper = new EventWrapper(_crashDoc);
			_eventWrapper.AddCrashObject += NotifyServerOfAddCrashObject;
			_eventWrapper.DeleteCrashObject += NotifyServerOfDeleteCrashObject;
			_eventWrapper.TransformCrashObject += NotifyServerOfTransformCrashObject;
			_eventWrapper.SelectCrashObjects += NotifyServerOfSelectCrashObjects;
			_eventWrapper.DeSelectCrashObjects += NotifyServerOfDeSelectCrashObjects;
			_eventWrapper.UpdateCrashObject += NotifyServerOfUpdateCrashObject;
			_eventWrapper.LayerModified += NotifyServerOfCrashLayerModified;
			_eventWrapper.CrashViewModified += NotifyServerOfCrashViewModified;
		}

		public void ClearUndoRedoQueue()
		{
			_eventWrapper.ClearUndoRedoQueue();
		}

		/// <summary>Deregisters server notifiers.</summary>
		public void DeregisterDefaultServerCalls()
		{
			_eventWrapper.Dispose();
		}

	}
}
