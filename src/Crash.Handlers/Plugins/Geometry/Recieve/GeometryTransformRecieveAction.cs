using Crash.Changes.Extensions;
using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Handlers.Changes;
using Crash.Utils;

using Rhino.DocObjects;
using Rhino.Geometry;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles transforms recieved from the server</summary>
	internal sealed class GeometryTransformRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change) => change.Action.HasFlag(ChangeAction.Transform);

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			GeometryBase geometry = null;

			var transChange = TransformChange.CreateFrom(recievedChange);
			var xform = transChange.Transform.ToRhino();

			if (!recievedChange.HasFlag(ChangeAction.Temporary))
			{
				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);
				if (!ChangeUtils.TryGetRhinoObject(recievedChange, out RhinoObject rhinoObject))
					return;

				geometry = rhinoObject.Geometry;
			}
			else if (crashDoc.CacheTable.TryGetValue(recievedChange.Id, out GeometryChange geomChange))
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
