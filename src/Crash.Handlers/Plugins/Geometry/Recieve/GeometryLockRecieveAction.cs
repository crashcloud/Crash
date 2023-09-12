using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;
using Crash.Utils;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles Selected objects from the server</summary>
	internal sealed class GeometryLockRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change) => change.Action.HasFlag(ChangeAction.Locked);


		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			var changeArgs = new IdleArgs(crashDoc, recievedChange);
			var lockAction = new IdleAction(LockChange, changeArgs);
			await crashDoc.Queue.AddActionAsync(lockAction);
		}

		private void LockChange(IdleArgs args)
		{
			if (!args.Change.TryGetRhinoObject(out var rhinoObject))
			{
				return;
			}

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);
			rhinoDoc.Objects.Lock(rhinoObject, true);
		}
	}
}
