namespace Crash.Handlers.InternalEvents.Wrapping
{
	/// <summary>
	///     Records a single added item
	/// </summary>
	internal record AddRecord : IUndoRedoCache
	{
		internal readonly CrashObjectEventArgs AddArgs;

		internal AddRecord(CrashObjectEventArgs addArgs)
		{
			AddArgs = addArgs;
		}

		public bool TryGetInverse(out IUndoRedoCache cache)
		{
			cache =
				new DeleteRecord(new CrashObjectEventArgs(AddArgs.Doc, AddArgs.Geometry, AddArgs.RhinoId,
				                                          AddArgs.ChangeId));
			return true;
		}
	}
}
