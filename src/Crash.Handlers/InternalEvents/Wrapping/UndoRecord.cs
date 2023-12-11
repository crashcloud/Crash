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

		public IUndoRedoCache GetInverse()
		{
			throw new NotImplementedException("Not enough data here to implement a reset call");
		}
	}
}
