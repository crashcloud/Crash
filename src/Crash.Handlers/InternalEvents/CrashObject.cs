using Crash.Common.Document;

using Rhino.DocObjects;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps an <see cref="IChange" /> and <see cref="RhinoObject" /> Id </summary>
	public readonly struct CrashObject
	{
		/// <summary>The Change Id</summary>
		public readonly Guid ChangeId;

		/// <summary>The Rhino Id</summary>
		public readonly Guid RhinoId;

		/// <summary>
		///     Creates a new CrashObject
		/// </summary>
		internal CrashObject(CrashDoc crashDoc, RhinoObject rhinoObject)
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
