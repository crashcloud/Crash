﻿using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Common.Exceptions;
using Crash.Common.Logging;
using Crash.Handlers.Changes;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins.Initializers;
using Crash.Utils;

using Microsoft.Extensions.Logging;

using Rhino;
using Rhino.Display;

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

			RegisterDefaultEvents();
		}

		/// <summary>Registeres a Definition and all of the Create and recieve actions within</summary>
		public void RegisterDefinition(IChangeDefinition definition)
		{
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
		/// <param name="args">The EventArgs</param>
		/// <param name="doc">The associated RhinoDoc</param>
		public async Task NotifyDispatcher(ChangeAction changeAction, object sender, EventArgs args, RhinoDoc doc)
		{
			if (!_createActions.TryGetValue(changeAction, out var actionChain))
			{
				CrashLogger.Logger.LogDebug($"Could not find a CreateAction for {changeAction}");
				return;
			}

			var crashArgs = new CreateRecieveArgs(changeAction, args, doc);

			var crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			if (null == crashDoc)
			{
				return;
			}

			var changes = Enumerable.Empty<Change>();
			foreach (var action in actionChain)
			{
				if (!action.CanConvert(sender, crashArgs))
				{
					continue;
				}

				if (!action.TryConvert(sender, crashArgs, out changes))
				{
				}
			}

			var tasks = new List<Task>(changes.Count());
			foreach (var change in changes)
			{
				var message = $"Added Change {change.Action}, {change.Id}";

				switch (change.Action)
				{
					case ChangeAction.Add:
					case ChangeAction.Add | ChangeAction.Temporary:
						try
						{
							// TODO : This is not a good paradigm
							if (change.Type == new CameraChange().Type)
							{
								tasks.Add(crashDoc.LocalClient.CameraChangeAsync(change));
							}
							else
							{
								tasks.Add(crashDoc.LocalClient.AddAsync(change));
							}

							CrashLogger.Logger.LogDebug(message);
						}
						catch (OversizedChangeException oversized)
						{
							RhinoApp.WriteLine(oversized.Message);
							CrashLogger.Logger.LogDebug($"Failed to Add Change {change.Id}");
						}

						break;
					case ChangeAction.Remove:
						tasks.Add(crashDoc.LocalClient.DeleteAsync(change.Id));
						CrashLogger.Logger.LogDebug(message);
						break;

					case ChangeAction.Transform:
						// tasks.Add(Doc.LocalClient.TransformAsync(change));
						CrashLogger.Logger.LogDebug(message);
						break;

					case ChangeAction.Update:
						tasks.Add(crashDoc.LocalClient.UpdateAsync(change));
						CrashLogger.Logger.LogDebug(message);
						break;

					case ChangeAction.Locked:
						tasks.Add(crashDoc.LocalClient.LockAsync(change.Id));
						CrashLogger.Logger.LogDebug(message);
						break;
					case ChangeAction.Unlocked:
						tasks.Add(crashDoc.LocalClient.UnlockAsync(change.Id));
						CrashLogger.Logger.LogDebug(message);
						break;

					default:
						CrashLogger.Logger.LogDebug("ACTION NOT SUPPORTED");
						break;
				}
			}

			await Task.WhenAll(tasks);
		}

		/// <summary>
		///     Captures Calls from the Server
		/// </summary>
		/// <param name="Doc">The related Crash Doc</param>
		/// <param name="change">The Change from the Server</param>
		public async Task NotifyDispatcherAsync(CrashDoc Doc, Change change)
		{
			if (!_recieveActions.TryGetValue(change.Type, out var recievers) ||
			    recievers is null)
			{
				CrashLogger.Logger.LogDebug($"Could not find a Recieve Action for {change.Type}, {change.Id}");
				return;
			}

			await RegisterUserAsync(Doc, change);

			foreach (var action in recievers)
			{
				if (action.Action != change.Action)
				{
					continue;
				}

				CrashLogger.Logger
				           .LogDebug($"Calling action {action.GetType().Name}, {change.Action}, {change.Type}, {change.Id}");

				await action.OnRecieveAsync(Doc, change);
				return;
			}
		}

		private async Task RegisterUserAsync(CrashDoc doc, Change change)
		{
			doc.Users.Add(change.Owner);
		}

		private void RegisterDefaultEvents()
		{
			// Object Events
			RhinoDoc.AddRhinoObject += (sender, args) =>
			                           {
				                           //TODO: Is Init? Where is that checked for?
				                           var crashDoc = CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);
				                           if (crashDoc is not null)
				                           {
					                           if (crashDoc.CacheTable.IsInit)
					                           {
						                           return;
					                           }

					                           if (crashDoc.CacheTable.SomeoneIsDone)
					                           {
						                           return;
					                           }
				                           }

				                           var crashArgs = new CrashObjectEventArgs(args.TheObject);
				                           NotifyDispatcher(ChangeAction.Add | ChangeAction.Temporary, sender,
				                                            crashArgs, args.TheObject.Document);
			                           };

			RhinoDoc.UndeleteRhinoObject += (sender, args) =>
			                                {
				                                //TODO: Is Init? Where is that checked for?
				                                var crashDoc =
					                                CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);
				                                if (crashDoc is not null)
				                                {
					                                if (crashDoc.CacheTable.IsInit)
					                                {
						                                return;
					                                }

					                                if (crashDoc.CacheTable.SomeoneIsDone)
					                                {
						                                return;
					                                }
				                                }

				                                var crashArgs = new CrashObjectEventArgs(args.TheObject);
				                                NotifyDispatcher(ChangeAction.Add | ChangeAction.Temporary, sender,
				                                                 crashArgs, args.TheObject.Document);
			                                };

			RhinoDoc.DeleteRhinoObject += (sender, args) =>
			                              {
				                              var crashDoc =
					                              CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);
				                              if (crashDoc is not null)
				                              {
					                              if (crashDoc.CacheTable.IsInit)
					                              {
						                              return;
					                              }

					                              if (crashDoc.CacheTable.SomeoneIsDone)
					                              {
						                              return;
					                              }
				                              }

				                              args.TheObject.TryGetChangeId(out var changeId);
				                              if (changeId == Guid.Empty)
				                              {
					                              return;
				                              }

				                              var crashArgs = new CrashObjectEventArgs(args.TheObject, changeId);
				                              NotifyDispatcher(ChangeAction.Remove, sender, crashArgs,
				                                               args.TheObject.Document);
			                              };

			RhinoDoc.BeforeTransformObjects += (sender, args) =>
			                                   {
				                                   if (args.GripCount > 0)
				                                   {
					                                   return;
				                                   }

				                                   var crashArgs =
					                                   new CrashTransformEventArgs(args.Transform.ToCrash(),
						                                   args.Objects.Select(o => new CrashObject(o)),
						                                   args.ObjectsWillBeCopied);
				                                   var rhinoDoc = args.Objects
				                                                      .FirstOrDefault(o => o.Document is not null)
				                                                      .Document;
				                                   NotifyDispatcher(ChangeAction.Transform, sender, crashArgs,
				                                                    rhinoDoc);
			                                   };

			RhinoDoc.DeselectObjects += (sender, args) =>
			                            {
				                            var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
				                            if (crashDoc is not null)
				                            {
					                            if (crashDoc.CacheTable.IsInit)
					                            {
						                            return;
					                            }

					                            if (crashDoc.CacheTable.SomeoneIsDone)
					                            {
						                            return;
					                            }
				                            }

				                            var crashArgs =
					                            new CrashSelectionEventArgs(args.Selected,
					                                                        args.RhinoObjects
						                                                        .Select(o => new CrashObject(o)));
				                            NotifyDispatcher(ChangeAction.Unlocked, sender, crashArgs, args.Document);
			                            };

			RhinoDoc.DeselectAllObjects += (sender, args) =>
			                               {
				                               var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
				                               if (crashDoc is not null)
				                               {
					                               if (crashDoc.CacheTable.IsInit)
					                               {
						                               return;
					                               }

					                               if (crashDoc.CacheTable.SomeoneIsDone)
					                               {
						                               return;
					                               }
				                               }

				                               var crashArgs = new CrashSelectionEventArgs();
				                               NotifyDispatcher(ChangeAction.Unlocked, sender, crashArgs,
				                                                args.Document);
			                               };
			RhinoDoc.SelectObjects += (sender, args) =>
			                          {
				                          var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
				                          if (crashDoc is not null)
				                          {
					                          if (crashDoc.CacheTable.IsInit)
					                          {
						                          return;
					                          }

					                          if (crashDoc.CacheTable.SomeoneIsDone)
					                          {
						                          return;
					                          }
				                          }

				                          var crashArgs =
					                          new CrashSelectionEventArgs(args.Selected,
					                                                      args.RhinoObjects
					                                                          .Select(o => new CrashObject(o)));
				                          NotifyDispatcher(ChangeAction.Locked, sender, crashArgs, args.Document);
			                          };

			RhinoDoc.ModifyObjectAttributes += (sender, args) =>
			                                   {
				                                   // TODO : Create Wrapper
				                                   NotifyDispatcher(ChangeAction.Update, sender, args, args.Document);
			                                   };

			RhinoDoc.UserStringChanged += (sender, args) =>
			                              {
				                              // TODO : Create Wrapper
				                              NotifyDispatcher(ChangeAction.Update, sender, args, args.Document);
			                              };

			// Doc Events
			// RhinoDoc.UnitsChangedWithScaling += 

			// View Events
			RhinoView.Modified += (sender, args) =>
			                      {
				                      var crashArgs = new CrashViewArgs(args.View);
				                      NotifyDispatcher(ChangeAction.Add, sender, crashArgs, args.View.Document);
			                      };
		}

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
		/// <summary>
		///     Registers all default server calls.
		///     If you need to create custom calls do this elsewhere.
		///     These calls cannot currently be overriden or switched off
		/// </summary>
		public void RegisterDefaultServerCalls(CrashDoc Doc)
		{
			Doc.LocalClient.OnAdd += async change => await NotifyDispatcherAsync(Doc, change);
			Doc.LocalClient.OnDelete += async changeGuid => await NotifyDispatcherAsync(Doc, DeleteChange(changeGuid));

			Doc.LocalClient.OnLock += async (name, changeGuid) =>
				                          await NotifyDispatcherAsync(Doc, SelectChange(changeGuid, name));
			Doc.LocalClient.OnUnlock += async (name, changeGuid) =>
				                            await NotifyDispatcherAsync(Doc, UnSelectChange(changeGuid, name));

			Doc.LocalClient.OnDone += async name => await NotifyDispatcherAsync(Doc, DoneChange(name));

			Doc.LocalClient.OnCameraChange += async change => await NotifyDispatcherAsync(Doc, change);

			var initialInit = false;

			// OnInit is called on reconnect as well?
			Doc.LocalClient.OnInitialize += async changes =>
			                                {
				                                if (initialInit)
				                                {
					                                return;
				                                }

				                                initialInit = true;

				                                CrashLogger.Logger
				                                           .LogDebug($"{nameof(Doc.LocalClient.OnInitialize)} - Initial : {initialInit}");
				                                foreach (var change in changes)
				                                {
					                                await NotifyDispatcherAsync(Doc, change);
				                                }
			                                };
			;
		}
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates

		private static Change DoneChange(string name)
		{
			return new Change
			       {
				       Owner = name,
				       Action = ChangeAction.None,
				       Type = new DoneDefinition().ChangeName,
				       Id = Guid.NewGuid(),
				       Stamp = DateTime.UtcNow
			       };
		}

		private static Change DeleteChange(Guid id)
		{
			return new Change
			       {
				       Id = id,
				       Type = GeometryChange.ChangeType,
				       Action = ChangeAction.Remove,
				       Stamp = DateTime.Now,
				       Payload = null
			       };
		}

		private static Change SelectChange(Guid id, string name)
		{
			return new Change
			       {
				       Id = id,
				       Owner = name,
				       Type = GeometryChange.ChangeType,
				       Action = ChangeAction.Locked,
				       Stamp = DateTime.Now,
				       Payload = null
			       };
		}

		private static Change UnSelectChange(Guid id, string name)
		{
			return new Change
			       {
				       Id = id,
				       Owner = name,
				       Type = GeometryChange.ChangeType,
				       Action = ChangeAction.Unlocked,
				       Stamp = DateTime.Now,
				       Payload = null
			       };
		}
	}
}
