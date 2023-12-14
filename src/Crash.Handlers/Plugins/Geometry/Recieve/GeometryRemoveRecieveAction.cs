using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;
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

			if (crashDoc.TemporaryChangeTable.TryGetChangeOfType(recievedChange.Id, out IChange temporaryChange))
			{
				resultingAction = new IdleAction(RemoveFromCache, changeArgs);
			}
			else if (crashDoc.RealisedChangeTable.ContainsChangeId(recievedChange.Id))
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

				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);
				rhinoDoc.Objects.Delete(rhinoObject, true, true);

				args.Doc.RealisedChangeTable.RemoveSelected(args.Change.Id);
				args.Doc.RealisedChangeTable.RemoveChange(args.Change.Id);
			}
			finally
			{
				args.Doc.DocumentIsBusy = false;
			}
		}

		private void RemoveFromCache(IdleArgs args)
		{
			args.Doc.TemporaryChangeTable.DeleteChange(args.Change.Id);
		}
	}
}
