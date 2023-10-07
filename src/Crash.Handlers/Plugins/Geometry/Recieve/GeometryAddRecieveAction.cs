using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;
using Crash.Handlers.Changes;
using Crash.Utils;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles recieving of Geometry</summary>
	internal sealed class GeometryAddRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change) => change.Action.HasFlag(ChangeAction.Add);

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			await OnRecieveAsync(crashDoc, GeometryChange.CreateFrom(recievedChange));
		}

		/// <summary>Handles recieved Geometry Changes</summary>
		public async Task OnRecieveAsync(CrashDoc crashDoc, GeometryChange geomChange)
		{
			if (IsDuplicate(crashDoc, geomChange))
			{
				return;
			}

			var changeArgs = new IdleArgs(crashDoc, geomChange);
			var bakeAction = new IdleAction(AddToDocument, changeArgs);
			await crashDoc.Queue.AddActionAsync(bakeAction);
		}

		// Prevents issues with the same user logged in twice
		private static bool IsDuplicate(CrashDoc crashDoc, IChange change)
		{
			var isNotInit = !crashDoc.CacheTable.IsInit;
			var isByCurrentUser = change.Owner.Equals(crashDoc.Users.CurrentUser.Name, StringComparison.Ordinal);
			return isNotInit && isByCurrentUser;
		}

		private void AddToDocument(IdleArgs args)
		{
			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);
			if (args.Change is not GeometryChange geomChange)
			{
				return;
			}

			args.Doc.CacheTable.IsInit = true;
			try
			{
				var rhinoId = rhinoDoc.Objects.Add(geomChange.Geometry);
				var rhinoObject = rhinoDoc.Objects.FindId(rhinoId);
				rhinoObject.SyncHost(geomChange);

				if (args.Change.Action.HasFlag(ChangeAction.Locked))
				{
					rhinoDoc.Objects.Lock(rhinoId, true);
				}
			}
			finally
			{
				args.Doc.CacheTable.IsInit = false;
			}
		}
	}
}
