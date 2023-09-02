using Crash.Handlers;

using Rhino.Commands;

namespace Crash.Commands
{
	/// <summary>Command to Release Changes</summary>
	[CommandStyle(Style.DoNotRepeat | Style.NotUndoable)]
	public sealed class Release : Command
	{
		/// <summary>Default Constructor</summary>
		public Release()
		{
			Instance = this;
		}

		public static Release Instance { get; private set; }

		
		public override string EnglishName => "Release";

		
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			// TODO : Wait for response for data integrity check
			var crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			crashDoc?.LocalClient?.DoneAsync();

			return Result.Success;
		}
	}
}
