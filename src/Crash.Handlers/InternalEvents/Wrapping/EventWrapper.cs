using Crash.Common.App;
using Crash.Common.Document;

using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;

namespace Crash.Handlers.InternalEvents.Wrapping
{
	internal class EventWrapper : IDisposable
	{
		private readonly CrashDoc ContextDocument;

		private readonly List<RhinoObject> Created;
		private readonly List<RhinoObject> Deleted;
		private readonly Queue<IUndoRedoCache> EventQueue;
		private readonly Stack<IUndoRedoCache> RedoRecords;
		private readonly Dictionary<Guid, bool> SelectionQueue;
		private readonly Stack<IUndoRedoCache> UndoRecords;

		internal EventWrapper(CrashDoc _crashDoc)
		{
			ContextDocument = _crashDoc;
			UndoRecords = new Stack<IUndoRedoCache>();
			RedoRecords = new Stack<IUndoRedoCache>();
			EventQueue = new Queue<IUndoRedoCache>();
			SelectionQueue = new Dictionary<Guid, bool>();
			Created = new List<RhinoObject>();
			Deleted = new List<RhinoObject>();
			RegisterDefaultEvents();
		}

		private bool CopyIsActive { get; set; }
		private bool TransformIsActive { get; set; }
		private bool UndoIsActive { get; set; }
		private bool RedoIsActive { get; set; }

		public void Dispose()
		{
			DeRegisterDefaultEvents();
		}

		private bool IgnoreEvent(CrashDoc comparisonDoc,
			bool ignoreIfCopy = true,
			bool ignoreIfBusy = true,
			bool ignoreIfTransform = true,
			bool ignoreIfUndoActive = true,
			bool ignoreIfRedoActive = true)
		{
			if (comparisonDoc is null)
			{
				return true;
			}

			if (comparisonDoc != ContextDocument)
			{
				return true;
			}

			if (ignoreIfCopy && CopyIsActive)
			{
				return true;
			}

			if (ignoreIfBusy && comparisonDoc.DocumentIsBusy)
			{
				return true;
			}

			if (ignoreIfTransform && TransformIsActive)
			{
				return true;
			}

			if (ignoreIfUndoActive && UndoIsActive)
			{
				return true;
			}

			if (ignoreIfRedoActive && RedoIsActive)
			{
				return true;
			}

			return false;
		}

#pragma warning disable VSTHRD100 // Cannot avoid async void methods here
		// TODO : Use Queue to prevent > 60fps being sent
		// TODO : Use Stream?
		private async void CaptureRhinoViewModified(object? sender, ViewEventArgs args)
		{
			if (CrashViewModified is null)
			{
				return;
			}

			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.View.Document);
			if (crashDoc is null)
			{
				return;
			}

			try
			{
				var viewArgs = new CrashViewArgs(crashDoc, args.View);
				await CrashViewModified.Invoke(sender, viewArgs);
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}
#pragma warning restore VSTHRD100 // Avoid async void methods

		private void Push(IUndoRedoCache add, bool forward = true)
		{
			EventQueue.Enqueue(add);

			if (forward)
			{
				UndoRecords.Push(add);
			}
			else
			{
				RedoRecords.Push(add);
			}
		}

#pragma warning disable VSTHRD100 // Cannot avoid async void methods here
		private async void CaptureIdle(object? _, EventArgs __)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(RhinoDoc.ActiveDoc);
			if (crashDoc != ContextDocument)
			{
				return;
			}

			if (EventQueue.Count <= 0 && SelectionQueue.Count <= 0)
			{
				return;
			}

			try
			{
				while (EventQueue.Count > 0)
				{
					var cache = EventQueue.Dequeue();
					var cacheAction = cache switch
					                  {
						                  AddRecord add => AddCrashObject.Invoke(this, add.AddArgs),
						                  TransformRecord transform =>
							                  TransformCrashObject.Invoke(this, transform.TransformArgs),
						                  DeleteRecord delete => DeleteCrashObject.Invoke(this, delete.DeleteArgs),
						                  UpdateRecord update => UpdateCrashObject.Invoke(this, update.UpdateArgs),
						                  ModifyGeometryRecord modify => SendModifyAsync(modify),
						                  _ => throw new NotImplementedException("Scenario not implemented!")
					                  };

					await cacheAction;
				}

				var selectionQueue = SelectionQueue.ToArray();
				SelectionQueue.Clear();
				foreach (var queueItem in selectionQueue)
				{
					var isSelected = queueItem.Value;
					var changeId = queueItem.Key;

					if (!crashDoc.RealisedChangeTable.TryGetRhinoId(changeId, out var rhinoId))
					{
						continue;
					}

					var theObject = new CrashObject(changeId, rhinoId);

					if (isSelected)
					{
						var crashArgs =
							CrashSelectionEventArgs.CreateSelectionEvent(ContextDocument, new[] { theObject });
						await SelectCrashObjects.Invoke(this, crashArgs);
					}
					else
					{
						var crashArgs =
							CrashSelectionEventArgs.CreateDeSelectionEvent(ContextDocument, new[] { theObject });
						await DeSelectCrashObjects.Invoke(this, crashArgs);
					}
				}
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
				Console.WriteLine(e);
			}
		}
#pragma warning restore VSTHRD100 // Avoid async void methods

		private async Task SendModifyAsync(ModifyGeometryRecord modifyRecord)
		{
			foreach (var addArgs in modifyRecord.AddArgs)
			{
				await AddCrashObject.Invoke(this, addArgs);
			}

			foreach (var removeArgs in modifyRecord.RemoveArgs)
			{
				await DeleteCrashObject.Invoke(this, removeArgs);
			}
		}

		private void CaptureAddRhinoObject(object? sender, RhinoObjectEventArgs args)
		{
			CaptureAddOrUndeleteRhinoObject(sender, args, false);
		}

		private void CaptureUnDeleteRhinoObject(object? sender, RhinoObjectEventArgs args)
		{
			CaptureAddOrUndeleteRhinoObject(sender, args, true);
		}

		private void CaptureAddOrUndeleteRhinoObject(object? sender, RhinoObjectEventArgs args, bool undelete)
		{
			var crashDoc =
				CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);

			if (IgnoreEvent(crashDoc, false))
			{
				return;
			}

			Created.Add(args.TheObject);

			var crashArgs = new CrashObjectEventArgs(crashDoc, args.TheObject, unDelete: undelete);
			var add = new AddRecord(crashArgs);
			Push(add);
		}

		private void CaptureDeleteRhinoObject(object? sender, RhinoObjectEventArgs args)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);
			if (IgnoreEvent(crashDoc))
			{
				return;
			}

			Deleted.Add(args.TheObject);

			var crashArgs = new CrashObjectEventArgs(crashDoc, args.TheObject);
			var deleteRecord = new DeleteRecord(crashArgs);
			Push(deleteRecord);
		}

		private void CaptureTransformRhinoObject(object? sender, RhinoTransformObjectsEventArgs args)
		{
			if (args.GripCount > 0)
			{
				CrashApp.Log("Grips Event Missed");
				RhinoApp.WriteLine("Modifying Grips is not yet supported by Crash");
				return;
			}

			var rhinoDoc = RhinoDocUtils.GetRhinoDocFromObjects(args.Objects);
			var crashDoc = CrashDocRegistry.GetRelatedDocument(rhinoDoc);
			if (IgnoreEvent(crashDoc, false))
			{
				return;
			}

			if (args.ObjectsWillBeCopied)
			{
				CopyIsActive = true;
				return;
			}

			TransformIsActive = true;

			var transform = args.Transform.ToCrash();
			var transformArgs =
				new CrashTransformEventArgs(crashDoc, transform,
				                            args.Objects.Select(o => new CrashObject(crashDoc, o)),
				                            args.ObjectsWillBeCopied);

			var transformRecord = new TransformRecord(transformArgs);
			Push(transformRecord);
		}

		private void CaptureSelectRhinoObjects(object? sender, RhinoObjectSelectionEventArgs args)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
			if (IgnoreEvent(crashDoc, false, ignoreIfUndoActive: false, ignoreIfRedoActive: false))
			{
				return;
			}

			var changeIds = new List<Guid>(args.RhinoObjects.Length);
			foreach (var rhinoObject in args.RhinoObjects)
			{
				if (!crashDoc.RealisedChangeTable.TryGetChangeId(rhinoObject.Id, out var changeId))
				{
					continue;
				}

				changeIds.Add(changeId);
			}

			PushSelections(changeIds, true);
		}

		private void CaptureDeselectRhinoObjects(object? sender, RhinoObjectSelectionEventArgs args)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
			if (IgnoreEvent(crashDoc, false, ignoreIfUndoActive: false, ignoreIfRedoActive: false))
			{
				return;
			}

			var changeIds = new List<Guid>(args.RhinoObjects.Length);
			foreach (var rhinoObject in args.RhinoObjects)
			{
				if (!crashDoc.RealisedChangeTable.TryGetChangeId(rhinoObject.Id, out var changeId))
				{
					continue;
				}

				changeIds.Add(changeId);
			}

			PushSelections(changeIds, false);
		}

		private void CaptureDeselectAllRhinoObjects(object? sender, RhinoDeselectAllObjectsEventArgs args)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
			if (IgnoreEvent(crashDoc, false, ignoreIfUndoActive: false, ignoreIfRedoActive: false))
			{
				return;
			}

			var currentlySelected = crashDoc.RealisedChangeTable.GetSelected();
			PushSelections(currentlySelected, false);
		}

		// Fires after all other Events
		private void CaptureEndCommand(object? sender, CommandEventArgs e)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(e.Document);
			if (crashDoc?.Equals(ContextDocument) != true)
			{
				return;
			}

			if (!UndoIsActive && !RedoIsActive)
			{
				if (Deleted.Count > 0 && Created.Count > 0)
				{
					for (var i = 0; i < Created.Count + Deleted.Count; i++)
					{
						UndoRecords.Pop();
						EventQueue.Dequeue();
					}

					var modifyGeometryRecord = new ModifyGeometryRecord(crashDoc, Created, Deleted);
					Push(modifyGeometryRecord);
				}
			}

			// Reset
			TransformIsActive = false;
			CopyIsActive = false;
			RedoIsActive = false;
			UndoIsActive = false;

			Created.Clear();
			Deleted.Clear();
		}

		private void PushSelections(IEnumerable<Guid> selection, bool select)
		{
			foreach (var selected in selection)
			{
				try
				{
					if (SelectionQueue.TryGetValue(selected, out var isSelected) && isSelected != select)
					{
						SelectionQueue.Remove(selected);
					}
					else
					{
						SelectionQueue.Add(selected, select);
					}
				}
				catch (Exception ex)
				{
				}
			}
		}

		private void CaptureModifyRhinoObjectAttributes(object? sender, RhinoModifyObjectAttributesEventArgs args)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
			if (IgnoreEvent(crashDoc, ignoreIfRedoActive: false, ignoreIfUndoActive: false))
			{
				return;
			}

			try
			{
				if (!crashDoc.RealisedChangeTable.TryGetChangeId(args.RhinoObject.Id, out var changeId))
				{
					return;
				}

				var updates =
					RhinoObjectAttributesUtils.GetAttributeDifferencesAsDictionary(args.OldAttributes,
							 args.NewAttributes);

				if (updates is null || !updates.Any())
				{
					return;
				}

				// TODO : Make into a const and document
				// Adding this allows us to quickly check if we need to loop through all the Rhino Object Attributes.
				updates.Add("HasRhinoObjectAttributes", bool.TrueString);

				var crashObject = new CrashObject(changeId, args.RhinoObject.Id);

				var updateArgs = new CrashUpdateArgs(crashDoc, crashObject, updates);
				Push(new UpdateRecord(updateArgs));
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}

		// TODO : Does this fire if redo cannot be done in Rhino?
		private void CaptureUndoRedo(object? sender, UndoRedoEventArgs args)
		{
			var rhinoDoc = RhinoDoc.ActiveDoc;
			var crashDoc = CrashDocRegistry.GetRelatedDocument(rhinoDoc);
			if (crashDoc != ContextDocument)
			{
				return;
			}

			if (crashDoc.DocumentIsBusy)
			{
				return;
			}

			if (!UndoIsActive)
			{
				UndoIsActive = args.IsBeginUndo || args.IsEndUndo;
			}

			if (!RedoIsActive)
			{
				RedoIsActive = args.IsBeginRedo || args.IsEndRedo;
			}

			if ((args.IsBeginUndo && UndoRecords.Count == 0) ||
			    (args.IsBeginRedo && RedoRecords.Count == 0))
			{
				return;
			}

			// TODO : Certain scenarios empty the redo queue.
			// A new Add for example should empty it.
			// Maybe we can check Rhino to see if Redo is possible
			if (args.IsEndUndo && UndoRecords.Count > 0)
			{
				var record = UndoRecords.Pop();
				if (record.TryGetInverse(out var cache))
				{
					Push(cache, false); // We went backwards
				}
			}
			else if (args.IsEndRedo && RedoRecords.Count > 0)
			{
				var record = RedoRecords.Pop();
				if (record.TryGetInverse(out var cache))
				{
					Push(cache);
				}
			}
		}

		internal delegate Task AsyncEventHandler<in TEventArgs>(object? sender, TEventArgs e);

		#region Published Events

		/// <summary>Invoked when a Crash Object is added and the Crash Doc is not busy</summary>
		internal event AsyncEventHandler<CrashObjectEventArgs>? AddCrashObject;

		/// <summary>Invoked when a Crash Object is deleted and the Crash Doc is not busy</summary>
		internal event AsyncEventHandler<CrashObjectEventArgs>? DeleteCrashObject;

		/// <summary>Invoked when a Crash Object is transformed and the Crash Doc is not busy</summary>
		internal event AsyncEventHandler<CrashTransformEventArgs>? TransformCrashObject;

		/// <summary>Invoked when a Crash Object is selected and the Crash Doc is not busy</summary>
		internal event AsyncEventHandler<CrashSelectionEventArgs>? SelectCrashObjects;

		/// <summary>Invoked when a Crash Object is unselected and the Crash Doc is not busy</summary>
		internal event AsyncEventHandler<CrashSelectionEventArgs>? DeSelectCrashObjects;

		/// <summary>Invoked when a Crash Object is updated and the Crash Doc is not busy</summary>
		internal event AsyncEventHandler<CrashUpdateArgs>? UpdateCrashObject;

		/// <summary>Is invoked when the Rhino View is modified</summary>
		internal event AsyncEventHandler<CrashViewArgs>? CrashViewModified;

		#endregion

		#region Register Events

		private void RegisterDefaultEvents()
		{
			// Object Events
			RhinoDoc.AddRhinoObject += CaptureAddRhinoObject;
			RhinoDoc.UndeleteRhinoObject += CaptureUnDeleteRhinoObject;
			RhinoDoc.DeleteRhinoObject += CaptureDeleteRhinoObject;
			RhinoDoc.BeforeTransformObjects += CaptureTransformRhinoObject;
			RhinoDoc.DeselectObjects += CaptureDeselectRhinoObjects;
			RhinoDoc.DeselectAllObjects += CaptureDeselectAllRhinoObjects;
			RhinoDoc.SelectObjects += CaptureSelectRhinoObjects;
			RhinoDoc.ModifyObjectAttributes += CaptureModifyRhinoObjectAttributes;
			// RhinoDoc.ReplaceRhinoObject

			// Command Events
			Command.UndoRedo += CaptureUndoRedo;
			Command.EndCommand += CaptureEndCommand;

			// App Events
			RhinoApp.Idle += CaptureIdle;

			// Doc Events
			// TODO : Implement
			// RhinoDoc.BeginOpenDocument += RhinoDocOnBeginOpenDocument;

			// View Events
			RhinoView.Modified += CaptureRhinoViewModified;
		}

		private void DeRegisterDefaultEvents()
		{
			// Object Events
			RhinoDoc.AddRhinoObject -= CaptureAddRhinoObject;
			RhinoDoc.UndeleteRhinoObject -= CaptureAddRhinoObject;
			RhinoDoc.DeleteRhinoObject -= CaptureDeleteRhinoObject;
			RhinoDoc.BeforeTransformObjects -= CaptureTransformRhinoObject;
			RhinoDoc.DeselectObjects -= CaptureDeselectRhinoObjects;
			RhinoDoc.DeselectAllObjects -= CaptureDeselectAllRhinoObjects;
			RhinoDoc.SelectObjects -= CaptureSelectRhinoObjects;
			RhinoDoc.ModifyObjectAttributes -= CaptureModifyRhinoObjectAttributes;

			// Command Events
			Command.UndoRedo -= CaptureUndoRedo;
			Command.EndCommand -= CaptureEndCommand;

			// Doc Events
			// TODO : Implement
			// RhinoDoc.BeginOpenDocument -= RhinoDocOnBeginOpenDocument;

			// View Events
			RhinoView.Modified -= CaptureRhinoViewModified;
		}

		#endregion
	}
}
