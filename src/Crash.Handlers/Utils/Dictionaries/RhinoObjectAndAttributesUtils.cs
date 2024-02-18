using System.Drawing;

using Rhino.DocObjects;

namespace Crash.Handlers.Utils
{
	internal static class RhinoObjectAndAttributesUtils
	{
		static RhinoObjectAndAttributesUtils()
		{
			s_attribs = new Dictionary<string, AttribGetterAndSetter>
			            {
				            {
					            nameof(ObjectAttributes.Name), new AttribGetterAndSetter(attribs => attribs.Name,
						            (attribs, value) => attribs.Name = value)
				            },
				            {
					            nameof(ObjectAttributes.Space),
					            new AttribGetterAndSetter(attribs => attribs.Space.ToString(),
					                                      (attribs, value) =>
						                                      attribs.Space =
							                                      DictionaryUtils
								                                      .ParseEnumOrDefault(value,
									                                      ActiveSpace.ModelSpace))
				            },
				            {
					            nameof(ObjectAttributes.LinetypeSource),
					            new AttribGetterAndSetter(attribs => attribs.LinetypeSource.ToString(),
					                                      (attribs, value) =>
						                                      DictionaryUtils.ParseEnumOrDefault(value,
							                                      ObjectLinetypeSource.LinetypeFromLayer))
				            },
				            {
					            nameof(ObjectAttributes.LinetypeIndex),
					            new AttribGetterAndSetter(attribs => attribs.LinetypeIndex.ToString(),
					                                      (attribs, value) =>
						                                      attribs.LinetypeIndex =
							                                      DictionaryUtils.GetIntOrDefault(value))
				            },
				            {
					            nameof(ObjectAttributes.ColorSource),
					            new AttribGetterAndSetter(attribs => attribs.ColorSource.ToString(),
					                                      (attribs, value) =>
						                                      DictionaryUtils.ParseEnumOrDefault(value,
							                                      ObjectColorSource.ColorFromLayer))
				            },
				            {
					            nameof(ObjectAttributes.ObjectColor), new
						            AttribGetterAndSetter(attribs => DictionaryUtils.SerializeColour(attribs.ObjectColor),
						                                  (attribs, value) =>
							                                  attribs.ObjectColor =
								                                  DictionaryUtils
									                                  .GetColourOrDefault(value, Color.Black))
				            },
				            {
					            nameof(ObjectAttributes.PlotColorSource),
					            new AttribGetterAndSetter(attribs => attribs.PlotColorSource.ToString(),
					                                      (attribs, value) =>
						                                      DictionaryUtils.ParseEnumOrDefault(value,
							                                      ObjectPlotColorSource.PlotColorFromLayer))
				            },
				            {
					            nameof(ObjectAttributes.PlotColor), new
						            AttribGetterAndSetter(attribs => DictionaryUtils.SerializeColour(attribs.PlotColor),
						                                  (attribs, value) =>
							                                  attribs.PlotColor =
								                                  DictionaryUtils
									                                  .GetColourOrDefault(value, Color.Black))
				            },
				            {
					            nameof(ObjectAttributes.PlotWeightSource),
					            new AttribGetterAndSetter(attribs => attribs.PlotWeightSource.ToString(),
					                                      (attribs, value) =>
						                                      DictionaryUtils.ParseEnumOrDefault(value,
							                                      ObjectPlotWeightSource.PlotWeightFromLayer))
				            },
				            {
					            nameof(ObjectAttributes.PlotWeight),
					            new AttribGetterAndSetter(attribs => attribs.PlotWeight.ToString(),
					                                      (attribs, value) =>
						                                      attribs.PlotWeight =
							                                      DictionaryUtils.GetDoubleOrDefault(value))
				            },
				            {
					            nameof(ObjectAttributes.MaterialSource),
					            new AttribGetterAndSetter(attribs => attribs.MaterialSource.ToString(),
					                                      (attribs, value) =>
						                                      DictionaryUtils.ParseEnumOrDefault(value,
							                                      ObjectMaterialSource.MaterialFromLayer))
				            },
				            {
					            nameof(Layer.FullPath),
					            new AttribGetterAndSetter(attribs => attribs.LayerIndex.ToString(),
					                                      (attribs, value) =>
						                                      attribs.LayerIndex =
							                                      DictionaryUtils.GetIntOrDefault(value))
				            }
			            };
			s_objects = new Dictionary<string, ObjectGetterAndSetter>();
		}

		private static Dictionary<string, AttribGetterAndSetter> s_attribs { get; }
		private static Dictionary<string, ObjectGetterAndSetter> s_objects { get; }


		internal static Dictionary<string, string> GetAttributeDifferences(ObjectAttributes oldState,
			ObjectAttributes newState, string userName)
		{
			var dict = new Dictionary<string, string>();

			foreach (var getter in s_attribs)
			{
				if (!DictionaryUtils.IsDifferent(getter.Value.Get, oldState, newState, out var oldValue,
				                                 out var newValue))
				{
					continue;
				}

				dict.Add(DictionaryUtils.GetOldKey(getter.Key, userName), oldValue);
				dict.Add(DictionaryUtils.GetNewKey(getter.Key, userName), newValue);
			}

			return dict;
		}

		internal static Dictionary<string, string> GetDefaults(ObjectAttributes attributes, string userName)
		{
			return GetAttributeDifferences(new ObjectAttributes(), attributes, userName);
		}

		internal static void UpdateAttributes(ObjectAttributes attribs, Dictionary<string, string> values,
			string userName)
		{
			foreach (var kvp in values)
			{
				if (!s_attribs.TryGetValue(DictionaryUtils.GetNeutralKey(kvp.Key, userName), out var setter))
				{
					continue;
				}

				setter.Set(attribs, kvp.Value);
			}
		}

		private record struct AttribGetterAndSetter(Func<ObjectAttributes, string> Get,
			Action<ObjectAttributes, string> Set);

		private record struct ObjectGetterAndSetter(Func<ObjectAttributes, string> Get,
			Action<ObjectAttributes, string> Set);
	}
}
