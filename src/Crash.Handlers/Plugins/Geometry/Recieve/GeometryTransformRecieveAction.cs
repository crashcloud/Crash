using Crash.Changes.Extensions;
using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Handlers.Changes;
using Crash.Utils;

using Rhino.Geometry;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles transforms recieved from the server</summary>
	internal sealed class GeometryTransformRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.Action.HasFlag(ChangeAction.Transform);
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			GeometryBase geometry = null;

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

			if (!recievedChange.HasFlag(ChangeAction.Temporary))
			{
				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);
				if (!recievedChange.TryGetRhinoObject(crashDoc, out var rhinoObject))
				{
					return;
				}

				geometry = rhinoObject.Geometry;
			}
			else if (crashDoc.TemporaryChangeTable.TryGetChangeOfType(recievedChange.Id, out GeometryChange geomChange))
			{
				geometry = geomChange.Geometry;
			}
			else
			{
				return;
			}

			geometry.Transform(xform);
		}
	}
}
