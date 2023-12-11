namespace Crash.Handlers.InternalEvents.Wrapping
{
	/// <summary>
	///     Stores UndoRedo events
	/// </summary>
	internal interface IUndoRedoCache
	{
		IUndoRedoCache GetInverse();
	}
}
