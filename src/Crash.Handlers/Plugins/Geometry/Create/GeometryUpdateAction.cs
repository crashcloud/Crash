using Crash.Common.Document;
using Crash.Handlers.Changes;
using Crash.Handlers.InternalEvents;

namespace Crash.Handlers.Plugins.Geometry.Create
{
	/// <summary>Captures Creation of default Rhino Geometry</summary>
	internal sealed class GeometryUpdateAction : IChangeCreateAction
	{
		public ChangeAction Action => ChangeAction.Update;

		public bool CanConvert(object? sender, CreateRecieveArgs crashArgs)
		{
			return crashArgs.Args is CrashUpdateArgs args && args.Updates.Count > 0;
		}

		public bool TryConvert(object? sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes)
		{
			if (crashArgs.Args is not CrashUpdateArgs args)
			{
				changes = Array.Empty<Change>();
				return false;
			}

			changes = CreateUpdatesChange(crashArgs.Doc, args.CrashObject.ChangeId, args.Updates);
			return changes.Any();
		}

		private Change[] CreateUpdatesChange(CrashDoc crashDoc, Guid changeId, Dictionary<string, string> updates)
		{
			var userName = crashDoc.Users.CurrentUser.Name;
			var change = GeometryChange.CreateChange(changeId, userName, Action, null, updates);

			return new[] { change };
		}
	}
}
