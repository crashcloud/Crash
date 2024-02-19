using System.Text.Json;

namespace Crash.Handlers.Plugins.Layers
{
	public sealed class LayerChange : IChange
	{
		public const string ChangeType = "Crash.LayerChange";


		public CrashLayer Layer { get; private set; }

		public DateTime Stamp { get; private set; }
		public Guid Id { get; private set; }
		public string? Owner { get; private set; }
		public string? Payload { get; private set; }
		public string Type { get; private set; }
		public ChangeAction Action { get; set; }


		public static Change CreateChange(string owner, CrashLayer layer, ChangeAction action,
			Dictionary<string, string> updates)
		{
			var packet = new PayloadPacket { Updates = updates };
			return new Change
			       {
				       Stamp = DateTime.Now,
				       Id = layer.Id,
				       Owner = owner,
				       Type = ChangeType,
				       Action = action,
				       Payload = JsonSerializer.Serialize(packet)
			       };
		}

		public static bool TryCreateLayerChange(IChange change, out LayerChange layerChange)
		{
			var payload = JsonSerializer.Deserialize<PayloadPacket>(change.Payload);
			if (payload is null)
			{
				layerChange = default;
				return false;
			}

			layerChange = new LayerChange
			              {
				              Stamp = DateTime.Now,
				              Id = change.Id,
				              Owner = change.Owner,
				              Type = ChangeType,
				              Action = change.Action,
				              Payload = change.Payload,
				              Layer = CrashLayer.CreateFrom(change)
			              };

			return true;
		}
	}
}
