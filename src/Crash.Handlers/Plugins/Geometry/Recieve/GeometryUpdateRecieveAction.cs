using System.Text.Json;

using Crash.Common.Document;
using Crash.Common.Tables;
using Crash.Handlers.Utils;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles updates from the server</summary>
	internal sealed class GeometryUpdateRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.Action.HasFlag(ChangeAction.Update);
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			var payload = JsonSerializer.Deserialize<PayloadPacket>(recievedChange.Payload);

			if (!crashDoc.Tables.TryGet<RealisedChangeTable>(out var realisedTable)) return;

			realisedTable.TryGetRhinoId(recievedChange.Id, out var rhinoId);
			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);

			var rhinoObject = rhinoDoc.Objects.FindId(rhinoId);

			var updates = payload.Updates;
			if (updates?.Count > 0)
			{
				var userName = crashDoc.Users.CurrentUser.Name;
				ObjectAttributeComparisonUtils.UpdateAttributes(rhinoObject.Attributes, updates, userName);
				rhinoObject.CommitChanges();
			}
		}
	}
}
