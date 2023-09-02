using Crash.Handlers.Changes;
using Crash.Handlers.InternalEvents;
using Crash.Utils;

namespace Crash.Handlers.Plugins.Geometry.Create
{
	/// <summary>Handles unselction</summary>
	internal sealed class GeometryUnSelectAction : IChangeCreateAction
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

			var userName = crashArgs.Doc.Users.CurrentUser.Name;

			if (cargs.DeselectAll)
			{
				var guids = ChangeUtils.GetSelected().ToList();
				ChangeUtils.ClearSelected();
				changes = getChanges(guids, userName);
			}
			else
			{
				changes = getChanges(cargs.CrashObjects, userName);
			}

			return true;
		}

		private IEnumerable<Change> getChanges(IEnumerable<CrashObject> crashObjects, string userName)
		{
			foreach (var crashObject in crashObjects)
			{
				yield return CreateChange(crashObject.ChangeId, userName);
			}
		}

		private IEnumerable<Change> getChanges(IEnumerable<Guid> changeIds, string userName)
		{
			foreach (var changeId in changeIds)
			{
				yield return CreateChange(changeId, userName);
			}
		}

		private Change CreateChange(Guid changeId, string userName)
		{
			return GeometryChange.CreateChange(changeId, userName, ChangeAction.Unlocked);
		}
	}
}
