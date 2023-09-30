﻿using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Handlers;
using Crash.UI.UsersView;

using Rhino.Commands;

namespace Crash.Commands
{
	/// <summary>Command to Close a Shared Model</summary>
	[CommandStyle(Style.ScriptRunner)]
	public sealed class LeaveSharedModel : AsyncCommand
	{
		private bool defaultValue;

		public LeaveSharedModel()
		{
			Instance = this;
		}


		public static LeaveSharedModel Instance { get; private set; }


		public override string EnglishName => "LeaveSharedModel";


		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc CrashDoc, RunMode mode)
		{
			var client = CrashDoc?.LocalClient;
			if (client is null)
			{
				RhinoApp.WriteLine("You aren't in a shared model.");
				return Result.Failure;
			}

			var choice = _GetReleaseChoice();
			switch (choice)
			{
				case null:
					return Result.Cancel;
				case true:
					var doneChange = DoneChange.GetDoneChange(CrashDoc.Users.CurrentUser.Name);
					await client.PushChangeAsync(doneChange);
					break;
			}

			CrashDocRegistry.ActiveDoc?.Dispose();
			InteractivePipe.Active.Enabled = false;

			_EmptyModel(doc);

			RhinoApp.WriteLine("Model closed and saved successfully");

			doc.Views.Redraw();
			UsersForm.CloseActiveForm();

			return Result.Success;
		}

		private bool? _GetReleaseChoice()
		{
			return SelectionUtils.GetBoolean(ref defaultValue,
			                                 "Would you like to Release before exiting?",
			                                 "JustExit",
			                                 "ReleaseThenExit");
		}

		private static void _EmptyModel(RhinoDoc doc)
		{
			doc.Objects.Clear();
		}
	}
}
