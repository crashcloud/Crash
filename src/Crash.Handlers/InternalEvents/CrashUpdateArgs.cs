using Crash.Common.Document;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps Rhino Object change Event Args</summary>
	public sealed class CrashUpdateArgs : EventArgs
	{
		/// <summary>The modified Crash Object</summary>
		public readonly CrashObject CrashObject;

		/// <summary>The Crash Doc of these Args</summary>
		public readonly CrashDoc Doc;

		/// <summary>Object Updates</summary>
		public readonly Dictionary<string, string> Updates;

		/// <summary>Default constructor</summary>
		/// <param name="crashObject">The modified Crash Object</param>
		/// <param name="updates">The given updates</param>
		public CrashUpdateArgs(CrashDoc crashDoc, CrashObject crashObject, Dictionary<string, string> updates)
		{
			Doc = crashDoc;
			CrashObject = crashObject;
			Updates = updates;
		}
	}
}
