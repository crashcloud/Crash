using System.Text.Json;

namespace Crash.Handlers.Plugins.Layers
{
	public sealed class LayerChange
	{
		public const string ChangeType = "Crash.LayerChange";


		public static Change CreateChange(string owner, CrashLayer layer, ChangeAction action,
			Dictionary<string, string> updates)
		{
			var packet = new PayloadPacket { Updates = updates };
			return new Change
			       {
				       Stamp = DateTime.Now,
				       Id = layer.ChangeId,
				       Owner = owner,
				       Type = ChangeType,
				       Action = action,
				       Payload = JsonSerializer.Serialize(packet)
			       };
		}
	}
}
