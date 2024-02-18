using System.Text.Json;

namespace Crash.Handlers.Plugins.Layers
{
	public sealed class LayerChange
	{
		public const string ChangeType = "Crash.LayerChange";


		public static Change CreateChange(string owner, Guid changeId, ChangeAction action,
			Dictionary<string, string> updates)
		{
			var packet = new PayloadPacket { Updates = updates };
			return new Change
			       {
				       Stamp = DateTime.Now,
				       Id = changeId,
				       Owner = owner,
				       Type = ChangeType,
				       Action = action,
				       Payload = JsonSerializer.Serialize(packet)
			       };
		}
	}
}
