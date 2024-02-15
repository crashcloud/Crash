using Crash.Common.Document;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps Rhino Object change Event Args</summary>
	public sealed class CrashLayerArgs : EventArgs
	{
		public readonly ChangeAction Action;

		/// <summary>The modified Crash Object</summary>
		public readonly CrashObject CrashLayer;

		/// <summary>The Crash Doc of these Args</summary>
		public readonly CrashDoc Doc;

		/// <summary>Object Updates</summary>
		public readonly Dictionary<string, string> Updates;

		/// <summary>Default constructor</summary>
		/// <param name="layer">The modified Crash Object</param>
		/// <param name="updates">The given updates</param>
		public CrashLayerArgs(CrashDoc crashDoc, CrashObject layer, ChangeAction action,
			Dictionary<string, string> updates)
		{
			Doc = crashDoc;
			Action = action;
			CrashLayer = layer;
			Updates = updates;
		}
	}
}
