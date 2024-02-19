using System.Globalization;

using Rhino.DocObjects;

namespace Crash.Handlers.Utils
{
	internal static class LayerComparisonUtils
	{
		static LayerComparisonUtils()
		{
			Getters = new Dictionary<string, Getter<Layer>>
			          {
				          { nameof(Layer.Name), new Getter<Layer>(layer => layer.Name) },
				          { nameof(Layer.FullPath), new Getter<Layer>(layer => layer.FullPath) },
				          {
					          nameof(Layer.Color),
					          new Getter<Layer>(layer => DictionaryUtils.SerializeColour(layer.Color))
				          },
				          { nameof(Layer.LinetypeIndex), new Getter<Layer>(layer => layer.LinetypeIndex.ToString()) },
				          {
					          nameof(Layer.PlotColor),
					          new Getter<Layer>(layer => DictionaryUtils.SerializeColour(layer.PlotColor))
				          },
				          {
					          nameof(Layer.PlotWeight),
					          new Getter<Layer>(layer => layer.PlotWeight.ToString(CultureInfo.InvariantCulture))
				          },
				          {
					          nameof(Layer.RenderMaterial),
					          new Getter<Layer>(layer => layer.RenderMaterial?.DisplayName)
				          },
				          // User Specific
				          { nameof(Layer.IsLocked), new Getter<Layer>(layer => layer.IsLocked.ToString()) },
				          { nameof(Layer.IsVisible), new Getter<Layer>(layer => layer.IsVisible.ToString()) }
			          };
		}

		private static Dictionary<string, Getter<Layer>> Getters { get; }

		private static Layer GetDefaultLayer()
		{
			return new Layer();
		}

		internal static Dictionary<string, string> GetDefault(Layer layer, string userName)
		{
			return GetDifferences(GetDefaultLayer(), layer, userName);
		}

		internal static Dictionary<string, string> GetDifferences(Layer oldState, Layer newState, string userName)
		{
			var dict = new Dictionary<string, string>();

			foreach (var getterPair in Getters)
			{
				var getter = getterPair.Value;
				if (!DictionaryUtils.IsDifferent(getter, oldState, newState, out var oldValue,
				                                 out var newValue))
				{
					continue;
				}

				dict.Add(DictionaryUtils.GetOldKey(getterPair.Key, userName), oldValue);
				dict.Add(DictionaryUtils.GetNewKey(getterPair.Key, userName), newValue);
			}

			return dict;
		}

		internal static void InsertLayerDefaults(Layer oldState, Layer newState, Dictionary<string, string> updates,
			string userName)
		{
			var oldFullPathKey = DictionaryUtils.GetOldKey(nameof(Layer.FullPath), userName);
			var newFullPathKey = DictionaryUtils.GetNewKey(nameof(Layer.FullPath), userName);
			if (!updates.ContainsKey(oldFullPathKey))
			{
				updates.Add(oldFullPathKey, oldState.FullPath);
			}

			if (!updates.ContainsKey(newFullPathKey))
			{
				updates.Add(newFullPathKey, newState.FullPath);
			}
		}
	}
}
