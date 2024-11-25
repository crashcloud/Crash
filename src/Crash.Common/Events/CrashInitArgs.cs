using Crash.Common.Document;

namespace Crash.Common.Events
{

	public sealed class CrashInitArgs : CrashEventArgs
	{

		public int ChangeCount { get; }

		public CrashInitArgs(CrashDoc crashDoc, int changeCount)
			: base(crashDoc)
		{
			ChangeCount = changeCount;
		}

	}

}
