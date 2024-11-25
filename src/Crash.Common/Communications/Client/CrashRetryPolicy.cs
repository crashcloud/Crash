using Microsoft.AspNetCore.SignalR.Client;

namespace Crash.Common.Communications;

internal class CrashRetryPolicy : IRetryPolicy
{

	private Dictionary<int, TimeSpan> retryDelays { get; } = new()
	{
	  { 0, TimeSpan.FromMilliseconds(10)},
	  { 1, TimeSpan.FromMilliseconds(50)},
	  { 2, TimeSpan.FromMilliseconds(100)},
	  { 3, TimeSpan.FromSeconds(1)},
	  { 4, TimeSpan.FromSeconds(2.5)},
	  { 5, TimeSpan.FromSeconds(5)},
	};

	public TimeSpan? NextRetryDelay(RetryContext retryContext)
	{
		int index = (int)retryContext.PreviousRetryCount;
		if (retryDelays.TryGetValue(index, out TimeSpan delay))
			return delay;

		return null;
	}
}
