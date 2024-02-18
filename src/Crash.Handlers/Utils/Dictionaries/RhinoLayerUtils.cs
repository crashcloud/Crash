using System.Drawing;

using Rhino;
using Rhino.DocObjects;

namespace Crash.Handlers.Utils
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

		static RhinoLayerUtils()
		{
			s_gettersAndSetters = new Dictionary<string, GetterAndSetter>
			                      {
				                      {
					                      nameof(Layer.Name), new GetterAndSetter(layer => layer.Name,
						                      (layer, value) =>
						                      {
							                      layer.Name =
								                      DictionaryUtils.GetStringOrDefault(value,
									                      layer.FullPath
									                           ?.Split(new[] { Separater },
									                                   StringSplitOptions
										                                   .RemoveEmptyEntries)
									                           ?.Last());
						                      })
				                      },
				                      {
					                      nameof(Layer.FullPath),
					                      new(layer => layer.FullPath, DictionaryUtils.EmptySetValue)
				                      },
				                      {
					                      nameof(Layer.Color),
					                      new GetterAndSetter(layer => DictionaryUtils.SerializeColour(layer.Color),
					                                          (layer, value) =>
						                                          layer.Color =
							                                          DictionaryUtils
								                                          .GetColourOrDefault(value, Color.Black))
				                      },
				                      {
					                      nameof(Layer.LinetypeIndex),
					                      new GetterAndSetter(layer => layer.LinetypeIndex.ToString(),
					                                          (layer, value) =>
						                                          layer.LinetypeIndex =
							                                          DictionaryUtils.GetIntOrDefault(value))
				                      },
				                      {
					                      nameof(Layer.PlotColor), new
						                      GetterAndSetter(layer => DictionaryUtils.SerializeColour(layer.PlotColor),
						                                      (layer, value) =>
							                                      layer.Color =
								                                      DictionaryUtils
									                                      .GetColourOrDefault(value, Color.Black)
						                                     )
				                      },
				                      {
					                      nameof(Layer.PlotWeight),
					                      new GetterAndSetter(layer => layer.PlotWeight.ToString(),
					                                          (layer, value) =>
						                                          layer.PlotWeight =
							                                          DictionaryUtils.GetDoubleOrDefault(value))
				                      },
				                      /*
				                      {
					                      nameof(Layer.RenderMaterial),
					                      new GetterAndSetter(layer => layer.RenderMaterial?.DisplayName,
					                                          DictionaryUtils.EmptySetValue)
				                      },
										*/

				                      // User Specific
				                      {
					                      nameof(Layer.IsLocked),
					                      new GetterAndSetter(layer => layer.IsLocked.ToString(),
					                                          (layer, value) =>
						                                          layer.IsLocked =
							                                          DictionaryUtils.GetBoolOrDefault(value))
				                      },
				                      {
					                      nameof(Layer.IsVisible),
					                      new GetterAndSetter(layer => layer.IsVisible.ToString(),
					                                          (layer, value) =>
						                                          layer.IsVisible =
							                                          DictionaryUtils.GetBoolOrDefault(value))
				                      }
			                      };
		}

		private static Dictionary<string, GetterAndSetter> s_gettersAndSetters { get; }

		internal static Dictionary<string, string> GetLayerDefaults(Layer layer, string userName)
		{
			return GetLayerDifference(GetDefaultLayer(), layer, userName);
		}

		internal static Dictionary<string, string> GetLayerDifference(Layer oldState, Layer newState, string userName)
		{
			var dict = new Dictionary<string, string>();

			foreach (var getter in s_gettersAndSetters)
			{
				if (!DictionaryUtils.IsDifferent(getter.Value.Get, oldState, newState, out var oldValue,
				                                 out var newValue))
				{
					continue;
				}

				dict.Add(DictionaryUtils.GetOldKey(getter.Key, userName), oldValue);
				dict.Add(DictionaryUtils.GetNewKey(getter.Key, userName), newValue);
			}

			var oldFullPathKey = DictionaryUtils.GetOldKey(nameof(Layer.FullPath), userName);
			var newFullPathKey = DictionaryUtils.GetNewKey(nameof(Layer.FullPath), userName);
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

		internal static bool TryGetAtExpectedPath(RhinoDoc rhinoDoc, Dictionary<string, string> layerUpdates,
			string userName, out Layer layer)
		{
			return TryGetLayer(DictionaryUtils.GetNewKey(nameof(Layer.FullPath), userName), rhinoDoc, layerUpdates,
			                   out layer);
		}

		internal static bool TryGetAtOldPath(RhinoDoc rhinoDoc, Dictionary<string, string> layerUpdates,
			string userName, out Layer layer)
		{
			return TryGetLayer(DictionaryUtils.GetOldKey(nameof(Layer.FullPath), userName), rhinoDoc, layerUpdates,
			                   out layer);
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

		internal static void UpdateLayer(Layer layer, Dictionary<string, string> values, string userName)
		{
			foreach (var kvp in values)
			{
				if (!s_gettersAndSetters.TryGetValue(DictionaryUtils.GetNeutralKey(kvp.Key, userName), out var setter))
				{
					continue;
				}

				setter.Set(layer, kvp.Value);
			}
		}

		public static Layer MoveLayerToExpectedPath(RhinoDoc rhinoDoc, Dictionary<string, string> layerUpdates,
			string userName)
		{
			TryGetAtOldPath(rhinoDoc, layerUpdates, userName, out var originalLayer);
			var expectedPath = layerUpdates[DictionaryUtils.GetNewKey(nameof(Layer.FullPath), userName)];

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

			TryGetAtExpectedPath(rhinoDoc, layerUpdates, userName, out var expectedLayer);
			return expectedLayer;
		}

		private record struct GetterAndSetter(Func<Layer, string> Get, Action<Layer, string> Set);
	}
}
