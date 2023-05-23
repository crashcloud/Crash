using Crash.Common.Document;
using Crash.Handlers;
using Crash.Utils;

using Rhino.Commands;


namespace Crash.Commands
{

	/// <summary>Command to Release Changes</summary>
	[CommandStyle(Style.DoNotRepeat | Style.NotUndoable)]
	public sealed class ReleaseSelected : Command
	{

		/// <summary>Default Constructor</summary>
		public ReleaseSelected()
		{
			Instance = this;
		}

		/// <inheritdoc />
		public static ReleaseSelected Instance { get; private set; }

		/// <inheritdoc />
		public override string EnglishName => "Release";

		/// <inheritdoc />
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			// TODO : Wait for response for data integrity check
			CrashDoc? crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			var selectedObjects = doc.Objects.GetSelectedObjects(true, false);

			List<Guid> selectedChanges = new List<Guid>(selectedObjects.Count());
			foreach(var rObj in selectedObjects)
			{
				if (!ChangeUtils.TryGetChangeId(rObj, out Guid changeId)) continue;
				selectedChanges.Add(changeId);
			}

			crashDoc?.LocalClient?.DoneAsync(selectedChanges);

			return Result.Success;
		}

	}

}
