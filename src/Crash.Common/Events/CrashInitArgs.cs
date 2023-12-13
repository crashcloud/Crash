using Crash.Common.Document;

namespace Crash.Common.Events
{
	public sealed class CrashInitArgs : CrashEventArgs
	{
		public readonly IEnumerable<Change> Changes;

		public CrashInitArgs(CrashDoc crashDoc, IEnumerable<Change> changes)
			: base(crashDoc)
		{
			Changes = changes;
		}
	}
}
