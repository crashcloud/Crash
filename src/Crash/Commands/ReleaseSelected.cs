using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Utils;

using Rhino.Commands;

namespace Crash.Commands
{
	/// <summary>Command to Release Changes</summary>
	[CommandStyle(Style.DoNotRepeat | Style.NotUndoable)]
	public sealed class ReleaseSelected : AsyncCommand
	{
		public ReleaseSelected()
		{
			Instance = this;
		}


		public static ReleaseSelected Instance { get; private set; }


		public override string EnglishName => "ReleaseSelected";

		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc CrashDoc, RunMode mode)
		{
			var selectedChanges = GetSelectedChanges(doc);
			if (!selectedChanges.Any())
			{
				return Result.Cancel;
			}

			if (CrashDoc?.LocalClient is null)
			{
				RhinoApp.WriteLine("You aren't in a shared model.");
				return Result.Failure;
			}

			// TODO : Wait for response for data integrity check
			var user = CrashDoc.Users.CurrentUser.Name;
			await CrashDoc.LocalClient.PushIdenticalChangesAsync(selectedChanges, DoneChange.GetDoneChange(user));

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
