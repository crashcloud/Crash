using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;

namespace Crash.Common.Tests
{
	public sealed class IdleQueueTests
	{
		[Test]
		[Parallelizable]
		public void Count_ReturnsZero_WhenQueueIsEmpty()
		{
			// Arrange
			var queue = new IdleQueue(new CrashDoc());

			// Act
			var count = queue.Count;

			// Assert
			Assert.That(count, Is.EqualTo(0));
		}

		[Test]
		[Parallelizable]
		public async Task Count_ReturnsCorrectCount_AfterEnqueueingItemsAsync()
		{
			var expectedCount = 3;

			// Arrange
			var queue = new IdleQueue(new CrashDoc());
			for (var i = 0; i < expectedCount; i++)
			{
				queue.AddAction(new IdleAction(args => { }, new IdleArgs(null, null)));
			}

			// Act
			var realCount = queue.Count;

			// Assert
			Assert.That(expectedCount, Is.EqualTo(realCount));
		}

		[Test]
		[Parallelizable]
		public void RunNextAction_DoesNothing_WhenQueueIsEmpty()
		{
			// Arrange
			var queue = new IdleQueue(new CrashDoc());

			// Act
			queue.RunNextAction();

			// Assert
			Assert.That(queue.Count, Is.EqualTo(0));
		}

		[Test]
		[Parallelizable]
		public async Task RunNextAction_InvokesAction_WhenQueueIsNotEmpty()
		{
			// Arrange
			var queue = new IdleQueue(new CrashDoc());
			var action = new IdleAction(DisposableCrashEvent, new IdleArgs(null, null));
			queue.AddAction(action);

			// Act
			queue.RunNextAction();

			// Assert
			Assert.IsTrue(action.Invoked);
		}

		[Test]
		[Parallelizable]
		public async Task RunNextAction_RemovesActionFromQueue_AfterInvoking()
		{
			var expectedCount = 3;

			// Arrange
			var queue = new IdleQueue(new CrashDoc());
			for (var i = 0; i < expectedCount; i++)
			{
				queue.AddAction(new IdleAction(DisposableCrashEvent, new IdleArgs(null, null)));
			}

			// Act
			var realCount = queue.Count;

			// Act
			queue.RunNextAction();

			// Assert
			Assert.That(queue.Count, Is.EqualTo(expectedCount - 1));
		}

		[Test]
		[Parallelizable]
		public void RunNextAction_NoInvokeOnCompletedQueueEvent_WhenQueueIsEmpty()
		{
			// Arrange
			var queue = new IdleQueue(new CrashDoc());
			var eventRaised = false;
			queue.OnCompletedQueue += (sender, args) => { eventRaised = true; };

			// Act
			queue.RunNextAction();

			// Assert
			Assert.IsFalse(eventRaised);
		}

		[Test]
		[Parallelizable]
		public async Task RunNextAction_InvokeOnCompletedQueueEvent()
		{
			// Arrange
			var queue = new IdleQueue(new CrashDoc());
			queue.AddAction(new IdleAction(DisposableCrashEvent, new IdleArgs(null, null)));

			var eventRaised = false;
			queue.OnCompletedQueue += (sender, args) => { eventRaised = true; };

			// Act
			queue.RunNextAction();

			// Assert
			Assert.IsTrue(eventRaised);
		}


		private void DisposableCrashEvent(IdleArgs args) { }
	}
}
