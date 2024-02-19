using System.Drawing;
using System.Globalization;

using Rhino.DocObjects;

using ROA = Rhino.DocObjects.ObjectAttributes;

namespace Crash.Handlers.Utils
{
	internal static class ObjectAttributeComparisonUtils
	{
		static ObjectAttributeComparisonUtils()
		{
			s_getters = new Dictionary<string, Getter<ROA>>();
			s_setters = new Dictionary<string, Setter<ROA>>();

			s_setters.Add(nameof(ROA.Name), new Setter<ROA>((a, v) => a.Name = v));
			s_setters.Add(nameof(ROA.Space),
			              new Setter<ROA>((a, v) =>
				                              a.Space = DictionaryUtils.ParseEnumOrDefault(v, ActiveSpace.ModelSpace)));
			s_setters.Add(nameof(ROA.LinetypeSource),
			              new Setter<ROA>((a, v) =>
				                              DictionaryUtils.ParseEnumOrDefault(v,
					                              ObjectLinetypeSource.LinetypeFromLayer)));
			s_setters.Add(nameof(ROA.LinetypeIndex),
			              new Setter<ROA>((a, v) =>
				                              a.LinetypeIndex = DictionaryUtils.GetIntOrDefault(v)));
			s_setters.Add(nameof(ROA.ColorSource),
			              new Setter<ROA>((a, v) =>
				                              DictionaryUtils.ParseEnumOrDefault(v,
					                              ObjectColorSource.ColorFromLayer)));
			s_setters.Add(nameof(ROA.ObjectColor),
			              new Setter<ROA>((a, v) =>
				                              a.ObjectColor =
					                              DictionaryUtils.GetColourOrDefault(v, Color.Black)));
			s_setters.Add(nameof(ROA.PlotColorSource),
			              new Setter<ROA>((a, v) =>
				                              DictionaryUtils.ParseEnumOrDefault(v,
					                              ObjectPlotColorSource.PlotColorFromLayer)));
			s_setters.Add(nameof(ROA.PlotColor),
			              new Setter<ROA>((a, v) =>
				                              a.PlotColor =
					                              DictionaryUtils.GetColourOrDefault(v, Color.Black)));
			s_setters.Add(nameof(ROA.PlotWeightSource),
			              new Setter<ROA>((a, v) =>
				                              DictionaryUtils.ParseEnumOrDefault(v,
					                              ObjectPlotWeightSource.PlotWeightFromLayer)));
			s_setters.Add(nameof(ROA.PlotWeight),
			              new Setter<ROA>((a, v) =>
				                              a.PlotWeight = DictionaryUtils.GetDoubleOrDefault(v)));
			s_setters.Add(nameof(ROA.MaterialSource),
			              new Setter<ROA>((a, v) =>
				                              DictionaryUtils.ParseEnumOrDefault(v,
					                              ObjectMaterialSource.MaterialFromLayer)));
			s_setters.Add(nameof(Layer.FullPath),
			              new Setter<ROA>((a, v) =>
				                              a.LayerIndex = DictionaryUtils.GetIntOrDefault(v)));


			s_getters.Add(nameof(ROA.Name), new Getter<ROA>(a => a.Name));
			s_getters.Add(nameof(ROA.Space), new Getter<ROA>(a => a.Space.ToString()));
			s_getters.Add(nameof(ROA.LinetypeSource), new Getter<ROA>(a => a.LinetypeSource.ToString()));
			s_getters.Add(nameof(ROA.LinetypeIndex), new Getter<ROA>(a => a.LinetypeIndex.ToString()));
			s_getters.Add(nameof(ROA.ColorSource), new Getter<ROA>(a => a.ColorSource.ToString()));
			s_getters.Add(nameof(ROA.ObjectColor),
			              new Getter<ROA>(a => DictionaryUtils.SerializeColour(a.ObjectColor)));
			s_getters.Add(nameof(ROA.PlotColorSource), new Getter<ROA>(a => a.PlotColorSource.ToString()));
			s_getters.Add(nameof(ROA.PlotColor),
			              new Getter<ROA>(a => DictionaryUtils.SerializeColour(a.PlotColor)));
			s_getters.Add(nameof(ROA.PlotWeightSource),
			              new Getter<ROA>(a => a.PlotWeightSource.ToString()));
			s_getters.Add(nameof(ROA.PlotWeight),
			              new Getter<ROA>(a => a.PlotWeight.ToString(CultureInfo.InvariantCulture)));
			s_getters.Add(nameof(ROA.MaterialSource), new Getter<ROA>(a => a.MaterialSource.ToString()));
			s_getters.Add(nameof(Layer.FullPath), new Getter<ROA>(a => a.LayerIndex.ToString()));
		}

		private static Dictionary<string, Getter<ROA>> s_getters { get; }
		private static Dictionary<string, Setter<ROA>> s_setters { get; }


		internal static Dictionary<string, string> GetAttributeDifferences(ROA oldState,
			ROA newState, string userName)
		{
			var dict = new Dictionary<string, string>();

			foreach (var getterPair in s_getters)
			{
				if (!DictionaryUtils.IsDifferent(getterPair.Value, oldState, newState, out var oldValue,
				                                 out var newValue))
				{
					continue;
				}

				dict.Add(DictionaryUtils.GetOldKey(getterPair.Key, userName), oldValue);
				dict.Add(DictionaryUtils.GetNewKey(getterPair.Key, userName), newValue);
			}

			return dict;
		}

		internal static Dictionary<string, string> GetDefaults(ROA attributes, string userName)
		{
			return GetAttributeDifferences(new ROA(), attributes, userName);
		}

		internal static void UpdateAttributes(ROA a, Dictionary<string, string> vs,
			string userName)
		{
			foreach (var kvp in vs)
			{
				if (!s_setters.TryGetValue(DictionaryUtils.GetNeutralKey(kvp.Key, userName), out var setter))
				{
					continue;
				}

				setter.Set(a, kvp.Value);
			}
		}
	}
}
