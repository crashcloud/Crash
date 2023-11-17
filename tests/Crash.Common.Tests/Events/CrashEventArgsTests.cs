using Crash.Common.Document;
using Crash.Common.Events;

namespace Crash.Common.Tests.Events
{
	public sealed class CrashEventArgsTests
	{
		[Test]
		public void Constructor()
		{
			var crashDoc = new CrashDoc();
			var args = new CrashEventArgs(crashDoc);
		}
	}
}
