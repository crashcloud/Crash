using System.Drawing;

using Rhino.DocObjects;

namespace Crash.Handlers
{
	public static class RhinoLayerUtils
	{
		private static readonly Dictionary<string, Func<Layer, string>> s_getters;
		private static readonly Dictionary<string, Action<Layer, string>> s_setters;
		private static readonly Layer s_defaultLayer;

		static RhinoLayerUtils()
		{
			s_defaultLayer = new Layer();
			s_getters = new Dictionary<string, Func<Layer, string>>
			            {
				            { nameof(Layer.Name), layer => layer.Name },
				            { nameof(Layer.FullPath), layer => layer.FullPath },
				            { nameof(Layer.Color), layer => layer.Color.ToArgb().ToString() },
				            { nameof(Layer.IsLocked), layer => layer.IsLocked.ToString() },
				            { nameof(Layer.IsVisible), layer => layer.IsVisible.ToString() }
			            };

			s_setters = new Dictionary<string, Action<Layer, string>>
			            {
				            { nameof(Layer.Name), (layer, value) => layer.Name = value },
				            // { nameof(Layer.FullPath), (layer, value ) => layer.FullPath = value } // TODO : FIX
				            { nameof(Layer.Color), (layer, value) => layer.Color = Color.FromArgb(int.Parse(value)) },
				            { nameof(Layer.IsLocked), (layer, value) => layer.IsLocked = bool.Parse(value) },
				            { nameof(Layer.IsVisible), (layer, value) => layer.IsVisible = bool.Parse(value) }
			            };
		}

		public static Dictionary<string, string> GetLayerDefaults(Layer layer)
		{
			return GetLayerDifference(s_defaultLayer, layer);
		}

		public static Dictionary<string, string> GetLayerDifference(Layer oldState, Layer newState)
		{
			var dict = new Dictionary<string, string>();

			foreach (var getter in s_getters)
			{
				if (IsDifferent(getter.Value, oldState, newState, out var newValue))
				{
					dict.Add(getter.Key, newValue);
				}
			}

			return dict;
		}

		public static void UpdateLayer(Layer layer, Dictionary<string, string> values)
		{
			foreach (var kvp in values)
			{
				if (!s_setters.TryGetValue(kvp.Key, out var setter))
				{
					continue;
				}

				setter(layer, kvp.Value);
			}
		}

		private static bool IsDifferent(Func<Layer, string> getter, Layer oldState, Layer newState, out string value)
		{
			var oldValue = getter(oldState);
			var newValue = getter(newState);
			value = newValue;

			if (!string.IsNullOrEmpty(oldValue))
			{
				return !oldValue.Equals(newValue);
			}

			return !string.IsNullOrEmpty(newValue);
		}
	}
}
