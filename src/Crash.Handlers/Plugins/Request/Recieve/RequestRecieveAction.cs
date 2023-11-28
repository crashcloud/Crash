using Crash.Changes.Extensions;
using Crash.Common.Document;

namespace Crash.Handlers.Plugins.Request.Recieve
{
	public class RequestRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.HasFlag(ChangeAction.Add);
		}

		public Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			if (!RequestChange.TryCreateFromPayload(recievedChange, out var requestChange))
			{
				return Task.CompletedTask;
			}

			crashDoc.TemporaryChangeTable.UpdateChange(requestChange);
		}
	}
}
