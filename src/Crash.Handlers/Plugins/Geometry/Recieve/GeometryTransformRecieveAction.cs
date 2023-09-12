using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Handlers.Changes;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles transforms recieved from the server</summary>
	internal sealed class GeometryTransformRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change) => change.Action.HasFlag(ChangeAction.Transform);


		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			if (!crashDoc.CacheTable.TryGetValue(recievedChange.Id, out GeometryChange geomChange))
			{
				return;
			}

			var transChange = TransformChange.CreateFrom(recievedChange);
			var xform = transChange.Transform.ToRhino();
			geomChange.Geometry.Transform(xform);
		}
	}
}
