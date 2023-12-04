using Crash.Common.App;
using Crash.Utils;

using Microsoft.Extensions.Logging;

using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

namespace Crash.Handlers.InternalEvents
{
	internal class EventWrapper : IDisposable
	{
		internal EventWrapper()
		{
			RegisterDefaultEvents();
		}

		public void Dispose()
		{
			DeRegisterDefaultEvents();
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

		private async Task CaptureAddOrUndeleteRhinoObject(object sender, RhinoObjectEventArgs args, bool undelete)
		{
			CrashApp.Log($"{nameof(CaptureAddRhinoObject)} event fired.", LogLevel.Trace);

			var crashDoc =
				CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);

			if (crashDoc is null || crashDoc.DocumentIsBusy)
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

		private async void CaptureAddRhinoObject(object sender, RhinoObjectEventArgs args)
		{
			await CaptureAddOrUndeleteRhinoObject(sender, args, false);
		}

		private async void CaptureUnDeleteRhinoObject(object sender, RhinoObjectEventArgs args)
		{
			await CaptureAddOrUndeleteRhinoObject(sender, args, true);
		}

		private async void CaptureDeleteRhinoObject(object sender, RhinoObjectEventArgs args)
		{
			CrashApp.Log($"{nameof(CaptureDeleteRhinoObject)} event fired.", LogLevel.Trace);

			var crashDoc =
				CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);
			if (crashDoc is null || crashDoc.DocumentIsBusy)
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
			if (crashDoc is null or { CopyIsActive: false, DocumentIsBusy: true })
			{
				return;
			}

			try
			{
				var crashArgs =
					new CrashTransformEventArgs(crashDoc, args.Transform.ToCrash(),
					                            args.Objects.Select(o => new CrashObject(o)),
					                            args.ObjectsWillBeCopied);

				await TransformCrashObject.Invoke(sender, crashArgs);
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
			if (crashDoc is null || crashDoc.DocumentIsBusy)
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

			if (crashDoc is null || crashDoc.DocumentIsBusy)
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
			if (crashDoc is null || crashDoc.DocumentIsBusy)
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
			if (crashDoc is null || crashDoc.DocumentIsBusy)
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

		private async void CaptureRhinoViewModified(object sender, ViewEventArgs args)
		{
			if (CrashViewModified is null)
			{
				return;
			}

			CrashApp.Log($"{nameof(CaptureRhinoViewModified)} event fired.", LogLevel.Trace);

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
