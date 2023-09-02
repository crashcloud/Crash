using Crash.Handlers;
using Crash.UI.UsersView;

using Rhino.Commands;

namespace Crash.Commands
{
	/// <summary>Command to Close a Shared Model</summary>
	[CommandStyle(Style.ScriptRunner)]
	public sealed class LeaveSharedModel : Command
	{
		private bool defaultValue;

		/// <summary>Default Constructor</summary>
		public LeaveSharedModel()
		{
			Instance = this;
		}

		
		public static LeaveSharedModel Instance { get; private set; }

		
		public override string EnglishName => "LeaveSharedModel";

		
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			var client = CrashDocRegistry.ActiveDoc?.LocalClient;
			if (client is null)
			{
				RhinoApp.WriteLine("You aren't in a shared model.");
				return Result.Success;
			}

			var choice = _GetReleaseChoice();
			switch (choice)
			{
				case null:
					return Result.Cancel;
				case true:
					client.DoneAsync();
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
