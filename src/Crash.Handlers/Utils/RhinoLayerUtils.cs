using System.Drawing;

using Rhino;
using Rhino.DocObjects;

namespace Crash.Handlers
{
	internal static class RhinoLayerUtils
	{
		private const string Separater = "::";
		private static readonly Layer s_defaultLayer;

		static RhinoLayerUtils()
		{
			s_defaultLayer = new Layer();
			s_gettersAndSetters = new Dictionary<string, GetterAndSetter>
			                      {
				                      {
					                      nameof(Layer.Name),
					                      new GetterAndSetter(layer => layer.Name, (layer, value) => layer.Name = value)
				                      },
				                      { nameof(Layer.FullPath), new(layer => layer.FullPath, (layer, value) => { }) },
				                      {
					                      nameof(Layer.Color),
					                      new GetterAndSetter(layer => layer.Color.ToArgb().ToString(),
					                                          (layer, value) =>
						                                          layer.Color = Color.FromArgb(int.Parse(value)))
				                      },
				                      {
					                      nameof(Layer.IsLocked),
					                      new GetterAndSetter(layer => layer.IsLocked.ToString(),
					                                          (layer, value) =>
						                                          layer.IsLocked = bool.Parse(value))
				                      },
				                      {
					                      nameof(Layer.IsVisible),
					                      new GetterAndSetter(layer => layer.IsVisible.ToString(),
					                                          (layer, value) =>
						                                          layer.IsVisible = bool.Parse(value))
				                      }
			                      };
		}

		private static Dictionary<string, GetterAndSetter> s_gettersAndSetters { get; }

		internal static Dictionary<string, string> GetLayerDefaults(Layer layer)
		{
			return GetLayerDifference(s_defaultLayer, layer);
		}

		internal static Dictionary<string, string> GetLayerDifference(Layer oldState, Layer newState)
		{
			var dict = new Dictionary<string, string>();

			foreach (var getter in s_gettersAndSetters)
			{
				if (!IsDifferent(getter.Value.Get, oldState, newState, out var oldValue, out var newValue))
				{
					continue;
				}

				dict.Add(GetNewKey(getter.Key), newValue);
				dict.Add(GetOldKey(getter.Key), oldValue);
			}

			return dict;
		}

		internal static void SetLayerFullPath(RhinoDoc doc, Layer oldLayer, string layerFullPath)
		{
			if (oldLayer.FullPath.Equals(layerFullPath))
			{
			}
		}

		internal static string GetNewKey(string key)
		{
			return $"New_{key}";
		}

		internal static string GetOldKey(string key)
		{
			return $"Old_{key}";
		}

		private static string GetNeutralKey(string key)
		{
			return key.Replace("Old", "").Replace("New", "");
		}

		internal static bool TryGetAtExpectedPath(RhinoDoc rhinoDoc, Dictionary<string, string> layerUpdates,
			out Layer layer)
		{
			return TryGetLayer(GetNewKey(nameof(Layer.FullPath)), rhinoDoc, layerUpdates, out layer);
		}

		internal static bool TryGetAtOldPath(RhinoDoc rhinoDoc, Dictionary<string, string> layerUpdates,
			out Layer layer)
		{
			return TryGetLayer(GetOldKey(nameof(Layer.FullPath)), rhinoDoc, layerUpdates, out layer);
		}

		private static bool TryGetLayer(string key, RhinoDoc rhinoDoc, Dictionary<string, string> layerUpdates,
			out Layer layer)
		{
			if (layerUpdates.TryGetValue(key, out var oldFullPath))
			{
				var layerIndex = rhinoDoc.Layers.FindByFullPath(oldFullPath, -1);
				layer = rhinoDoc.Layers.FindIndex(layerIndex);
				return layer is not null;
			}

			layer = default;
			return false;
		}

		internal static void UpdateLayer(Layer layer, Dictionary<string, string> values)
		{
			foreach (var kvp in values)
			{
				if (!s_gettersAndSetters.TryGetValue(GetNeutralKey(kvp.Key), out var setter))
				{
					continue;
				}

				setter.Set(layer, kvp.Value);
			}
		}

		private static bool IsDifferent(Func<Layer, string> getter, Layer oldState, Layer newState, out string oldValue,
			out string newValue)
		{
			oldValue = getter(oldState);
			newValue = getter(newState);

			if (!string.IsNullOrEmpty(oldValue))
			{
				return !oldValue.Equals(newValue);
			}

			return !string.IsNullOrEmpty(newValue);
		}

		public static Layer MoveLayerToExpectedPath(RhinoDoc rhinoDoc, Dictionary<string, string> layerUpdates)
		{
			TryGetAtOldPath(rhinoDoc, layerUpdates, out var originalLayer);
			var expectedPath = layerUpdates[GetNewKey(nameof(Layer.FullPath))];

			var lineage = expectedPath.Split(new[] { Separater }, StringSplitOptions.RemoveEmptyEntries).ToList();

			for (var i = 0; i < lineage.Count; i++)
			{
				var l = lineage.GetRange(0, i);
				var layer = string.Join(Separater, l);
				if (string.IsNullOrEmpty(layer))
				{
					continue;
				}

				var layerIndex = rhinoDoc.Layers.FindByFullPath(layer, -1);
				if (layerIndex == -1)
				{
					// .. Infill!
					var newLayer = new Layer { Name = lineage[i] };
					var l2 = lineage.GetRange(0, i - 1);
					var parentLayerFullName = string.Join(Separater, l2);
					var parentIndex = rhinoDoc.Layers.FindByFullPath(parentLayerFullName, -1);
					newLayer.ParentLayerId = rhinoDoc.Layers.FindIndex(parentIndex)?.Id ?? Guid.Empty;
					rhinoDoc.Layers.Add(newLayer);
				}
			}

			TryGetAtExpectedPath(rhinoDoc, layerUpdates, out var expectedLayer);
			return expectedLayer;
		}

		private record struct GetterAndSetter(Func<Layer, string> Get, Action<Layer, string> Set);
	}
}
