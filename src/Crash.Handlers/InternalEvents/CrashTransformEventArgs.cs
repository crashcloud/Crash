using Crash.Common.Document;
using Crash.Geometry;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps Rhino Transform Event Args</summary>
	public sealed class CrashTransformEventArgs : EventArgs
	{
		/// <summary>The Crash Doc of these Args</summary>
		public readonly CrashDoc Doc;

		/// <summary>The affected Objects</summary>
		public readonly IEnumerable<CrashObject> Objects;

		/// <summary>Will objects be copied?</summary>
		public readonly bool ObjectsWillBeCopied;

		/// <summary>The CTransform of the Event</summary>
		public readonly CTransform Transform;

		/// <summary>Default Constructor</summary>
		internal CrashTransformEventArgs(CrashDoc crashDoc, CTransform transform,
			IEnumerable<CrashObject> objects,
			bool objectsWillBeCopied)
		{
			Doc = crashDoc;
			Transform = transform;
			Objects = objects;
			ObjectsWillBeCopied = objectsWillBeCopied;
		}
	}
}
