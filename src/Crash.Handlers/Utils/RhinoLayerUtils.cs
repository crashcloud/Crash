using System.Drawing;

using Rhino;
using Rhino.DocObjects;

namespace Crash.Handlers
{
	internal static class RhinoLayerUtils
	{
#if NETFRAMEWORK
		private static readonly string Separater = Layer.PathSeparator;
#else
		private static readonly string Separater = ModelComponent.NamePathSeparator;
#endif
		private static Layer GetDefaultLayer()
		{
			return new Layer();
		}

		private static void EmptySetValue(Layer layer, object value) { }

		private static int GetIntOrDefault(string value)
		{
			if (int.TryParse(value, out var result))
			{
				return result;
			}

			return -1;
		}

		private static Color GetColourOrDefault(string value, Color defaultValue)
		{
			if (!int.TryParse(value, out var result))
			{
				return defaultValue;
			}

			return Color.FromArgb(result);
		}

		private static string SerializeColour(Color colour)
		{
			return colour.ToArgb().ToString();
		}

		private static bool GetBoolOrDefault(string value, bool defaultValue = true)
		{
			if (bool.TryParse(value, out var result))
			{
				return result;
			}

			return defaultValue;
		}

		private static string GetStringOrDefault(string value, string defaultValue)
		{
			if (string.IsNullOrEmpty(value))
			{
				return defaultValue ?? string.Empty;
			}

			return value;
		}

		static RhinoLayerUtils()
		{
			s_gettersAndSetters = new Dictionary<string, GetterAndSetter>
			                      {
				                      {
					                      nameof(Layer.Name), new GetterAndSetter(layer => layer.Name,
						                      (layer, value) =>
						                      {
							                      layer.Name =
								                      GetStringOrDefault(value,
								                                         layer.FullPath
								                                              ?.Split(new[] { Separater },
									                                              StringSplitOptions
										                                              .RemoveEmptyEntries)
								                                              ?.Last());
						                      })
				                      },
				                      { nameof(Layer.FullPath), new(layer => layer.FullPath, EmptySetValue) },
				                      {
					                      nameof(Layer.Color),
					                      new GetterAndSetter(layer => SerializeColour(layer.Color),
					                                          (layer, value) =>
						                                          layer.Color = GetColourOrDefault(value, Color.Black))
				                      },
				                      {
					                      nameof(Layer.LinetypeIndex),
					                      new GetterAndSetter(layer => layer.LinetypeIndex.ToString(),
					                                          (layer, value) =>
						                                          layer.LinetypeIndex = GetIntOrDefault(value))
				                      },
				                      {
					                      nameof(Layer.IsLocked),
					                      new GetterAndSetter(layer => layer.IsLocked.ToString(),
					                                          (layer, value) =>
						                                          layer.IsLocked = GetBoolOrDefault(value))
				                      },
				                      {
					                      nameof(Layer.IsVisible),
					                      new GetterAndSetter(layer => layer.IsVisible.ToString(),
					                                          (layer, value) =>
						                                          layer.IsVisible = GetBoolOrDefault(value))
				                      }
			                      };
		}

		private static Dictionary<string, GetterAndSetter> s_gettersAndSetters { get; }

		internal static Dictionary<string, string> GetLayerDefaults(Layer layer)
		{
			return GetLayerDifference(GetDefaultLayer(), layer);
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

				dict.Add(GetOldKey(getter.Key), oldValue);
				dict.Add(GetNewKey(getter.Key), newValue);
			}

			var oldFullPathKey = GetOldKey(nameof(Layer.FullPath));
			var newFullPathKey = GetNewKey(nameof(Layer.FullPath));
			if (!dict.ContainsKey(oldFullPathKey))
			{
				dict.Add(oldFullPathKey, oldState.FullPath);
			}

			if (!dict.ContainsKey(newFullPathKey))
			{
				dict.Add(newFullPathKey, newState.FullPath);
			}

			return dict;
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
			return key.Replace("Old", "").Replace("New", "").Replace("_", "");
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

			Layer previousLayer = null;
			for (var i = 0; i <= lineage.Count; i++)
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
					var newLayer = new Layer();
					if (i == lineage.Count)
					{
						newLayer = originalLayer;
						layerIndex = newLayer.Index;
					}

					var parentLayerFullName = previousLayer.FullPath;
					var parentIndex = rhinoDoc.Layers.FindByFullPath(parentLayerFullName, -1);
					newLayer.ParentLayerId = rhinoDoc.Layers.FindIndex(parentIndex)?.Id ?? Guid.Empty;

					if (!newLayer.HasIndex)
					{
						layerIndex = rhinoDoc.Layers.Add(newLayer);
					}
				}

				previousLayer = rhinoDoc.Layers.FindIndex(layerIndex);
			}

			TryGetAtExpectedPath(rhinoDoc, layerUpdates, out var expectedLayer);
			return expectedLayer;
		}

		private record struct GetterAndSetter(Func<Layer, string> Get, Action<Layer, string> Set);
	}
}
