using Crash.Common.Document;

using Rhino.DocObjects;

namespace Crash.Handlers.InternalEvents.Wrapping
{
	/// <summary>
	///     Records an Event that converts one or more change(s) into one or more change(s)
	///     A good example is Explode or BooleanUnion
	/// </summary>
	internal record ModifyGeometryRecord : IUndoRedoCache
	{
		internal readonly CrashObjectEventArgs[] AddArgs;
		internal readonly CrashObjectEventArgs[] RemoveArgs;

		internal ModifyGeometryRecord(CrashDoc doc,
			IEnumerable<RhinoObject> addedObjects,
			IEnumerable<RhinoObject> removedObjects)
		{
			AddArgs = addedObjects.Select(ao => new CrashObjectEventArgs(doc, ao)).ToArray();
			RemoveArgs = removedObjects.Select(ro => new CrashObjectEventArgs(doc, ro)).ToArray();
		}

		private ModifyGeometryRecord(CrashObjectEventArgs[] addedArgs,
			CrashObjectEventArgs[] removedArgs)
		{
			AddArgs = addedArgs;
			RemoveArgs = removedArgs;
		}

		public IUndoRedoCache TryGetInverse()
		{
			return new ModifyGeometryRecord(RemoveArgs, AddArgs);
		}
	}
}
