using System.Text.Json;

namespace Crash.Handlers.Plugins.Request
{
	public sealed class RequestChange : IChange
	{
		public const string ChangeType = "Request.Temporary";
		public Guid RequestedId { get; private set; }

		public DateTime Stamp { get; private set; }
		public Guid Id { get; private set; }
		public string? Owner { get; private set; }
		public string? Payload { get; private set; }
		public string Type => ChangeType;
		public ChangeAction Action { get; set; }

		public static bool TryCreateFromPayload(IChange　recievedChange, out RequestChange change)
		{
			change = default;
			var payload = JsonSerializer.Deserialize<PayloadPacket>(recievedChange.Payload);

			if (!payload.Updates.TryGetValue("RequestedChangeId", out var requestedChangeIdString) ||
			    Guid.TryParse(requestedChangeIdString, out var requestedId))
			{
				return false;
			}

			change = new RequestChange
			         {
				         Id = recievedChange.Id,
				         RequestedId = requestedId,
				         Action = ChangeAction.Add,
				         Owner = recievedChange.Owner,
				         Payload = null,
				         Stamp = recievedChange.Stamp
			         };

			return true;
		}
	}
}
