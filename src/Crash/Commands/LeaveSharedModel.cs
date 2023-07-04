using Crash.Handlers;

using Rhino.Commands;

namespace Crash.Commands
{

	/// <summary>Command to Close a Shared Model</summary>
	[CommandStyle(Style.ScriptRunner)]
	public sealed class LeaveSharedModel : Command
	{
		private bool defaultValue = false;

		/// <summary>Default Constructor</summary>
		public LeaveSharedModel()
		{
			Instance = this;
		}

		/// <inheritdoc />
		public static LeaveSharedModel Instance { get; private set; }

		/// <inheritdoc />
		public override string EnglishName => "LeaveSharedModel";

		/// <inheritdoc />
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			Client.CrashClient? client = CrashDocRegistry.ActiveDoc?.LocalClient;
			if (null == client) return Result.Success;

			bool? choice = _GetReleaseChoice();
			if (null == choice)
				return Result.Cancel;

			if (choice.Value == true)
				client.DoneAsync();

			CrashDocRegistry.ActiveDoc?.Dispose();
			InteractivePipe.Active.Enabled = false;

			_EmptyModel(doc);

			RhinoApp.WriteLine("Model closed and saved successfully");

			doc.Views.Redraw();
			UsersForm.CloseActiveForm();

			return Result.Success;
		}

		private bool? _GetReleaseChoice()
			=> SelectionUtils.GetBoolean(ref defaultValue,
				"Would you like to Release before exiting?",
				"JustExit",
				"ReleaseThenExit");

		private void _EmptyModel(Rhino.RhinoDoc doc)
		{
			doc.Objects.Clear();
		}

	}

}
