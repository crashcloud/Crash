namespace Crash.Handlers.InternalEvents.Wrapping
{
	/// <summary>
	///     Records the delete of a single item
	/// </summary>
	internal record DeleteRecord : IUndoRedoCache
	{
		internal readonly CrashObjectEventArgs DeleteArgs;

		internal DeleteRecord(CrashObjectEventArgs args)
		{
			DeleteArgs = args;
		}

		public bool TryGetInverse(out IUndoRedoCache cache)
		{
			cache =
				new AddRecord(new CrashObjectEventArgs(DeleteArgs.Doc, DeleteArgs.Geometry, DeleteArgs.RhinoId,
				                                       DeleteArgs.ChangeId, true));
			return true;
		}
	}
}
