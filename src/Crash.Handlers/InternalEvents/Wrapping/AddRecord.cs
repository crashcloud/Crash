namespace Crash.Handlers.InternalEvents.Wrapping
{
	internal record AddRecord : IUndoRedoCache
	{
		internal readonly CrashObjectEventArgs AddArgs;

		internal AddRecord(CrashObjectEventArgs addArgs)
		{
			AddArgs = addArgs;
		}

		public IUndoRedoCache GetInverse()
		{
			var deleteRecord =
				new DeleteRecord(new CrashObjectEventArgs(AddArgs.Doc, AddArgs.Geometry, AddArgs.RhinoId,
				                                          AddArgs.ChangeId));
			return deleteRecord;
		}
	}
}
