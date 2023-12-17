using Crash.UI.UsersView;

using Rhino.Commands;

namespace Crash.Commands
{
	/// <summary>Toggles the Crash Users UI</summary>
	public sealed class ShowCrashUsers : Command
	{
		public override string EnglishName => nameof(ShowCrashUsers);

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			UsersForm.ShowForm();
			return Result.Success;
		}
	}
}
