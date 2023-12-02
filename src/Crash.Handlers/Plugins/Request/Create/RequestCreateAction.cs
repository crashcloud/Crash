using System.Text.Json;

namespace Crash.Handlers.Plugins.Request.Create
{
	public class RequestCreateAction : IChangeCreateAction
	{
		public ChangeAction Action => ChangeAction.Add;

		public bool TryConvert(object sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes)
		{
			var packet = new PayloadPacket();
			// packet.Updates.Add("RequestedChangeId",)
			var requestChange = new Change
			                    {
				                    Id = Guid.NewGuid(),
				                    Action = ChangeAction.Add,
				                    Payload = JsonSerializer.Serialize(packet),
				                    Stamp = DateTime.UtcNow
			                    };
			throw new NotImplementedException();
		}

		public bool CanConvert(object sender, CreateRecieveArgs crashArgs)
		{
			return false;
		}
	}
}
