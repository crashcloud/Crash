using Crash.Common.Document;
using Crash.Common.Events;

namespace Crash.Handlers.Plugins.Request
{
	public sealed class RequestEventArgs : CrashEventArgs
	{
		public readonly string RequestedName;

		public RequestEventArgs(CrashDoc crashDoc, string requestedName) : base(crashDoc)
		{
			RequestedName = requestedName;
		}
	}
}
