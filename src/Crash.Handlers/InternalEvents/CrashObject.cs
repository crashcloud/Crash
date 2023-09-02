﻿using Crash.Utils;

using Rhino.DocObjects;
using Rhino.Geometry;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps a Rhino Object and Change</summary>
	public sealed class CrashObject
	{
		/// <summary>The Change Id</summary>
		public readonly Guid ChangeId;

		/// <summary>The Rhino Geometry</summary>
		public readonly GeometryBase Geometry;

		/// <summary>The Rhino Id</summary>
		public readonly Guid RhinoId;

		/// <summary>Consructor</summary>
		internal CrashObject(RhinoObject rhinoObject)
		{
			RhinoId = rhinoObject.Id;
			Geometry = rhinoObject.Geometry;
			rhinoObject.TryGetChangeId(out ChangeId);
		}

		internal CrashObject(Guid changeId, Guid rhinoId, GeometryBase geometry)
		{
			ChangeId = changeId;
			RhinoId = rhinoId;
			Geometry = geometry;
		}
	}
}
