using Crash.Changes.Extensions;
using Crash.Changes.Utils;
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
		public bool CanRecieve(IChange change)
		{
			if (change is null)
				return false;

			if (!change.Action.HasFlag(ChangeAction.Add))
			{
				return false;
			}

			if (change.Action.HasFlag(ChangeAction.Remove))
			{
				return false;
			}

			return true;
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			await OnRecieveAsync(crashDoc, GeometryChange.CreateFrom(recievedChange));
		}

		/// <summary>Handles recieved Geometry Changes</summary>
		public async Task OnRecieveAsync(CrashDoc crashDoc, GeometryChange geomChange)
		{
			if (crashDoc is null || geomChange is null)
				return;

			var changeArgs = new IdleArgs(crashDoc, geomChange);
			IdleAction resultingAction = null;

			if (!geomChange.HasFlag(ChangeAction.Temporary))
			{
				resultingAction = new IdleAction(AddToDocument, changeArgs);
			}
			else if (geomChange.Owner?.Equals(crashDoc.Users.CurrentUser.Name,
			                                 StringComparison.InvariantCultureIgnoreCase) == true)
			{
				resultingAction = new IdleAction(AddToDocument, changeArgs);
			}
			else
			{
				resultingAction = new IdleAction(AddToCache, changeArgs);
			}

			crashDoc.Queue.AddAction(resultingAction);
		}

		private void AddToDocument(IdleArgs args)
		{
			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);
			if (args.Change is not GeometryChange geomChange)
			{
				return;
			}

			args.Doc.IsInit = true;
			try
			{
				var rhinoId = rhinoDoc.Objects.Add(geomChange.Geometry);
				var rhinoObject = rhinoDoc.Objects.FindId(rhinoId);
				rhinoObject.SyncHost(geomChange, args.Doc);

				if (args.Change.HasFlag(ChangeAction.Locked))
				{
					rhinoDoc.Objects.Select(rhinoId, true, true);
				}
			}
			finally
			{
				args.Doc.IsInit = false;
			}
		}

		private void AddToCache(IdleArgs args)
		{
			if (args.Change is not GeometryChange geomChange)
			{
				return;
			}

			var finalChange = geomChange;
			if (args.Doc.TemporaryChangeTable.TryGetChangeOfType(geomChange.Id, out GeometryChange existingChange))
			{
				var combinedChange = ChangeUtils.CombineChanges(existingChange, geomChange);
				finalChange = GeometryChange.CreateFrom(combinedChange);
			}

			args.Doc.TemporaryChangeTable.UpdateChange(finalChange);
		}
	}
}
