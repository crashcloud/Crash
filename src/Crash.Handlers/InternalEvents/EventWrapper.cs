using Crash.Common.App;
using Crash.Common.Document;
using Crash.Geometry;
using Crash.Utils;

using Microsoft.Extensions.Logging;

using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Crash.Handlers.InternalEvents
{
	internal class EventWrapper : IDisposable
	{
		private readonly CrashDoc ContextDocument;
		private record struct TransformRecord(string Name, CrashTransformEventArgs TransformArgs);

		private readonly Stack<TransformRecord> UndoTransformRecords;
		private readonly Stack<TransformRecord> RedoTransformRecords;

		internal EventWrapper(CrashDoc _crashDoc)
		{
			ContextDocument = _crashDoc;
			UndoTransformRecords = new();
			RedoTransformRecords = new();
			RegisterDefaultEvents();
		}

		public void Dispose()
		{
			DeRegisterDefaultEvents();
		}

		private bool EventShouldFire(CrashDoc comparisonDoc)
		{
			if (comparisonDoc != ContextDocument)
				return false;

			if (comparisonDoc is null or { CopyIsActive: false, DocumentIsBusy: true, TransformIsActive: true })
			{
				return false;
			}

			return true;
		}

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

		private async void CaptureAddRhinoObject(object sender, RhinoObjectEventArgs args)
		{
			await CaptureAddOrUndeleteRhinoObject(sender, args, false);
		}

		private async void CaptureUnDeleteRhinoObject(object sender, RhinoObjectEventArgs args)
		{
			await CaptureAddOrUndeleteRhinoObject(sender, args, true);
		}

		private async Task CaptureAddOrUndeleteRhinoObject(object sender, RhinoObjectEventArgs args, bool undelete)
		{
			CrashApp.Log($"{nameof(CaptureAddRhinoObject)} event fired.", LogLevel.Trace);

			var crashDoc =
				CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);

			if (crashDoc is null || crashDoc.DocumentIsBusy || crashDoc.TransformIsActive)
			{
				return;
			}

			try
			{
				var crashArgs = new CrashObjectEventArgs(crashDoc, args.TheObject, unDelete: undelete);
				if (AddCrashObject is not null)
				{
					await AddCrashObject.Invoke(sender, crashArgs);
				}
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
				Console.WriteLine(e);
			}
		}

		private async void CaptureDeleteRhinoObject(object sender, RhinoObjectEventArgs args)
		{
			CrashApp.Log($"{nameof(CaptureDeleteRhinoObject)} event fired.", LogLevel.Trace);

			var crashDoc =
				CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);
			if (crashDoc is null || crashDoc.DocumentIsBusy || crashDoc.TransformIsActive)
			{
				return;
			}

			try
			{
				var crashArgs = new CrashObjectEventArgs(crashDoc, args.TheObject);
				if (DeleteCrashObject is not null)
				{
					await DeleteCrashObject.Invoke(sender, crashArgs);
				}
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
				Console.WriteLine(e);
			}
		}

		private async void CaptureTransformRhinoObject(object sender, RhinoTransformObjectsEventArgs args)
		{
			if (TransformCrashObject is null)
			{
				return;
			}

			if (args.GripCount > 0)
			{
				return;
			}

			CrashApp.Log($"{nameof(CaptureTransformRhinoObject)} event fired.", LogLevel.Trace);

			var rhinoDoc = args.Objects
							   ?.FirstOrDefault(o => o.Document is not null)
							   ?.Document;
			var crashDoc = CrashDocRegistry.GetRelatedDocument(rhinoDoc);
			if (!EventShouldFire(crashDoc))
			{
				return;
			}

			crashDoc.TransformIsActive = true;

			string commandName = string.Empty;
			var stack = Command.GetCommandStack();
			if (stack is not null && stack.Any())
			{
				commandName = Command.LookupCommandName(stack.Last(), true);
				TransformCommands.Add(commandName.ToUpperInvariant());
			}

			try
			{
				var transform = args.Transform.ToCrash();
				var crashArgs =
					new CrashTransformEventArgs(crashDoc, transform,
												args.Objects.Select(o => new CrashObject(o)),
												args.ObjectsWillBeCopied);

				await TransformCrashObject.Invoke(sender, crashArgs);

				UndoTransformRecords.Push(new(commandName, crashArgs));

				TransformCommands.Add(commandName);
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}

		private async void CaptureSelectRhinoObjects(object sender, RhinoObjectSelectionEventArgs args)
		{
			if (SelectCrashObjects is null)
			{
				return;
			}

			CrashApp.Log($"{nameof(CaptureSelectRhinoObjects)} event fired.", LogLevel.Trace);

			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
			if (!EventShouldFire(crashDoc))
			{
				return;
			}

			try
			{
				foreach (var rhinoObject in args.RhinoObjects)
				{
					if (!rhinoObject.TryGetChangeId(out var changeId))
					{
						continue;
					}

					crashDoc.RealisedChangeTable.AddSelected(changeId);
				}

				var crashArgs = CrashSelectionEventArgs.CreateSelectionEvent(crashDoc, args.RhinoObjects
																				 .Select(o => new CrashObject(o)));
				await SelectCrashObjects.Invoke(sender, crashArgs);
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}

		private async void CaptureDeselectRhinoObjects(object sender, RhinoObjectSelectionEventArgs args)
		{
			if (SelectCrashObjects is null)
			{
				return;
			}

			CrashApp.Log($"{nameof(CaptureDeselectRhinoObjects)} event fired.", LogLevel.Trace);

			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
			if (!EventShouldFire(crashDoc))
			{
				return;
			}

			try
			{
				foreach (var rhinoObject in args.RhinoObjects)
				{
					if (!rhinoObject.TryGetChangeId(out var changeId))
					{
						continue;
					}

					crashDoc.RealisedChangeTable.RemoveSelected(changeId);
				}

				var crashArgs =
					CrashSelectionEventArgs.CreateDeSelectionEvent(crashDoc, args.RhinoObjects
																	   .Select(o => new CrashObject(o)));

				await SelectCrashObjects.Invoke(sender, crashArgs);
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}

		private async void CaptureDeselectAllRhinoObjects(object sender, RhinoDeselectAllObjectsEventArgs args)
		{
			if (DeSelectCrashObjects is null)
			{
				return;
			}

			CrashApp.Log($"{nameof(CaptureDeselectAllRhinoObjects)} event fired.", LogLevel.Trace);

			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
			if (!EventShouldFire(crashDoc))
			{
				return;
			}

			try
			{
				var currentlySelected = crashDoc.RealisedChangeTable.GetSelected();
				var crashObjects = currentlySelected.Select(cs => new CrashObject(cs, Guid.Empty));
				var crashArgs = CrashSelectionEventArgs.CreateDeSelectionEvent(crashDoc, crashObjects);

				await DeSelectCrashObjects.Invoke(sender, crashArgs);
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}

		private async void CaptureModifyRhinoObjectAttributes(object sender, RhinoModifyObjectAttributesEventArgs args)
		{
			if (UpdateCrashObject is null)
			{
				return;
			}

			CrashApp.Log($"{nameof(CaptureModifyRhinoObjectAttributes)} event fired.", LogLevel.Trace);

			var crashDoc = CrashDocRegistry.GetRelatedDocument(args.Document);
			if (!EventShouldFire(crashDoc))
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
				await UpdateCrashObject.Invoke(sender, updateArgs);
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
			}
		}

		private async void CaptureUndoRedo(object sender, UndoRedoEventArgs args)
		{
			if (TransformCrashObject is null)
				return;

			var rhinoDoc = RhinoDoc.FromRuntimeSerialNumber(args.UndoSerialNumber);
			var crashDoc = CrashDocRegistry.GetRelatedDocument(rhinoDoc);
			if (crashDoc != ContextDocument)
				return;

			var commandId = args.CommandId;
			var commandName = Command.LookupCommandName(commandId, true);
			if (!IsTransformCommand(commandName))
				return;

			// TODO : Check for Doc is busy etc?

			if (args.IsBeginUndo || args.IsBeginRedo)
			{
				CrashDocRegistry.ActiveDoc.TransformIsActive = true;
			}	

			if (args.IsEndUndo)
			{
				// If the name is not equal then the Transform was enacted, but not sent to the server
				var peekResult = UndoTransformRecords.Peek();

				if (!peekResult.Name.Equals(commandName))
				{
					return;
				}

				var transformRecord = UndoTransformRecords.Pop();
				await TransformCrashObject.Invoke(sender, transformRecord.TransformArgs);
				RedoTransformRecords.Push(GetInvertedRecord(transformRecord));
			}
			else if (args.IsEndRedo)
			{

				// If the name is not equal then the Transform was enacted, but not sent to the server
				var peekResult = UndoTransformRecords.Peek();

				if (!peekResult.Name.Equals(commandName))
				{
					return;
				}

				var transformRecord = RedoTransformRecords.Pop();

				await TransformCrashObject.Invoke(sender, transformRecord.TransformArgs);

				UndoTransformRecords.Push(GetInvertedRecord(transformRecord));
			}
		}

		private TransformRecord GetInvertedRecord(TransformRecord transformRecord)
		{
			var name = transformRecord.Name;
			var args = transformRecord.TransformArgs;

			var rhinoTransform = args.Transform.ToRhino();
			rhinoTransform.TryGetInverse(out Transform invertedRhinoTransform);
			var invertedCrashTransform = invertedRhinoTransform.ToCrash();

			return new TransformRecord(name,
				new CrashTransformEventArgs(args.Doc, invertedCrashTransform, args.Objects, args.ObjectsWillBeCopied));
		}

		private static HashSet<string> TransformCommands = new();
		private static bool IsTransformCommand(string commandName)
			=> TransformCommands.Contains(commandName.ToUpperInvariant());

		private async void CaptureBeginCommand(object sender, CommandEventArgs args)
		{
			var commandName = args.CommandEnglishName.ToUpperInvariant();
		}

		private async void CaptureEndCommand(object sender, CommandEventArgs args)
		{
			var commandName = args.CommandEnglishName.ToUpperInvariant();
		}

		private async void CaptureRhinoViewModified(object sender, ViewEventArgs args)
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

		private void CaptureIdle(object sender, EventArgs e)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(RhinoDoc.ActiveDoc);
			if (crashDoc != ContextDocument)
				return;

			ContextDocument.DocumentIsBusy = false;
			ContextDocument.TransformIsActive = false;
			ContextDocument.CopyIsActive = false;
		}

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

		// TODO : Read up on Covariants again
		internal delegate Task AsyncEventHandler<in TEventArgs>(object? sender, TEventArgs e);
	}
}
