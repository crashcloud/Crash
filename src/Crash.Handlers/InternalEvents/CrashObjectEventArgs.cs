using Rhino.DocObjects;
using Rhino.Geometry;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps RhinoObjectEventArgs</summary>
	public sealed class CrashObjectEventArgs : EventArgs
	{
		/// <summary>Change Id</summary>
		public readonly Guid ChangeId;

		/// <summary>The Event Geometry</summary>
		public readonly GeometryBase Geometry;

		/// <summary>The Event Rhino Id</summary>
		public readonly Guid RhinoId;

		/// <summary>UnDelete flag</summary>
		public readonly bool UnDelete;

		/// <summary>Constructor mainly for tests</summary>
		public CrashObjectEventArgs(GeometryBase geometry, Guid rhinoId, Guid changeId = default, bool unDelete = false)
		{
			ChangeId = changeId;
			RhinoId = rhinoId;
			Geometry = geometry;
			UnDelete = unDelete;
		}

		/// <summary>Default Constructor</summary>
		public CrashObjectEventArgs(RhinoObject rhinoObject, Guid changeId = default, bool unDelete = false)
			: this(rhinoObject.Geometry, rhinoObject.Id, changeId, unDelete)
		{
		}
	}
}
