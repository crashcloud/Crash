using Crash.Common.Document;
using Crash.Common.Tables;

using Rhino.DocObjects;
using Rhino.Geometry;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps RhinoObjectEventArgs</summary>
	public sealed class CrashObjectEventArgs : EventArgs
	{
		/// <summary>Change Id</summary>
		public readonly Guid ChangeId;

		/// <summary>The Crash Doc this Object comes from</summary>
		public readonly CrashDoc Doc;

		/// <summary>The Event Geometry</summary>
		public readonly GeometryBase Geometry;

		/// <summary>The Event Rhino Id</summary>
		public readonly Guid RhinoId;

		/// <summary>UnDelete flag</summary>
		public readonly bool UnDelete;

		/// <summary>Constructor mainly for tests</summary>
		public CrashObjectEventArgs(CrashDoc crashDoc, GeometryBase geometry, Guid rhinoId, Guid changeId = default,
			bool unDelete = false)
		{
			Doc = crashDoc;
			RhinoId = rhinoId;
			Geometry = geometry;
			if (changeId == default)
			{
				geometry.UserDictionary.TryGetGuid(RealisedChangeTable.ChangeIdKey, out ChangeId);
			}
			else
			{
				ChangeId = changeId;
			}

			UnDelete = unDelete;
		}

		/// <summary>Default Constructor</summary>
		public CrashObjectEventArgs(CrashDoc crashDoc, RhinoObject rhinoObject, Guid changeId = default,
			bool unDelete = false)
			: this(crashDoc, rhinoObject.Geometry, rhinoObject.Id, changeId, unDelete)
		{
		}
	}
}
