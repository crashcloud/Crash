using Crash.Common.Document;
using Crash.Handlers.Changes;
using Crash.Handlers.InternalEvents;

namespace Crash.Handlers.Plugins.Geometry.Create
{
	/// <summary>Handles Selection</summary>
	internal sealed class GeometrySelectAction : IChangeCreateAction
	{
		public ChangeAction Action => ChangeAction.Locked;


		public bool CanConvert(object sender, CreateRecieveArgs crashArgs)
		{
			return crashArgs.Args is CrashSelectionEventArgs cargs && cargs.Selected;
		}


		public bool TryConvert(object sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes)
		{
			changes = Array.Empty<Change>();
			if (crashArgs.Args is not CrashSelectionEventArgs cargs)
			{
				return false;
			}

			var userName = crashArgs.Doc.Users.CurrentUser.Name;
			changes = GetChanges(crashArgs.Doc, cargs.CrashObjects, userName);

			return true;
		}

		private IEnumerable<Change> GetChanges(CrashDoc crashDoc, IEnumerable<CrashObject> crashObjects,
			string userName)
		{
			var changes = new List<Change>(crashObjects.Count());
			foreach (var crashObject in crashObjects)
			{
				if (crashObject.ChangeId == Guid.Empty)
				{
					continue;
				}

				var change = GeometryChange.CreateChange(crashObject.ChangeId, userName, ChangeAction.Locked);
				changes.Add(change);
			}

			return changes;
		}
	}
}
