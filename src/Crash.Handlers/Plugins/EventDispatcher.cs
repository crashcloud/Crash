using Crash.Common.App;
using Crash.Common.Document;
using Crash.Common.Logging;
using Crash.Handlers.InternalEvents;
using Crash.Utils;

using Microsoft.Extensions.Logging;

using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

namespace Crash.Handlers.Plugins
{
	/// <summary>Handles all events that should be communicated through the server</summary>
	public sealed class EventDispatcher
	{
		/// <summary>The current Dispatcher. This is used across Docs</summary>
		public static EventDispatcher Instance;

		private readonly Dictionary<ChangeAction, List<IChangeCreateAction>> _createActions;
		private readonly Dictionary<string, List<IChangeRecieveAction>> _recieveActions;

		/// <summary>Default Constructor</summary>
		public EventDispatcher()
		{
			Instance = this;

			_createActions = new Dictionary<ChangeAction, List<IChangeCreateAction>>();
			_recieveActions = new Dictionary<string, List<IChangeRecieveAction>>();
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

		// TODO : How can we prevent the same events being subscribed multiple times?
		/// <summary>
		///     Notifies the Dispatcher of any Events that should notify the server
		///     Avoid Subscribing to events and pinging the server yourself
		///     Wrap any related events with this method.
		/// </summary>
		/// <param name="changeAction">The ChangeAction</param>
		/// <param name="sender">The sender of the Event</param>
		/// <param name="args">
		///     The EventArgs
		///     <-/param>
		///         <param name="doc">The associated RhinoDoc</param>
		public async Task NotifyServerAsync(ChangeAction changeAction, object sender, EventArgs args, RhinoDoc doc)
		{
			if (!_createActions.TryGetValue(changeAction, out var actionChain))
			{
				CrashLogger.Logger.LogDebug($"Could not find a CreateAction for {changeAction}");
				return;
			}

			CrashApp.Log("Create Action Found", LogLevel.Trace);

			var crashArgs = new CreateRecieveArgs(changeAction, args, doc);

			var crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			if (null == crashDoc)
			{
				CrashApp.Log("CrashDoc was null", LogLevel.Trace);
				return;
			}

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
				CrashApp.Log("No changes created as a result", LogLevel.Trace);
				return;
			}

			// Here we are essentially streaming?
			// We need to make sure this gets broken up better.
			await crashDoc.LocalClient.PushChangesAsync(changes);
			CrashApp.Log($"{changes.Count()} Changes sent.", LogLevel.Trace);
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

			await RegisterUserAsync(doc, change);

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

		private async Task RegisterUserAsync(CrashDoc doc, Change change)
		{
			if (!doc.Users.Add(change.Owner))
			{
				return;
			}

			CrashApp.Log($"User {change.Owner} registered.", LogLevel.Trace);
		}

		private void AddRhinoObject(object sender, RhinoObjectEventArgs args)
		{
			CrashApp.Log($"{nameof(AddRhinoObject)} event fired.", LogLevel.Trace);

			var crashDoc =
				CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);
			if (crashDoc is null)
			{
				return;
			}

			if (crashDoc.IsInit ||
			    crashDoc.SomeoneIsDone ||
			    crashDoc.IsTransformActive)
			{
				return;
			}

			if (args.TheObject.IsActiveChange(crashDoc))
			{
				return;
			}

			var crashArgs = new CrashObjectEventArgs(args.TheObject);
			NotifyServerAsync(ChangeAction.Add | ChangeAction.Temporary, sender, 
				crashArgs, args.TheObject.Document);

			CrashApp.Log($"{nameof(AddRhinoObject)} notified server.", LogLevel.Trace);
		}

		private void DeleteRhinoObject(object sender, RhinoObjectEventArgs args)
		{
			CrashApp.Log($"{nameof(DeleteRhinoObject)} event fired.", LogLevel.Trace);

			var crashDoc =
				CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);
			if (crashDoc is null)
			{
				return;
			}

			if (crashDoc.IsInit ||
			    crashDoc.SomeoneIsDone ||
			    crashDoc.IsTransformActive)
			{
				return;
			}

			// Add check for IS Transforming

			args.TheObject.TryGetChangeId(out var changeId);
			if (changeId == Guid.Empty)
			{
				return;
			}

			var crashArgs = new CrashObjectEventArgs(args.TheObject, changeId);
			NotifyServerAsync(ChangeAction.Remove, sender, crashArgs,
			                  args.TheObject.Document);
			crashDoc.RealisedChangeTable.RemoveChange(changeId);

			CrashApp.Log($"{nameof(DeleteRhinoObject)} notified server.", LogLevel.Trace);
		}

		private void TransformRhinoObject(object sender, RhinoTransformObjectsEventArgs args)
		{
			if (args.GripCount > 0)
			{
				return;
			}

			CrashApp.Log($"{nameof(TransformRhinoObject)} event fired.", LogLevel.Trace);

			var rhinoDoc = args.Objects
			                   .FirstOrDefault(o => o.Document is not null)
			                   .Document;

			var crashDoc = CrashDocRegistry.GetRelatedDocument(rhinoDoc);
			if (crashDoc is null)
			{
				return;
			}

			if (crashDoc.IsInit ||
			    crashDoc.SomeoneIsDone)
			{
				return;
			}

			crashDoc.IsTransformActive = true;

			var crashArgs =
				new CrashTransformEventArgs(args.Transform.ToCrash(),
				                            args.Objects.Select(o => new CrashObject(o)),
				                            args.ObjectsWillBeCopied);

			NotifyServerAsync(ChangeAction.Transform, sender, crashArgs, rhinoDoc);

			CrashApp.Log($"{nameof(TransformRhinoObject)} notified server.", LogLevel.Trace);
		}

		private void DeselectRhinoObjects(object sender, RhinoObjectSelectionEventArgs args)
		{
			CrashApp.Log($"{nameof(DeselectRhinoObjects)} event fired.", LogLevel.Trace);

			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);

			if (crashDoc is null)
			{
				return;
			}

			if (crashDoc.IsInit ||
			    crashDoc.SomeoneIsDone)
			{
				return;
			}


			foreach (var rhinoObject in args.RhinoObjects)
			{
				if (!rhinoObject.TryGetChangeId(out var changeId))
				{
					continue;
				}

				crashDoc.RealisedChangeTable.RemoveSelected(changeId);
			}

			var crashArgs =
				CrashSelectionEventArgs.CreateDeSelectionEvent(args.RhinoObjects
				                                                   .Select(o => new CrashObject(o)));
			NotifyServerAsync(ChangeAction.Unlocked, sender, crashArgs, args.Document);

			CrashApp.Log($"{nameof(DeselectRhinoObjects)} notified server.", LogLevel.Trace);
		}

		private void DeselectAllRhinoObjects(object sender, RhinoDeselectAllObjectsEventArgs args)
		{
			CrashApp.Log($"{nameof(DeselectAllRhinoObjects)} event fired.", LogLevel.Trace);

			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);

			if (crashDoc is null)
			{
				return;
			}

			if (crashDoc.IsInit ||
			    crashDoc.SomeoneIsDone)
			{
				return;
			}

			var currentlySelected = crashDoc.RealisedChangeTable.GetSelected();
			var crashArgs = CrashSelectionEventArgs.CreateDeSelectionEvent(
			                                                               currentlySelected
				                                                               .Select(cs =>
					                                                               new CrashObject(cs,
						                                                               Guid.Empty)));
			NotifyServerAsync(ChangeAction.Unlocked, sender, crashArgs,
			                  args.Document);

			crashDoc.RealisedChangeTable.ClearSelected();

			CrashApp.Log($"{nameof(DeselectAllRhinoObjects)} notified server.", LogLevel.Trace);
		}

		private void SelectRhinoObjects(object sender, RhinoObjectSelectionEventArgs args)
		{
			CrashApp.Log($"{nameof(SelectRhinoObjects)} event fired.", LogLevel.Trace);

			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);

			if (crashDoc is null)
			{
				return;
			}

			if (crashDoc.IsInit ||
			    crashDoc.SomeoneIsDone)
			{
				return;
			}

			foreach (var rhinoObject in args.RhinoObjects)
			{
				if (!rhinoObject.TryGetChangeId(out var changeId))
				{
					continue;
				}

				crashDoc.RealisedChangeTable.AddSelected(changeId);
			}

			var crashArgs = CrashSelectionEventArgs.CreateSelectionEvent(args.RhinoObjects
			                                                                 .Select(o => new CrashObject(o)));
			NotifyServerAsync(ChangeAction.Locked, sender, crashArgs, args.Document);

			CrashApp.Log($"{nameof(SelectRhinoObjects)} notified server.", LogLevel.Trace);
		}

		private void ModifyRhinoObjectAttributes(object sender, RhinoModifyObjectAttributesEventArgs args)
		{
			CrashApp.Log($"{nameof(ModifyRhinoObjectAttributes)} event fired.", LogLevel.Trace);

			// TODO : Create Wrapper
			NotifyServerAsync(ChangeAction.Update, sender, args, args.Document);

			CrashApp.Log($"{nameof(ModifyRhinoObjectAttributes)} notified server.", LogLevel.Trace);
		}

		private void UserStringChanged(object sender, RhinoDoc.UserStringChangedArgs args)
		{
			CrashApp.Log($"{nameof(UserStringChanged)} event fired.", LogLevel.Trace);

			// TODO : Create Wrapper
			NotifyServerAsync(ChangeAction.Update, sender, args, args.Document);

			CrashApp.Log($"{nameof(UserStringChanged)} notified server.", LogLevel.Trace);
		}

		private void RhinoViewModified(object sender, ViewEventArgs args)
		{
			CrashApp.Log($"{nameof(RhinoViewModified)} event fired.", LogLevel.Trace);

			var crashArgs = new CrashViewArgs(args.View);
			NotifyServerAsync(ChangeAction.Add, sender, crashArgs, args.View.Document);

			CrashApp.Log($"{nameof(RhinoViewModified)} notified server.", LogLevel.Trace);
		}

		public void RegisterDefaultEvents()
		{
			// Object Events
			RhinoDoc.AddRhinoObject += AddRhinoObject;
			RhinoDoc.UndeleteRhinoObject += AddRhinoObject;
			RhinoDoc.DeleteRhinoObject += DeleteRhinoObject;
			RhinoDoc.BeforeTransformObjects += TransformRhinoObject;
			RhinoDoc.DeselectObjects += DeselectRhinoObjects;
			RhinoDoc.DeselectAllObjects += DeselectAllRhinoObjects;
			RhinoDoc.SelectObjects += SelectRhinoObjects;
			RhinoDoc.ModifyObjectAttributes += ModifyRhinoObjectAttributes;
			RhinoDoc.UserStringChanged += UserStringChanged;

			// View Events
			RhinoView.Modified += RhinoViewModified;
		}

		public void DeRegisterDefaultEvents()
		{
			// Object Events
			RhinoDoc.AddRhinoObject -= AddRhinoObject;
			RhinoDoc.UndeleteRhinoObject -= AddRhinoObject;
			RhinoDoc.DeleteRhinoObject -= DeleteRhinoObject;
			RhinoDoc.BeforeTransformObjects -= TransformRhinoObject;
			RhinoDoc.DeselectObjects -= DeselectRhinoObjects;
			RhinoDoc.DeselectAllObjects -= DeselectAllRhinoObjects;
			RhinoDoc.SelectObjects -= SelectRhinoObjects;
			RhinoDoc.ModifyObjectAttributes -= ModifyRhinoObjectAttributes;
			RhinoDoc.UserStringChanged -= UserStringChanged;

			// View Events
			RhinoView.Modified -= RhinoViewModified;
		}

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
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
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
	}
}
