using Crash.Handlers.Changes;
using Crash.Handlers.InternalEvents;

namespace Crash.Handlers.Plugins.Geometry.Create
{
	/// <summary>Handles Removed Objects</summary>
	internal sealed class GeometryRemoveAction : IChangeCreateAction
	{
		public ChangeAction Action => ChangeAction.Remove;


		public bool CanConvert(object? sender, CreateRecieveArgs crashArgs)
		{
			return crashArgs.Args is CrashObjectEventArgs rargs &&
			       rargs.ChangeId != Guid.Empty;
		}


		public bool TryConvert(object? sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes)
		{
			changes = Array.Empty<Change>();
			if (crashArgs.Args is not CrashObjectEventArgs rargs)
			{
				return false;
			}

			if (rargs.ChangeId == Guid.Empty)
			{
				return false;
			}

			var _user = crashArgs.Doc.Users.CurrentUser.Name;
			var removeChange =
				GeometryChange.CreateChange(rargs.ChangeId, _user, ChangeAction.Remove);

			changes = new List<Change> { removeChange };

			return true;
		}
	}
}
