using Crash.Common.Changes;
using Crash.Common.Document;

using Rhino.Commands;

namespace Crash.Commands
{
	/// <summary>Command to Release Changes</summary>
	[CommandStyle(Style.DoNotRepeat | Style.NotUndoable)]
	public sealed class Release : AsyncCommand
	{

		public override string EnglishName => "Release";

		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc crashDoc, RunMode mode)
		{
			if (!CommandUtils.InSharedModel(crashDoc))
			{
				RhinoApp.WriteLine("You aren't in a shared model.");
				return Result.Failure;
			}

			var doneChange = DoneChange.GetDoneChange(crashDoc.Users.CurrentUser.Name);

			await crashDoc.LocalClient.StreamChangesAsync(new List<Change> { doneChange }.ToAsyncEnumerable());

			doc.Objects.UnselectAll();
			doc.Views.Redraw();

			return Result.Success;
		}
	}
}
