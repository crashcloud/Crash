using System.Drawing;
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
			Id = changeId;

			IsDeleted = layer.IsDeleted;

			// Styles
			Color = layer.Color;
			PlotColor = layer.PlotColor;
			PlotWeight = layer.PlotWeight;
			LinetypeIndex = layer.LinetypeIndex;

			// User Specific
			IsVisible = layer.IsVisible;
			IsLocked = layer.IsLocked;
			IsExpanded = layer.IsExpanded;
		}

		public string FullPath { get; }
		public int Index { get; }

		public Guid Id { get; }
		public bool IsDeleted { get; internal set; }
		public bool IsVisible { get; internal set; }
		public bool IsLocked { get; internal set; }
		public bool IsExpanded { get; internal set; }
		public bool Current { get; internal set; }

		public int LinetypeIndex { get; private set; }

		public double PlotWeight { get; private set; }

		public Color PlotColor { get; private set; }

		public Color Color { get; private set; }

		public static CrashLayer CreateFrom(IChange change)
		{
			var packet = JsonSerializer.Deserialize<PayloadPacket>(change.Payload);

			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.FullPath), change.Owner, string.Empty, out var fullPath);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.Index), change.Owner, -1, out var index);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.IsDeleted), change.Owner, false, out var isDeleted);

			// Styles
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.Color), change.Owner, Color.Black, out var colour);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.PlotColor), change.Owner, Color.Black,
											out var plotColour);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.PlotWeight), change.Owner, 0.0, out var plotWeight);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.LinetypeIndex), change.Owner, -1, out var linetypeIndex);

			// User Specific
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.IsVisible), change.Owner, true, out var isVisible);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.IsLocked), change.Owner, false, out var isLocked);
			packet.Updates.TryGetLayerValue(nameof(RhinoLayer.IsExpanded), change.Owner, true, out var isExpanded);

			return new CrashLayer(fullPath, index, change.Id)
			{
				IsDeleted = isDeleted,

				// Styles
				Color = colour,
				PlotColor = plotColour,
				PlotWeight = plotWeight,
				LinetypeIndex = linetypeIndex,

				// User Specific
				IsVisible = isVisible,
				IsLocked = isLocked,
				IsExpanded = isExpanded
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

			// Styles
			rhinoLayer.Color = Color;
			rhinoLayer.PlotColor = PlotColor;
			rhinoLayer.PlotWeight = PlotWeight;
			rhinoLayer.LinetypeIndex = LinetypeIndex;

			// User Specific
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

			// Styles
			rhinoLayer.Color = crashLayer.Color;
			rhinoLayer.PlotColor = crashLayer.PlotColor;
			rhinoLayer.PlotWeight = crashLayer.PlotWeight;
			rhinoLayer.LinetypeIndex = crashLayer.LinetypeIndex;

			// User Specific
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
