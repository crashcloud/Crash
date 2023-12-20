using Crash.Common.Document;

using Rhino.DocObjects;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps a Rhino Object and Change</summary>
	public readonly struct CrashObject
	{
		/// <summary>The Change Id</summary>
		public readonly Guid ChangeId;

		/// <summary>The Rhino Id</summary>
		public readonly Guid RhinoId;

		public CrashObject(CrashDoc crashDoc, RhinoObject rhinoObject)
		{
			RhinoId = rhinoObject.Id;
			crashDoc.RealisedChangeTable.TryGetChangeId(rhinoObject.Id, out ChangeId);
		}

		internal CrashObject(Guid changeId, Guid rhinoId)
		{
			ChangeId = changeId;
			RhinoId = rhinoId;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ChangeId, RhinoId);
		}
	}
}
