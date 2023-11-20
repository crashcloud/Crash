using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;
using Crash.Handlers.Changes;
using Crash.Utils;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles transforms recieved from the server</summary>
	internal sealed class GeometryTransformRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.Action == ChangeAction.Transform;
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			var transformChange = TransformChange.CreateFrom(recievedChange);
			if (!transformChange.Transform.IsValid())
			{
				return;
			}

			var xform = transformChange.Transform.ToRhino();
			if (!xform.IsValid)
			{
				return;
			}

			if (crashDoc.TemporaryChangeTable.TryGetChangeOfType(recievedChange.Id, out GeometryChange geomChange))
			{
				geomChange.Geometry.Transform(xform);
			}
			else
			{
				crashDoc.Queue.AddAction(new IdleAction(TransformGeometry, new IdleArgs(crashDoc, transformChange)));
			}
		}

		private void TransformGeometry(IdleArgs args)
		{
			if (args.Change is not TransformChange transformChange)
			{
				return;
			}

			var xform = transformChange.Transform.ToRhino();
			if (!xform.IsValid)
			{
				return;
			}

			args.Doc.DocumentIsBusy = true;
			try
			{
				if (!args.Change.TryGetRhinoObject(args.Doc, out var rhinoObject))
				{
					return;
				}

				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);

				var isLocked = rhinoObject.IsLocked;
				if (isLocked)
				{
					rhinoDoc.Objects.Unlock(rhinoObject, true);
					rhinoObject.CommitChanges();
				}

				rhinoObject.Geometry.Transform(xform);
				rhinoObject.CommitChanges();

				if (isLocked)
				{
					rhinoDoc.Objects.Lock(rhinoObject, true);
					rhinoObject.CommitChanges();
				}
			}
			finally
			{
				args.Doc.DocumentIsBusy = false;
			}
		}
	}
}
