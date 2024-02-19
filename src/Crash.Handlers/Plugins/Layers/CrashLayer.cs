using System.Text.Json;

using Crash.Handlers.Utils;

using Rhino;

using RhinoLayer = Rhino.DocObjects.Layer;

namespace Crash.Handlers.Plugins.Layers
{
	public class CrashLayer
	{
		internal CrashLayer(string fullPath, int index, Guid changeId)
		{
			FullPath = fullPath;
			Index = index;
			Id = changeId;
		}

		internal CrashLayer(RhinoLayer layer, Guid changeId)
		{
			FullPath = layer.FullPath;
			Index = layer.Index;
			IsDeleted = layer.IsDeleted;
			IsVisible = layer.IsVisible;
			IsLocked = layer.IsLocked;
			IsExpanded = layer.IsExpanded;
			Id = changeId;
		}

		public string FullPath { get; set; }
		public int Index { get; set; }

		public Guid Id { get; set; }
		public bool IsDeleted { get; set; }
		public bool IsVisible { get; set; }
		public bool IsLocked { get; set; }
		public bool IsExpanded { get; set; }
		public bool Current { get; set; }

		public static CrashLayer CreateFrom(IChange change)
		{
			var packet = JsonSerializer.Deserialize<PayloadPacket>(change.Payload);

			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.FullPath), change.Owner, string.Empty, out var fullPath);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.Index), change.Owner, -1, out var index);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.IsDeleted), change.Owner, false, out var isDeleted);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.IsVisible), change.Owner, true, out var isVisible);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.IsLocked), change.Owner, false, out var isLocked);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.IsExpanded), change.Owner, true, out var isExpanded);
			// updates.TryGetValue(nameof(Layer.Current), out bool current);FullPath), out string fullName);

			return new CrashLayer(fullPath, index, change.Id)
			       {
				       IsDeleted = isDeleted, IsVisible = isVisible, IsLocked = isLocked, IsExpanded = isExpanded
				       // Current = current,
			       };
		}

		public RhinoLayer GetOrCreateRhinoLayer(RhinoDoc rhinoDoc)
		{
			var rhinoLayer = rhinoDoc.Layers.FindIndex(Index);
			if (rhinoLayer is null)
			{
				rhinoLayer = new RhinoLayer();
			}

			rhinoLayer.Name = FullPath.Split(new[] { RhinoLayerUtils.Separater }, StringSplitOptions.RemoveEmptyEntries)
			                          .Last();
			rhinoLayer.Index = Index;
			rhinoLayer.Id = Id;
			rhinoLayer.IsVisible = IsVisible;
			rhinoLayer.IsLocked = IsLocked;
			rhinoLayer.IsExpanded = IsExpanded;

			// rhinoLayer.FullPath = this.FullPath;

			// TODO : Resolve Deleted
			// rhinoLayer.IsDeleted = this.IsDeleted;
			// rhinoDoc.Layers.Delete()

			// TOOD : Resolve Current
			// rhinoLayer.Current = this.Current;

			return rhinoLayer;
		}
	}
}
