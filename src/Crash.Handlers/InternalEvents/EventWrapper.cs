﻿using Crash.Common.App;
using Crash.Common.Document;
using Crash.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;

namespace Crash.Handlers.InternalEvents
{

	internal class EventWrapper : IDisposable
	{
		#region Flags

		private bool CopyIsActive { get; set; }
		private bool TransformIsActive { get; set; }
		private bool UndoIsActive { get; set; }
		private bool RedoIsActive { get; set; }
		private bool CustomActionIsActive { get; set; }

		#endregion

		#region Captured Data
		private List<RhinoObject> Created { get; set; } = new();
		private List<RhinoObject> Deleted { get; set; } = new();

		#endregion

		#region UndoRedo Records

		private interface IUndoRedoCache
		{
			IUndoRedoCache GetInverse();
		}

		// TODO : Include Plane Cache or otherwise
		private record TransformRecord : IUndoRedoCache
		{
			public readonly CrashTransformEventArgs TransformArgs;

			internal TransformRecord(CrashTransformEventArgs args)
			{
				TransformArgs = args;
			}

			public IUndoRedoCache GetInverse()
			{
				var transformCache = TransformArgs.Transform.ToRhino();
				transformCache.TryGetInverse(out var inverseTransform);

				var newArgs = new CrashTransformEventArgs(TransformArgs.Doc,
															inverseTransform.ToCrash(),
															TransformArgs.Objects,
															TransformArgs.ObjectsWillBeCopied);
				return new TransformRecord(newArgs);
			}
		}

		private record AddRecord : IUndoRedoCache
		{
			internal readonly CrashObjectEventArgs AddArgs;
			internal AddRecord(CrashObjectEventArgs addArgs)
			{
				AddArgs = addArgs;
			}

			public IUndoRedoCache GetInverse()
			{
				var deleteRecord = new DeleteRecord(new CrashObjectEventArgs(AddArgs.Doc, AddArgs.Geometry, AddArgs.RhinoId, AddArgs.ChangeId, false));
				return deleteRecord;
			}
		}

		/// <summary>Records an Event that converts one or more change(s) into one or more change(s)
		/// A good example is Explode or BooleanUnion</summary>
		private record ModifyGeometryRecord : IUndoRedoCache
		{
			internal readonly CrashObjectEventArgs[] AddArgs;
			internal readonly CrashObjectEventArgs[] RemoveArgs;

			internal ModifyGeometryRecord(CrashDoc doc,
											IEnumerable<RhinoObject> addedObjects,
											IEnumerable<RhinoObject> removedObjects)
			{

			}

			private ModifyGeometryRecord(CrashObjectEventArgs[] addedArgs,
										CrashObjectEventArgs[] removedArgs)
			{
				AddArgs = addedArgs;
				RemoveArgs = removedArgs;
			}

			public IUndoRedoCache GetInverse()
				=> new ModifyGeometryRecord(RemoveArgs, AddArgs);
		}

		private record DeleteRecord : IUndoRedoCache
		{
			internal readonly CrashObjectEventArgs DeleteArgs;

			internal DeleteRecord(CrashObjectEventArgs args)
			{
				DeleteArgs = args;
			}

			public IUndoRedoCache GetInverse()
			{
				var addRecord = new AddRecord(new CrashObjectEventArgs(DeleteArgs.Doc, DeleteArgs.Geometry, DeleteArgs.RhinoId, DeleteArgs.ChangeId, true));
				return addRecord;
			}
		}

		private record UpdateRecord : IUndoRedoCache
		{
			internal readonly CrashUpdateArgs UpdateArgs;
			internal UpdateRecord(CrashUpdateArgs args)
			{
				UpdateArgs = args;
			}

			public IUndoRedoCache GetInverse()
			{
				throw new NotImplementedException("Not enough data here to implement a reset call");
			}
		}

		private readonly Stack<IUndoRedoCache> UndoRecords;
		private readonly Stack<IUndoRedoCache> RedoRecords;
		private readonly AsyncQueue<IUndoRedoCache> EventQueue;
		private readonly Dictionary<Guid, bool> SelectionQueue;

		#endregion

		private readonly CrashDoc ContextDocument;

		internal EventWrapper(CrashDoc _crashDoc)
		{
			ContextDocument = _crashDoc;
			UndoRecords = new();
			RedoRecords = new();
			EventQueue = new();
			SelectionQueue = new();
			RegisterDefaultEvents();
		}

		public void Dispose()
		{
			DeRegisterDefaultEvents();
		}

		private bool IgnoreEvent(CrashDoc comparisonDoc,
			bool ignoreIfCopy = true,
			bool ignoreIfBusy = true,
			bool ignoreIfTransform = true,
			bool ignoreIfUndoActive = true,
			bool ignoreIfRedoActive = true,
			bool ignoreIfCustomActionIsActive = true)
		{
			if (comparisonDoc is null)
				return true;

			if (comparisonDoc != ContextDocument)
				return true;

			if (ignoreIfCopy && CopyIsActive)
				return true;

			if (ignoreIfBusy && comparisonDoc.DocumentIsBusy)
				return true;

			if (ignoreIfTransform && TransformIsActive)
				return true;

			if (ignoreIfUndoActive && UndoIsActive)
				return true;

			if (ignoreIfRedoActive && RedoIsActive)
				return true;

			if (ignoreIfCustomActionIsActive && CustomActionIsActive)
				return true;

			return false;
		}

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

		#region Capturers

		private void CaptureAddRhinoObject(object? sender, RhinoObjectEventArgs args)
			=> CaptureAddOrUndeleteRhinoObject(sender, args, false);

		private void CaptureUnDeleteRhinoObject(object? sender, RhinoObjectEventArgs args)
			=> CaptureAddOrUndeleteRhinoObject(sender, args, true);

		private void CaptureAddOrUndeleteRhinoObject(object? sender, RhinoObjectEventArgs args, bool undelete)
		{
			CrashApp.Log($"{nameof(CaptureAddRhinoObject)} event fired.", LogLevel.Trace);

			var crashDoc =
				CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);

			Created.Add(args.TheObject);

			if (IgnoreEvent(crashDoc, ignoreIfCopy: false))
			{
				return;
			}

			var crashArgs = new CrashObjectEventArgs(crashDoc, args.TheObject, unDelete: undelete);
			AddRecord add = new AddRecord(crashArgs);
			Push(add);
		}

		private void CaptureDeleteRhinoObject(object? sender, RhinoObjectEventArgs args)
		{
			try
			{
				Deleted.Add(args.TheObject);

				CrashApp.Log($"{nameof(CaptureDeleteRhinoObject)} event fired.", LogLevel.Trace);

				var crashDoc =
					CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);

				if (IgnoreEvent(crashDoc))
				{
					return;
				}

				var crashArgs = new CrashObjectEventArgs(crashDoc, args.TheObject);
				var deleteRecord = new DeleteRecord(crashArgs);
				Push(deleteRecord);
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}

		private void CaptureTransformRhinoObject(object? sender, RhinoTransformObjectsEventArgs args)
		{
			if (TransformCrashObject is null || args is null)
			{
				return;
			}

			if (args.GripCount > 0)
			{
				return;
			}

			try
			{
				CrashApp.Log($"{nameof(CaptureTransformRhinoObject)} event fired.", LogLevel.Trace);

				var rhinoDoc = args.Objects
								   ?.FirstOrDefault(o => o.Document is not null)
								   ?.Document;
				var crashDoc = CrashDocRegistry.GetRelatedDocument(rhinoDoc);
				if (IgnoreEvent(crashDoc, ignoreIfCopy: false))
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
												args.Objects.Select(o => new CrashObject(o)),
												args.ObjectsWillBeCopied);

				var transformRecord = new TransformRecord(transformArgs);
				Push(transformRecord);
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}

		private void CaptureSelectRhinoObjects(object? sender, RhinoObjectSelectionEventArgs args)
		{
			if (SelectCrashObjects is null)
			{
				return;
			}

			try
			{
				CrashApp.Log($"{nameof(CaptureSelectRhinoObjects)} event fired.", LogLevel.Trace);

				var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);

				List<Guid> changeIds = new List<Guid>(args.RhinoObjects.Length);
				foreach (var rhinoObject in args.RhinoObjects)
				{
					if (!rhinoObject.TryGetChangeId(out var changeId))
					{
						continue;
					}

					crashDoc.RealisedChangeTable.AddSelected(changeId);
					changeIds.Add(changeId);
				}

				PushSelections(changeIds, true);
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}
		private void CaptureDeselectRhinoObjects(object? sender, RhinoObjectSelectionEventArgs args)
		{
			if (SelectCrashObjects is null)
			{
				return;
			}

			try
			{
				CrashApp.Log($"{nameof(CaptureDeselectRhinoObjects)} event fired.", LogLevel.Trace);

				var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);

				List<Guid> changeIds = new List<Guid>(args.RhinoObjects.Length);
				foreach (var rhinoObject in args.RhinoObjects)
				{
					if (!rhinoObject.TryGetChangeId(out var changeId))
					{
						continue;
					}

					crashDoc.RealisedChangeTable.RemoveSelected(changeId);
					changeIds.Add(changeId);
				}

				PushSelections(changeIds, false);
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}
		private void CaptureDeselectAllRhinoObjects(object? sender, RhinoDeselectAllObjectsEventArgs args)
		{
			if (DeSelectCrashObjects is null)
			{
				return;
			}

			try
			{
				CrashApp.Log($"{nameof(CaptureDeselectAllRhinoObjects)} event fired.", LogLevel.Trace);

				var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);

				var currentlySelected = crashDoc.RealisedChangeTable.GetSelected();
				PushSelections(currentlySelected, false);

				crashDoc.RealisedChangeTable.ClearSelected();
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}

		private void CaptureBeginCommand(object? sender, CommandEventArgs e)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(e.Document);
			if (crashDoc != ContextDocument)
				return;

			if (Deleted.Count > 0 && Created.Count > 0)
			{
				CustomActionIsActive = true;
			}
		}

		// Fires after all other Events
		private void CaptureEndCommand(object? sender, CommandEventArgs e)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(e.Document);
			if (crashDoc != ContextDocument)
				return;

			// Reset
			TransformIsActive = false;
			CopyIsActive = false;
			RedoIsActive = false;
			UndoIsActive = false;
			CustomActionIsActive = false;
			Created.Clear();
			Deleted.Clear();

			if (Deleted.Count > 0 && Created.Count > 0)
			{
				Push(new ModifyGeometryRecord(crashDoc, Created, Deleted));
			}
		}

		private void PushSelections(IEnumerable<Guid> selection, bool select)
		{
			foreach (var selected in selection)
			{
				if (SelectionQueue.TryGetValue(selected, out bool isSelected) && isSelected != select)
					SelectionQueue.Remove(selected);

				else
					SelectionQueue.Add(selected, select);
			}
		}

		private void CaptureModifyRhinoObjectAttributes(object? sender, RhinoModifyObjectAttributesEventArgs args)
		{
			if (UpdateCrashObject is null)
			{
				return;
			}

			CrashApp.Log($"{nameof(CaptureModifyRhinoObjectAttributes)} event fired.", LogLevel.Trace);

			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
			if (IgnoreEvent(crashDoc))
			{
				return;
			}

			try
			{
				if (!crashDoc.TemporaryChangeTable.TryGetChangeOfType(args.RhinoObject.Id, out IChange change))
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

				var crashObject = new CrashObject(change.Id, args.RhinoObject.Id);

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
				return;

			if (crashDoc.DocumentIsBusy)
				return;

			if ((args.IsBeginUndo && UndoRecords.Count == 0) ||
				(args.IsBeginRedo && RedoRecords.Count == 0))
				return;

			UndoIsActive = args.IsBeginUndo;
			RedoIsActive = args.IsBeginRedo;

			// TODO : Certain scenarios empty the redo queue.
			// A new Add for example should empty it.
			// Maybe we can check Rhino to see if Redo is possible
			if (args.IsEndUndo && UndoRecords.Count > 0)
			{
				var record = UndoRecords.Pop();
				Push(record, false); // We went backwards
			}
			else if (args.IsEndRedo && RedoRecords.Count > 0)
			{
				var record = RedoRecords.Pop();
				Push(record);
			}
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
				UndoRecords.Push(add.GetInverse());
			else
				RedoRecords.Push(add.GetInverse());
		}

#pragma warning disable VSTHRD100 // Cannot avoid async void methods here
		private async void CaptureIdle(object? _, EventArgs __)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(RhinoDoc.ActiveDoc);
			if (crashDoc != ContextDocument)
				return;

			TransformIsActive = false;
			CopyIsActive = false;
			RedoIsActive = false;
			UndoIsActive = false;
			CustomActionIsActive = false;

			if (EventQueue.IsEmpty && SelectionQueue.Count <= 0)
				return;

			try
			{
				while (!EventQueue.IsEmpty)
				{
					var cache = await EventQueue.DequeueAsync();
					var cacheAction = cache switch
					{
						AddRecord add => SendAddAsync(add),
						TransformRecord transform => SendTransformAsync(transform),
						DeleteRecord delete => SendDeleteAsync(delete),
						UpdateRecord update => SendUpdateAsync(update),
						ModifyGeometryRecord modify => SendModifyAsync(modify),
						_ => Task.CompletedTask
					};

					await cacheAction;
				}

				var selectionQueue = SelectionQueue.ToArray();
				SelectionQueue.Clear();
				foreach (var queueItem in selectionQueue)
				{
					bool isSelected = queueItem.Value;
					Guid changeId = queueItem.Key;

					if (!crashDoc.RealisedChangeTable.TryGetRhinoId(changeId, out Guid rhinoId))
						continue;

					var theObject = new CrashObject(changeId, rhinoId);

					if (isSelected)
						await SendSelectionAsync(theObject);
					else
						await SendDeselectionAsync(theObject);
				}

			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
				Console.WriteLine(e);
			}
		}
#pragma warning restore VSTHRD100 // Avoid async void methods

		#region Idle Push

		private async Task SendDeselectionAsync(CrashObject theObject)
		{
			if (DeSelectCrashObjects is not null)
			{
				var crashArgs = CrashSelectionEventArgs.CreateDeSelectionEvent(ContextDocument, new CrashObject[] { theObject });
				await DeSelectCrashObjects.Invoke(this, crashArgs);
			}
		}

		private async Task SendSelectionAsync(CrashObject theObject)
		{
			if (SelectCrashObjects is not null)
			{
				var crashArgs = CrashSelectionEventArgs.CreateSelectionEvent(ContextDocument, new CrashObject[] { theObject });
				await SelectCrashObjects.Invoke(this, crashArgs);
			}
		}

		private async Task SendUpdateAsync(UpdateRecord update)
		{
			if (UpdateCrashObject is not null)
			{
				await UpdateCrashObject.Invoke(this, update.UpdateArgs);
			}
		}

		private async Task SendDeleteAsync(DeleteRecord delete)
		{
			if (DeleteCrashObject is not null)
			{
				await DeleteCrashObject.Invoke(this, delete.DeleteArgs);
			}
		}

		private async Task SendTransformAsync(TransformRecord transform)
		{
			if (TransformCrashObject is not null)
			{
				await TransformCrashObject.Invoke(this, transform.TransformArgs);
			}
		}

		private async Task SendAddAsync(AddRecord addRecord)
		{
			if (AddCrashObject is not null)
			{
				await AddCrashObject.Invoke(this, addRecord.AddArgs);
			}
		}

		private async Task SendModifyAsync(ModifyGeometryRecord modifyRecord)
		{
			if (AddCrashObject is not null)
			{
				foreach (var addArgs in modifyRecord.AddArgs)
				{
					await AddCrashObject.Invoke(this, addArgs);
				}
			}

			if (DeleteCrashObject is not null)
			{
				foreach (var removeArgs in modifyRecord.RemoveArgs)
				{
					await DeleteCrashObject.Invoke(this, removeArgs);
				}
			}
		}

		#endregion

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
			Command.BeginCommand += CaptureBeginCommand;
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
			Command.BeginCommand -= CaptureBeginCommand;
			Command.EndCommand -= CaptureEndCommand;

			// Doc Events
			// TODO : Implement
			// RhinoDoc.BeginOpenDocument -= RhinoDocOnBeginOpenDocument;

			// View Events
			RhinoView.Modified -= CaptureRhinoViewModified;
		}

		#endregion

		// TODO : Read up on Covariants again
		internal delegate Task AsyncEventHandler<in TEventArgs>(object? sender, TEventArgs e);
	}
}
