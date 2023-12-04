namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps Rhino Object change Event Args</summary>
	public sealed class CrashUpdateArgs
	{
		public readonly CrashObject CrashObject;

		public readonly Dictionary<string, string> Updates;

		public CrashUpdateArgs(CrashObject crashObject, Dictionary<string, string> updates)
		{
			CrashObject = crashObject;
			Updates = updates;
		}
	}
}
