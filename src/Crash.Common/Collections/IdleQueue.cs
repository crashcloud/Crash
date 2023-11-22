using System.Collections;

using Crash.Common.Document;
using Crash.Common.Events;

namespace Crash.Events
{
	/// <summary>A Queue for running during the Rhino Idle Event.</summary>
	public sealed class IdleQueue : IEnumerable<IdleAction>
	{
		private readonly CrashDoc _hostDoc;
		private readonly ConcurrentQueue<IdleAction> _idleQueue;

		/// <summary>Constructs an Idle Queue</summary>
		public IdleQueue(CrashDoc hostDoc)
		{
			_hostDoc = hostDoc;
			_idleQueue = new ConcurrentQueue<IdleAction>();
		}

		/// <summary>The number of items in the Queue</summary>
		public int Count => _idleQueue.Count;

		/// <summary>GetEnumerator</summary>
		public IEnumerator<IdleAction> GetEnumerator()
		{
			return _idleQueue.GetEnumerator();
		}

		/// <summary>GetEnumerator</summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _idleQueue.GetEnumerator();
		}

		/// <summary>Adds an Action to the Queue</summary>
		public void AddAction(IdleAction action)
		{
			if (_idleQueue.All(iq => iq.Name?.Equals(action.Name) != true))
			{
				_idleQueue.Enqueue(action);
			}
		}

		/// <summary>Attempts to run the next Action</summary>
		public void RunNextAction()
		{
			if (_idleQueue.IsEmpty)
			{
				return;
			}

			if (!_idleQueue.TryDequeue(out var action))
			{
				return;
			}

			action?.Invoke();

			if (_idleQueue.IsEmpty)
			{
				OnCompletedQueue?.Invoke(this, new CrashEventArgs(_hostDoc));
			}
		}

		/// <summary>Fires when the queue has finished parsing more than 1 item.</summary>
		public event EventHandler<CrashEventArgs> OnCompletedQueue;
	}
}
