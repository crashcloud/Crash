using System.Text.Json;

using Crash.Handlers.Utils;

using Rhino;
using Rhino.DocObjects;

using RhinoLayer = Rhino.DocObjects.Layer;

namespace Crash.Handlers.Plugins.Layers
{
	public class CrashLayer
	{
#if NETFRAMEWORK
		internal static readonly string Separator = RhinoLayer.PathSeparator;
#else
		internal static readonly string Separator = ModelComponent.NamePathSeparator;
#endif

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

			rhinoLayer.Name = GetLayerNameFromPath(FullPath);
			rhinoLayer.Index = Index;
			rhinoLayer.Id = Id;
			rhinoLayer.IsVisible = IsVisible;
			rhinoLayer.IsLocked = IsLocked;
			rhinoLayer.IsExpanded = IsExpanded;

			return rhinoLayer;
		}

		public static RhinoLayer UpdateRhinoLayer(RhinoDoc rhinoDoc, CrashLayer crashLayer, RhinoLayer rhinoLayer)
		{
			rhinoLayer.Name = GetLayerNameFromPath(crashLayer.FullPath);
			rhinoLayer.Index = crashLayer.Index;
			rhinoLayer.Id = crashLayer.Id;
			rhinoLayer.IsVisible = crashLayer.IsVisible;
			rhinoLayer.IsLocked = crashLayer.IsLocked;
			rhinoLayer.IsExpanded = crashLayer.IsExpanded;

			return rhinoLayer;
		}

		public static void UpdateRegisteredLayer(RhinoDoc rhinoDoc, CrashLayer crashLayer)
		{
			if (crashLayer.IsDeleted)
			{
				rhinoDoc.Layers.Undelete(crashLayer.Index);
			}
			else
			{
				rhinoDoc.Layers.Delete(crashLayer.Index, false);
			}

			if (crashLayer.Current)
			{
				rhinoDoc.Layers.SetCurrentLayerIndex(crashLayer.Index, false);
			}
		}

		internal static string[] GetLayerLineage(string fullPath)
		{
			return fullPath.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
		}

		internal static string GetFullPath(IEnumerable<string> ancestors)
		{
			return string.Join(Separator, ancestors);
		}

		internal static string GetLayerNameFromPath(string fullPath)
		{
			return GetLayerLineage(fullPath)?.LastOrDefault() ?? string.Empty;
		}
	}
}
