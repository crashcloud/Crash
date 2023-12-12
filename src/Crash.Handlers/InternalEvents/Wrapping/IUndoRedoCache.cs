namespace Crash.Handlers.InternalEvents.Wrapping
{
	/// <summary>
	///     Stores UndoRedo events
	/// </summary>
	internal interface IUndoRedoCache
	{
		/// <summary>
		///     Calculates the inverse of the current Cache
		/// </summary>
		IUndoRedoCache GetInverse();
	}
}
