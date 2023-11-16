using Crash.Common.Document;
using Crash.Common.Events;
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

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);
			if (!recievedChange.TryGetRhinoObject(crashDoc, out var rhinoObject))
			{
				if (crashDoc.TemporaryChangeTable.TryGetChangeOfType(recievedChange.Id, out GeometryChange change))
				{
					idleArgs = new IdleArgs(crashDoc, recievedChange);
					idleAction = new IdleAction(RemoveTemporaryFromDocument, idleArgs);
					crashDoc.Queue.AddAction(idleAction);
				}

				return;
			}

			idleArgs = new IdleArgs(crashDoc, recievedChange);
			idleAction = new IdleAction(RemoveFromDocument, idleArgs);
			crashDoc.Queue.AddAction(idleAction);
		}

		private void RemoveFromDocument(IdleArgs args)
		{
			if (!args.Change.TryGetRhinoObject(args.Doc, out var rhinoObject))
			{
				return;
			}

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);

			rhinoDoc.Objects.Delete(rhinoObject, true);
			args.Doc.TemporaryChangeTable.RemoveChange(args.Change.Id);
		}

		private void RemoveTemporaryFromDocument(IdleArgs args)
		{
			args.Doc.TemporaryChangeTable.RemoveChange(args.Change.Id);
		}
	}
}
