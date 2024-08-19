using System;

namespace Crash.Common.Collections;

/*

			BackgroundWorker ServerSender = new ();
			ServerSender.
			ServerSender.RunWorkerAsync();
*/

public enum ChangeEquality
{
	Equal,
	NotEqual
}

public class DelayQueue<T>
{

	private Queue<T> _sendQueue { get; }

	private TimeSpan _delay { get; }

	private Func<T, T, bool> _groupable { get; }

	private DateTime _lastAdded { get; set; }

	public DelayQueue(TimeSpan delay, Func<T, T, bool> groupable)
	{
		_sendQueue = new();
		_delay = delay;
		_groupable = groupable;
	}

	public void Enqueue(T item)
	{
		_sendQueue.Enqueue(item);
		_lastAdded = DateTime.UtcNow;
	}

	public void Enqueue(IEnumerable<T> item)
	{
		_sendQueue.Enqueue(item);
		_lastAdded = DateTime.UtcNow;
	}

	private async Task Loop(CancellationToken cancellationToken)
	{
		while (true)
		{
			await Task.Delay(_delay, cancellationToken);
			if (_sendQueue.Count == 0) continue;
			if (DateTime.UtcNow - _lastAdded < _delay) continue;

			List<T> items = new();
			while (true)
			{
				if (_sendQueue.Count <= 0)
				{
					items.Add(_sendQueue.Dequeue());
					continue;
				}

				var peek = _sendQueue.Peek();
				if (!_groupable(items.Last(), peek))
					break;

				items.Add(_sendQueue.Dequeue());
			}

			OnReadyToSend?.Invoke(this, items);
		}
	}

	public async Task Start(CancellationToken cancellationToken = default)
	{
		await Loop(cancellationToken);
	}

	public EventHandler<List<T>> OnReadyToSend;

}
