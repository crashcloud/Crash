using System.Text.Json;

namespace Crash.Handlers.Plugins.Layers
{
	public sealed class LayerChange : IChange
	{
		public const string ChangeType = "Crash.LayerChange";

		private LayerChange() { }

		public DateTime Stamp { get; }
		public Guid Id { get; }
		public string? Owner { get; }
		public string? Payload { get; }
		public string Type { get; }
		public ChangeAction Action { get; set; }

		public static Change CreateChange(string owner, Guid changeId, ChangeAction action,
			Dictionary<string, string> updates)
		{
			var packet = new PayloadPacket { Updates = updates, Data = string.Empty };
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
