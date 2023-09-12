using Crash.Common.Document;
using Crash.Handlers.Changes;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles updates from the server</summary>
	internal sealed class GeometryUpdateRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change) => change.Action.HasFlag(ChangeAction.Update);


		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			if (!crashDoc.CacheTable.TryGetValue(recievedChange.Id, out GeometryChange geomChange))
			{
			}
			// geomChange.AddAction(recievedChange.Action);
			// Get Update Data
		}
	}
}
