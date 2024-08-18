using Crash.Common.Tables;
using Crash.Handlers.Changes;
using Crash.Handlers.InternalEvents;

namespace Crash.Handlers.Plugins.Geometry.Create
{
	/// <summary>Handles unselction</summary>
	internal sealed class GeometryUnlockAction : IChangeCreateAction
	{
		public ChangeAction Action => ChangeAction.Unlocked;


		public bool CanConvert(object sender, CreateRecieveArgs crashArgs)
		{
			return crashArgs.Args is CrashSelectionEventArgs cargs &&
				   !cargs.Selected;
		}

		public bool TryConvert(object sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes)
		{
			changes = Array.Empty<Change>();
			if (crashArgs.Args is not CrashSelectionEventArgs cargs)
			{
				return false;
			}

			if (!crashArgs.Doc.Tables.TryGet<RealisedChangeTable>(out var realisedTable))
			{
				return false;
			}

			var userName = crashArgs.Doc.Users.CurrentUser.Name;

			changes = getChanges(cargs.CrashObjects, userName);

			foreach (var change in changes)
			{
				realisedTable.RemoveSelected(change.Id);
			}

			return true;
		}

		private IEnumerable<Change> getChanges(IEnumerable<CrashObject> crashObjects, string userName)
		{
			var changes = new List<Change>();
			foreach (var crashObject in crashObjects)
			{
				changes.Add(CreateChange(crashObject.ChangeId, userName));
			}

			return changes;
		}

		private Change CreateChange(Guid changeId, string userName)
		{
			return GeometryChange.CreateChange(changeId, userName, ChangeAction.Unlocked);
		}
	}
}
