using Rhino;
using Rhino.DocObjects;

namespace Crash.Handlers
{
	internal static class RhinoDocUtils
	{
		internal static RhinoDoc GetRhinoDocFromObjects(IEnumerable<RhinoObject> rhinoObjects)
		{
			var rhinoDoc = rhinoObjects
			               ?.FirstOrDefault(o => o.Document is not null)
			               ?.Document;

			return rhinoDoc;
		}
	}
}
