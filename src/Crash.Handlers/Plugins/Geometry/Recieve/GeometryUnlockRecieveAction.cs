﻿using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;
using Crash.Utils;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles unselections from the server</summary>
	internal sealed class GeometryUnlockRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.Action.HasFlag(ChangeAction.Unlocked);
		}


		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			var changeArgs = new IdleArgs(crashDoc, recievedChange);
			var lockAction = new IdleAction(LockChange, changeArgs);
			crashDoc.Queue.AddAction(lockAction);
		}

		private void LockChange(IdleArgs args)
		{
			if (!args.Change.TryGetRhinoObject(args.Doc, out var rhinoObject))
			{
				return;
			}

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);
			rhinoDoc.Objects.Unlock(rhinoObject, true);
		}
	}
}
