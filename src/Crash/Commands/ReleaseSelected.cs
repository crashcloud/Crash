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


		public static ReleaseSelected Instance { get; private set; }


		public override string EnglishName => "ReleaseSelected";


		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			var selectedChanges = GetSelectedChanges(doc);
			if (selectedChanges.Count() <= 0)
			{
				return Result.Cancel;
			}

			// TODO : Wait for response for data integrity check
			var crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			crashDoc?.LocalClient?.DoneRangeAsync(selectedChanges);

			return Result.Success;
		}

		private static IEnumerable<Guid> GetSelectedChanges(RhinoDoc doc)
		{
			foreach (var rhinoObj in doc.Objects.GetSelectedObjects(false, false))
			{
				if (!rhinoObj.TryGetChangeId(out var id))
				{
					continue;
				}

				yield return id;
			}
		}
	}
}
