using Crash.Common.Document;

namespace Crash.Common.Events
{
	public sealed class CrashChangeArgs : CrashEventArgs
	{
		public readonly IEnumerable<Change> Changes;

		public CrashChangeArgs(CrashDoc crashDoc, IEnumerable<Change> changes)
			: base(crashDoc)
		{
			Changes = changes;
		}
	}
}
