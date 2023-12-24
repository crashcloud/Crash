using Rhino;
using Rhino.DocObjects;

namespace Crash.Handlers
{
	public static class RhinoDocUtils
	{
		public static RhinoDoc GetRhinoDocFromObjects(IEnumerable<RhinoObject> rhinoObjects)
		{
			var rhinoDoc = rhinoObjects
			               ?.FirstOrDefault(o => o.Document is not null)
			               ?.Document;

			return rhinoDoc;
		}
	}
}
