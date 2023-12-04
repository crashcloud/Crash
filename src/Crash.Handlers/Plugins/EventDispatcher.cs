﻿using Crash.Common.App;
using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Common.Logging;
using Crash.Handlers.InternalEvents;

using Microsoft.Extensions.Logging;

using Rhino;
using Rhino.DocObjects;

namespace Crash.Handlers.Plugins
{
	/// <summary>Handles all events that should be communicated through the server</summary>
	public sealed class EventDispatcher : IEventDispatcher
	{
		private readonly Dictionary<ChangeAction, List<IChangeCreateAction>> _createActions;
		private readonly Dictionary<string, List<IChangeRecieveAction>> _recieveActions;

		private EventWrapper _eventWrapper;

		/// <summary>Default Constructor</summary>
		public EventDispatcher()
		{
			_createActions = new Dictionary<ChangeAction, List<IChangeCreateAction>>();
			_recieveActions = new Dictionary<string, List<IChangeRecieveAction>>();
		}

		// TODO : How can we prevent the same events being subscribed multiple times?
		public async Task NotifyServerAsync(ChangeAction changeAction, object sender, EventArgs args, CrashDoc crashDoc)
		{
			if (!_createActions.TryGetValue(changeAction, out var actionChain))
			{
				if (args is not CrashViewArgs)
				{
					CrashLogger.Logger.LogDebug($"Could not find a CreateAction for {changeAction}");
				}

				return;
			}

			var crashArgs = new CreateRecieveArgs(changeAction, args, crashDoc);

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

			if (!changes.Any())
			{
				if (args is not CrashViewArgs)
				{
					CrashApp.Log("No changes created as a result", LogLevel.Trace);
				}

				return;
			}

			// Here we are essentially streaming?
			// We need to make sure this gets broken up better.
			await crashDoc.LocalClient.PushChangesAsync(changes);

#if DEBUG
			// This logic is a bit slow and I'd rather it wasn't compiled unless necessary
			CrashApp.Log($"{changes.Count()} Changes sent.", LogLevel.Trace);
			foreach (var change in changes)
			{
				CrashApp.Log($"Sent Change : {change.Type} | {change.Action} | {change.Id}", LogLevel.Trace);
			}
#endif
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
		public async Task NotifyClientAsync(CrashDoc doc, Change change)
		{
			if (!_recieveActions.TryGetValue(change.Type, out var recievers) ||
			    recievers is null)
			{
				CrashLogger.Logger.LogDebug($"Could not find a Recieve Action for {change.Type}, {change.Id}");
				return;
			}

			RegisterUser(doc, change);

			foreach (var recieveAction in recievers)
			{
				if (!recieveAction.CanRecieve(change))
				{
					continue;
				}

				CrashLogger.Logger
				           .LogDebug(
				                     $"Calling action {recieveAction.GetType().Name}, {change.Action}, {change.Type}, {change.Id}");

				await recieveAction.OnRecieveAsync(doc, change);
				return;
			}
		}

		private static void RegisterUser(CrashDoc doc, Change change)
		{
			if (!doc.Users.Add(change.Owner))
			{
				return;
			}

			CrashApp.Log($"User {change.Owner} registered.", LogLevel.Trace);
		}

		private async Task NotifyServerOfAddCrashObject(object sender, CrashObjectEventArgs args)
		{
			// TODO : Include Update?
			await NotifyServerAsync(ChangeAction.Add | ChangeAction.Temporary, sender,
			                        args, args.Doc);
			CrashApp.Log($"{nameof(NotifyServerOfAddCrashObject)} notified server.", LogLevel.Trace);
		}

		private async Task NotifyServerOfDeleteCrashObject(object sender, CrashObjectEventArgs crashArgs)
		{
			await NotifyServerAsync(ChangeAction.Remove, sender, crashArgs, crashArgs.Doc);
			CrashApp.Log($"{nameof(NotifyServerOfDeleteCrashObject)} notified server.", LogLevel.Trace);
		}

		private async Task NotifyServerOfTransformCrashObject(object sender, CrashTransformEventArgs crashArgs)
		{
			await NotifyServerAsync(ChangeAction.Transform, sender, crashArgs, crashArgs.Doc);
			CrashApp.Log($"{nameof(NotifyServerOfTransformCrashObject)} notified server.", LogLevel.Trace);
		}

		private async Task NotifyServerOfSelectCrashObjects(object sender, CrashSelectionEventArgs crashArgs)
		{
			await NotifyServerAsync(ChangeAction.Locked, sender, crashArgs, crashArgs.Doc);
			CrashApp.Log($"{nameof(NotifyServerOfSelectCrashObjects)} notified server.", LogLevel.Trace);
		}

		private async Task NotifyServerOfDeSelectCrashObjects(object sender, CrashSelectionEventArgs crashArgs)
		{
			await NotifyServerAsync(ChangeAction.Unlocked, sender, crashArgs, crashArgs.Doc);
			CrashApp.Log($"{nameof(NotifyServerOfDeSelectCrashObjects)} notified server.", LogLevel.Trace);
		}

		private async Task NotifyServerOfUpdateCrashObject(object sender, CrashUpdateArgs args)
		{
			await NotifyServerAsync(ChangeAction.Update, sender, args, args.Doc);
			CrashApp.Log($"{nameof(NotifyServerOfUpdateCrashObject)} notified server.", LogLevel.Trace);
		}

		private async Task NotifyServerOfCrashViewModified(object sender, CrashViewArgs crashArgs)
		{
			await NotifyServerAsync(ChangeAction.Add, sender, crashArgs, crashArgs.Doc);
		}

		// TODO : Use End?
		private void RhinoDocOnBeginOpenDocument(object sender, DocumentOpenEventArgs e)
		{
			if (!e.Merge)
			{
				return;
			}

			ObjectEnumeratorSettings settings = new() { HiddenObjects = true, LockedObjects = true };
			var objects = e.Document.Objects.GetObjectList(settings);
			foreach (var rhinoObject in objects)
			{
				rhinoObject.Geometry.UserDictionary.Clear();
			}
		}

		public void RegisterDefaultServerNotifiers()
		{
			_eventWrapper = new EventWrapper();
			_eventWrapper.AddCrashObject += NotifyServerOfAddCrashObject;
			_eventWrapper.DeleteCrashObject += NotifyServerOfDeleteCrashObject;
			_eventWrapper.TransformCrashObject += NotifyServerOfTransformCrashObject;
			_eventWrapper.SelectCrashObjects += NotifyServerOfSelectCrashObjects;
			_eventWrapper.DeSelectCrashObjects += NotifyServerOfDeSelectCrashObjects;
			_eventWrapper.UpdateCrashObject += NotifyServerOfUpdateCrashObject;
			_eventWrapper.CrashViewModified += NotifyServerOfCrashViewModified;
		}

		public void DeregisterDefaultServerCalls()
		{
			_eventWrapper.Dispose();
		}


		/// <summary>
		///     Registers all default server calls.
		///     If you need to create custom calls do this elsewhere.
		///     These calls cannot currently be overriden or switched off
		/// </summary>
		public void RegisterDefaultServerCalls(CrashDoc doc)
		{
			doc.LocalClient.OnPushChange += async change =>
				                                await NotifyClientAsync(doc, change);
			doc.LocalClient.OnPushChanges += async changes =>
				                                 await Task.WhenAll(changes.Select(c => NotifyClientAsync(doc, c)));
			doc.LocalClient.OnPushIdentical += async (ids, change) =>
				                                   await Task.WhenAll(ids.Select(c =>
					                                                      NotifyClientAsync(doc,
						                                                      new Change(change)
						                                                      {
							                                                      Id = c
						                                                      })));

			var initialInit = false;
			// OnInit is called on reconnect as well
			doc.LocalClient.OnInitializeChanges += async changes =>
			                                       {
				                                       if (initialInit)
				                                       {
					                                       return;
				                                       }

				                                       initialInit = true;

				                                       CrashLogger.Logger
				                                                  .LogDebug($"{nameof(doc.LocalClient.OnInitializeChanges)} - Initial : {initialInit}");
				                                       foreach (var change in changes)
				                                       {
					                                       await NotifyClientAsync(doc, change);
				                                       }
			                                       };
			;
		}
	}
}
