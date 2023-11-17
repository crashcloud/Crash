using Crash.Common.Changes;
using Crash.Common.Document;

using Rhino.Commands;

namespace Crash.Commands
{
	/// <summary>Command to Release Changes</summary>
	[CommandStyle(Style.DoNotRepeat | Style.NotUndoable)]
	public sealed class Release : AsyncCommand
	{
		public Release()
		{
			Instance = this;
		}

		public static Release Instance { get; private set; }

		public override string EnglishName => "Release";

		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc CrashDoc, RunMode mode)
		{
			if (CrashDoc?.LocalClient is null)
			{
				RhinoApp.WriteLine("You aren't in a shared model.");
				return Result.Failure;
			}

			var doneChange = DoneChange.GetDoneChange(CrashDoc.Users.CurrentUser.Name);
			await CrashDoc.LocalClient.PushChangeAsync(doneChange);
			return Result.Success;
		}
	}
}
