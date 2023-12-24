using Crash.Changes;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;

namespace Crash.Common.Tests.Events
{
	public sealed class IdleActionTests
	{
		[Test]
		public void AttemptDoubleInvoke_InvokesOnce()
		{
			var doc = new CrashDoc();
			var change = new Change();
			var idleArgs = new IdleArgs(doc, change);

			var invokeCount = 0;

			Action<IdleArgs> action = args =>
			                          {
				                          invokeCount += 1;
			                          };

			var idleAction = new IdleAction(action, idleArgs);

			Assert.That(invokeCount, Is.EqualTo(0));

			idleAction.Invoke();
			Assert.That(invokeCount, Is.EqualTo(1));

			idleAction.Invoke();
			Assert.That(invokeCount, Is.EqualTo(1));
		}
	}
}
