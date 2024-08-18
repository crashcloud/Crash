using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Common.Tables;
using Crash.Events;
using Crash.Handlers.Changes;
using Crash.Utils;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles Deleted objects from the Server</summary>
	internal sealed class GeometryRemoveRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.Action.HasFlag(ChangeAction.Remove);
		}


		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			IdleArgs idleArgs;
			IdleAction idleAction;

			var changeArgs = new IdleArgs(crashDoc, recievedChange);
			IdleAction resultingAction = null;

			if (crashDoc.Tables.TryGet<TemporaryChangeTable>(out var tempTable) && tempTable.TryGetChangeOfType(recievedChange.Id, out IChange temporaryChange))
			{
				resultingAction = new IdleAction(RemoveFromCache, changeArgs);
			}
			else if (crashDoc.Tables.TryGet<RealisedChangeTable>(out var realisedTable) && realisedTable.ContainsChangeId(recievedChange.Id))
			{
				resultingAction = new IdleAction(RemoveFromDocument, changeArgs);
			}
			else
			{
				return;
			}

			crashDoc.Queue.AddAction(resultingAction);
		}

		private void RemoveFromDocument(IdleArgs args)
		{
			args.Doc.DocumentIsBusy = true;
			try
			{
				if (!args.Change.TryGetRhinoObject(args.Doc, out var rhinoObject))
				{
					args.Doc.DocumentIsBusy = false;
					return;
				}

				var geomChange = GeometryChange.CreateFrom(args.Change);
				geomChange.SetGeometry(rhinoObject.Geometry.Duplicate());

				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);
				rhinoDoc.Objects.Delete(rhinoObject, true, true);

				if (!args.Doc.Tables.TryGet<TemporaryChangeTable>(out var tempTable)) return;
				if (!args.Doc.Tables.TryGet<RealisedChangeTable>(out var realTable)) return;

				tempTable.UpdateChange(geomChange);
				tempTable.DeleteChange(geomChange.Id);
				realTable.PurgeChange(args.Change.Id);
			}
			finally
			{
				args.Doc.DocumentIsBusy = false;
			}
		}

		private void RemoveFromCache(IdleArgs args)
		{
			if (!args.Doc.Tables.TryGet<TemporaryChangeTable>(out var tempTable)) return;
			tempTable.DeleteChange(args.Change.Id);
		}
	}
}
