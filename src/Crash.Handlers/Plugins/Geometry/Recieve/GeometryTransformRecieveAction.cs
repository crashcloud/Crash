using Crash.Common.Changes;
using Crash.Common.Document;
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
			var transChange = TransformChange.CreateFrom(recievedChange);
			if (!transChange.Transform.IsValid())
			{
				return;
			}

			var xform = transChange.Transform.ToRhino();
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
				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);
				if (!recievedChange.TryGetRhinoObject(crashDoc, out var rhinoObject))
				{
					return;
				}

				crashDoc.DocumentIsBusy = true;
				try
				{
					var isLocked = rhinoObject.IsLocked;

					if (isLocked)
					{
						rhinoDoc.Objects.Unlock(rhinoObject, true);
					}

					rhinoObject.Geometry.Transform(xform);

					if (isLocked)
					{
						rhinoDoc.Objects.Lock(rhinoObject, true);
					}
				}
				finally
				{
					crashDoc.DocumentIsBusy = false;
				}
			}
		}
	}
}
