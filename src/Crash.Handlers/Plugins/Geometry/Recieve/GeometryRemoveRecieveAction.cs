using Crash.Changes.Extensions;
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

			if (!recievedChange.HasFlag(ChangeAction.Temporary))
			{
				resultingAction = new IdleAction(RemoveFromCache, changeArgs);
			}
			else
			{
				resultingAction = new IdleAction(RemoveFromDocument, changeArgs);
			}

			crashDoc.Queue.AddAction(resultingAction);
		}

		private void RemoveFromDocument(IdleArgs args)
		{
			args.Doc.SomeoneIsDone = true;
			if (!args.Change.TryGetRhinoObject(args.Doc, out var rhinoObject))
			{
				return;
			}

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);
			rhinoDoc.Objects.Delete(rhinoObject, true);

			args.Doc.RealisedChangeTable.RemoveSelected(args.Change.Id);
			args.Doc.RealisedChangeTable.RemoveChange(args.Change.Id);

			args.Doc.SomeoneIsDone = false;
		}

		private void RemoveFromCache(IdleArgs args)
		{
			args.Doc.TemporaryChangeTable.RemoveChange(args.Change.Id);
		}
	}
}
