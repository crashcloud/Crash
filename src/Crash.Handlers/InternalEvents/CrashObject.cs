using Crash.Utils;

using Rhino.DocObjects;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps a Rhino Object and Change</summary>
	public struct CrashObject
	{
		/// <summary>The Change Id</summary>
		public readonly Guid ChangeId;

		/// <summary>The Rhino Id</summary>
		public readonly Guid RhinoId;

		public CrashObject(RhinoObject rhinoObject)
		{
			RhinoId = rhinoObject.Id;
			rhinoObject.TryGetChangeId(out ChangeId);
		}

		internal CrashObject(Guid changeId, Guid rhinoId)
		{
			ChangeId = changeId;
			RhinoId = rhinoId;
		}
	}
}
