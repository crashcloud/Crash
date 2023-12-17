using Crash.Handlers;
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
			var crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			if (crashDoc is null)
			{
				return Result.Failure;
			}

			UsersForm.ShowForm(crashDoc);
			return Result.Success;
		}
	}
}
