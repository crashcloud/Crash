using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Handlers;

using Rhino.Commands;

namespace Crash.Commands
{
	public class RequestChange : Command
	{
		public override string EnglishName => "RequestChange";

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			var crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			if (crashDoc is null)
			{
				return Result.Cancel;
			}

			// EventDispatcher.Instance.NotifyServerAsync(ChangeAction.Add, this,)

			return Result.Success;
		}
	}

	public sealed class RequestEventArgs : CrashEventArgs
	{
		public RequestEventArgs(CrashDoc crashDoc) : base(crashDoc)
		{
		}

		public Guid RequestedId { get; set; }
	}
}
