using Crash.Common.Communications;
using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Handlers;
using Crash.UI.UsersView;

using Rhino.Commands;
using Rhino.UI;

namespace Crash.Commands
{
	/// <summary>Command to Close a Shared Model</summary>
	[CommandStyle(Style.ScriptRunner)]
	public sealed class LeaveSharedModel : AsyncCommand
	{
		private bool defaultValue;

		public override string EnglishName => EnglishCommandName;
		public const string EnglishCommandName = "LeaveSharedModel";

		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc crashDoc, RunMode mode)
		{
			if (crashDoc?.LocalClient?.IsConnected != true)
			{
				RhinoApp.WriteLine("You are not connected to a Shared Model currently.");
				return Result.Cancel;
			}

			var choice = _GetReleaseChoice();
			switch (choice)
			{
				case null:
					return Result.Cancel;
				case true:
					var doneChange = DoneChange.GetDoneChange(crashDoc.Users.CurrentUser.Name);
					await crashDoc.LocalClient.StreamChangesAsync(new List<Change> { doneChange }.ToAsyncEnumerable());
					break;
			}

			doc.Objects.UnselectAll();

			(crashDoc.LocalClient as CrashClient).ClosedByUser = true;
			await CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);
			var pipe = InteractivePipe.GetActive(crashDoc);
			pipe.Enabled = false;

			RhinoApp.WriteLine("Model closed and saved successfully");

			doc.Views.Redraw();
			UsersForm.CloseActiveForm(crashDoc);
			LoadingUtils.Close(crashDoc);

			return Result.Success;
		}

		private bool? _GetReleaseChoice()
		{
			return SelectionUtils.GetBoolean(ref defaultValue,
											 "Would you like to Release your Changes before exiting?",
											 "JustExit",
											 "ReleaseThenExit");
		}
	}
}
