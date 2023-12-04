using Crash.Common.App;

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

		private event AsyncEventHandler<CrashObjectEventArgs>? AddCrashObject;
		private event AsyncEventHandler<CrashObjectEventArgs>? DeleteCrashObject;
		private event AsyncEventHandler<CrashObjectEventArgs>? TransformCrashObject;
		private event AsyncEventHandler<CrashObjectEventArgs>? SelectCrashObjects;
		private event AsyncEventHandler<CrashObjectEventArgs>? DeSelectCrashObjects;
		private event AsyncEventHandler<CrashObjectEventArgs>? UpdateCrashObject;
		private event AsyncEventHandler<CrashObjectEventArgs>? CrashViewModified;

		private async void CaptureAddRhinoObject(object sender, RhinoObjectEventArgs args)
		{
			try
			{
				CrashApp.Log($"{nameof(AddCrashObject)} event fired.", LogLevel.Trace);

				var crashDoc =
					CrashDocRegistry.GetRelatedDocument(args.TheObject.Document);
				if (crashDoc is null)
				{
					return;
				}

				// object HAS a Crash ID

				if (crashDoc.DocumentIsBusy)
				{
					return;
				}

				var crashArgs = new CrashObjectEventArgs(args.TheObject);
				if (AddCrashObject is not null)
				{
					await AddCrashObject.Invoke(sender, crashArgs);
				}
			}
			catch (Exception e)
			{
				CrashApp.Log(e.Message);
				Console.WriteLine(e);
				throw;
			}
		}

		private async void CaptureDeleteRhinoObject(object sender, RhinoObjectEventArgs args) { }

		private async void CaptureTransformRhinoObject(object sender, RhinoTransformObjectsEventArgs args) { }

		private async void CaptureSelectRhinoObjects(object sender, RhinoObjectSelectionEventArgs args) { }

		private async void CaptureDeselectRhinoObjects(object sender, RhinoObjectSelectionEventArgs args) { }

		private async void CaptureDeselectAllRhinoObjects(object sender, RhinoDeselectAllObjectsEventArgs args) { }

		private async void CaptureModifyRhinoObjectAttributes(object sender, RhinoModifyObjectAttributesEventArgs args)
		{
		}

		private async void CaptureUserStringChanged(object sender, RhinoDoc.UserStringChangedArgs args) { }

		private async void CaptureRhinoViewModified(object sender, ViewEventArgs args) { }

		private void RegisterDefaultEvents()
		{
			// Object Events
			RhinoDoc.AddRhinoObject += CaptureAddRhinoObject;
			RhinoDoc.AddRhinoObject += CaptureAddRhinoObject;
			RhinoDoc.DeleteRhinoObject += CaptureDeleteRhinoObject;
			RhinoDoc.BeforeTransformObjects += CaptureTransformRhinoObject;
			RhinoDoc.DeselectObjects += CaptureDeselectRhinoObjects;
			RhinoDoc.DeselectAllObjects += CaptureDeselectAllRhinoObjects;
			RhinoDoc.SelectObjects += CaptureSelectRhinoObjects;
			RhinoDoc.ModifyObjectAttributes += CaptureModifyRhinoObjectAttributes;
			RhinoDoc.UserStringChanged += CaptureUserStringChanged;

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
			RhinoDoc.AddRhinoObject -= CaptureAddRhinoObject;
			RhinoDoc.DeleteRhinoObject -= CaptureDeleteRhinoObject;
			RhinoDoc.BeforeTransformObjects -= CaptureTransformRhinoObject;
			RhinoDoc.DeselectObjects -= CaptureDeselectRhinoObjects;
			RhinoDoc.DeselectAllObjects -= CaptureDeselectAllRhinoObjects;
			RhinoDoc.SelectObjects -= CaptureSelectRhinoObjects;
			RhinoDoc.ModifyObjectAttributes -= CaptureModifyRhinoObjectAttributes;
			RhinoDoc.UserStringChanged -= CaptureUserStringChanged;

			// Doc Events
			// TODO : Implement
			// RhinoDoc.BeginOpenDocument -= RhinoDocOnBeginOpenDocument;

			// View Events
			RhinoView.Modified -= CaptureRhinoViewModified;
		}

		private delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);
	}
}
