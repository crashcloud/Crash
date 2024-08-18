using Crash.Common.Document;
using Crash.Common.Tables;
using Crash.Handlers.Changes;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Utils;

using Rhino.Geometry;

namespace Crash.Handlers.Plugins.Geometry.Create
{
	/// <summary>Captures Creation of default Rhino Geometry</summary>
	internal sealed class GeometryCreateAction : IChangeCreateAction
	{
		public ChangeAction Action => ChangeAction.Add | ChangeAction.Temporary;

		public bool CanConvert(object sender, CreateRecieveArgs crashArgs)
		{
			return crashArgs.Args is CrashObjectEventArgs rargs &&
				   rargs.Geometry is not null;
		}

		public bool TryConvert(object sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes)
		{
			if (crashArgs.Args is not CrashObjectEventArgs cargs)
			{
				changes = Array.Empty<Change>();
				return false;
			}

			changes = CreateChangesFromArgs(crashArgs.Doc, cargs.RhinoId, cargs.Geometry, cargs.UnDelete,
											cargs.ChangeId);
			return changes.Any();
		}

		private Change[] CreateChangesFromArgs(CrashDoc crashDoc, Guid rhinoId, GeometryBase geometry,
			bool unDelete, Guid changeId = default)
		{
			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);
			if (rhinoDoc is null)
			{
				throw new NullReferenceException("Rhino Document cannot be found!");
			}

			var rhinoObject = rhinoDoc.Objects.FindId(rhinoId);
			if (rhinoObject is null)
			{
				return Array.Empty<Change>();
			}

			var userName = crashDoc.Users.CurrentUser.Name;

			if (!crashDoc.Tables.TryGet<RealisedChangeTable>(out var realTable))
			{
				return Array.Empty<Change>();
			}

			// For unDelete
			if (changeId == Guid.Empty && !realTable.TryGetChangeId(rhinoId, out changeId))
			{
				changeId = Guid.NewGuid();
			}

			realTable.AddPair(changeId, rhinoId);

			Change change;
			if (unDelete)
			{
				change = GeometryChange.CreateChange(changeId, userName, ChangeAction.Add | ChangeAction.Temporary);
			}
			else
			{
				var updates = ObjectAttributeComparisonUtils.GetDefaults(rhinoObject.Attributes, userName);
				change = GeometryChange.CreateChange(changeId, userName, Action, geometry, updates);
			}

			return new[] { change };
		}
	}
}
