using System.Drawing;

using Rhino.DocObjects;

namespace Crash.Handlers.Utils
{
	internal static class RhinoObjectAndAttributesUtils
	{
		static RhinoObjectAndAttributesUtils()
		{
			s_userSpecificKeys = new HashSet<string> { nameof(RhinoObject.IsHidden), nameof(Layer.IsVisible) };
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
							                                      RhinoObjectsUtils
								                                      .ParseEnumOrDefault(value,
									                                      ActiveSpace.ModelSpace))
				            },
				            {
					            nameof(ObjectAttributes.LinetypeSource),
					            new AttribGetterAndSetter(attribs => attribs.LinetypeSource.ToString(),
					                                      (attribs, value) =>
						                                      RhinoObjectsUtils.ParseEnumOrDefault(value,
							                                      ObjectLinetypeSource.LinetypeFromLayer))
				            },
				            {
					            nameof(ObjectAttributes.LinetypeIndex),
					            new AttribGetterAndSetter(attribs => attribs.LinetypeIndex.ToString(),
					                                      (attribs, value) =>
						                                      attribs.LinetypeIndex =
							                                      RhinoObjectsUtils.GetIntOrDefault(value))
				            },
				            {
					            nameof(ObjectAttributes.ColorSource),
					            new AttribGetterAndSetter(attribs => attribs.ColorSource.ToString(),
					                                      (attribs, value) =>
						                                      RhinoObjectsUtils.ParseEnumOrDefault(value,
							                                      ObjectColorSource.ColorFromLayer))
				            },
				            {
					            nameof(ObjectAttributes.ObjectColor),
					            new
						            AttribGetterAndSetter(attribs => RhinoObjectsUtils.SerializeColour(attribs.ObjectColor),
						                                  (attribs, value) =>
							                                  attribs.ObjectColor =
								                                  RhinoObjectsUtils
									                                  .GetColourOrDefault(value, Color.Black)
				            },
				            {
					            nameof(ObjectAttributes.PlotColorSource),
					            new AttribGetterAndSetter(attribs => attribs.PlotColorSource.ToString(),
					                                      (attribs, value) =>
						                                      RhinoObjectsUtils.ParseEnumOrDefault(value,
							                                      ObjectPlotColorSource.PlotColorFromLayer))
				            },
				            {
					            nameof(ObjectAttributes.PlotColor),
					            new
						            AttribGetterAndSetter(attribs => RhinoObjectsUtils.SerializeColour(attribs.PlotColor),
						                                  (attribs, value) =>
							                                  attribs.PlotColor =
								                                  RhinoObjectsUtils
									                                  .GetColourOrDefault(value, Color.Black)
				            },
				            {
					            nameof(ObjectAttributes.PlotWeightSource),
					            new AttribGetterAndSetter(attribs => attribs.PlotWeightSource.ToString(),
					                                      (attribs, value) =>
						                                      RhinoObjectsUtils.ParseEnumOrDefault(value,
							                                      ObjectPlotWeightSource.PlotWeightFromLayer))
				            },
				            {
					            nameof(ObjectAttributes.PlotWeight),
					            new AttribGetterAndSetter(attribs => attribs.PlotWeight.ToString(),
					                                      (attribs, value) =>
						                                      attribs.PlotWeight =
							                                      RhinoObjectsUtils.GetDoubleOrDefault(value))
				            },
				            {
					            nameof(ObjectAttributes.MaterialSource),
					            new AttribGetterAndSetter(attribs => attribs.MaterialSource.ToString(),
					                                      (attribs, value) =>
						                                      RhinoObjectsUtils.ParseEnumOrDefault(value,
							                                      ObjectMaterialSource.MaterialFromLayer))
				            },
				            {
					            nameof(Layer.FullPath),
					            new AttribGetterAndSetter(attribs => attribs.LayerIndex.ToString(),
					                                      (attribs, value) =>
						                                      attribs.LayerIndex =
							                                      RhinoObjectsUtils.GetIntOrDefault(value))
				            }
			            };
			s_objects = new Dictionary<string, ObjectGetterAndSetter>();
		}

		private static Dictionary<string, AttribGetterAndSetter> s_attribs { get; }
		private static Dictionary<string, ObjectGetterAndSetter> s_objects { get; }
		private static HashSet<string> s_userSpecificKeys { get; }

		private record struct AttribGetterAndSetter(Func<ObjectAttributes, string> Get,
			Action<ObjectAttributes, string> Set);

		private record struct ObjectGetterAndSetter(Func<RhinoObject, string> Get, Action<RhinoObject, string> Set);
	}
}
