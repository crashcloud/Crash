namespace Crash.Handlers.InternalEvents.Wrapping
{
	/// <summary>
	///     Records an Object Update Record
	/// </summary>
	internal record UpdateRecord : IUndoRedoCache
	{
		internal readonly CrashUpdateArgs UpdateArgs;

		internal UpdateRecord(CrashUpdateArgs args)
		{
			UpdateArgs = args;
		}

		public bool TryGetInverse(out IUndoRedoCache cache)
		{
			cache = null;
			return false;
		}
	}
}
