using Crash.Common.Document;
using Crash.Handlers;
using Crash.Utils;

using Rhino.Commands;


namespace Crash.Commands
{

	/// <summary>Command to Release Changes</summary>
	[CommandStyle(Style.DoNotRepeat | Style.NotUndoable | Style.Hidden)]
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
		public override string EnglishName => "ReleaseSelected";

		/// <inheritdoc />
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			IEnumerable<Guid> selectedChanges = getSelectedChanges(doc);
			if (selectedChanges.Count() <= 0)
				return Result.Cancel;

			// TODO : Wait for response for data integrity check
			CrashDoc? crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			crashDoc?.LocalClient?.DoneAsync(selectedChanges);

			return Result.Success;
		}

		private IEnumerable<Guid> getSelectedChanges(RhinoDoc doc)
		{
			foreach (var rhinoObj in doc.Objects.GetSelectedObjects(false, false))
			{
				if (!ChangeUtils.TryGetChangeId(rhinoObj, out Guid id))
					continue;

				yield return id;
			}
		}
	}

}
