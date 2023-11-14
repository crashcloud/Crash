namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps the RhinoSelection and Deselection Events</summary>
	public sealed class CrashSelectionEventArgs : EventArgs
	{
		/// <summary>Related Event Objets</summary>
		public readonly IEnumerable<CrashObject> CrashObjects;

		/// <summary>Was this a Selection Event or Deselection Event</summary>
		public readonly bool Selected;

		/// <summary>Singular Selection/Deselection Event Constructor</summary>
		private CrashSelectionEventArgs(bool selected,
			IEnumerable<CrashObject> crashObjects)
		{
			CrashObjects = crashObjects;
			Selected = selected;
		}

		/// <summary>Creates a new DeSelection Event for One item</summary>
		public static CrashSelectionEventArgs CreateSelectionEvent(IEnumerable<CrashObject> crashObjects)
		{
			return new CrashSelectionEventArgs(true, crashObjects);
		}

		/// <summary>Creates a new Selection Event for One item</summary>
		public static CrashSelectionEventArgs CreateDeSelectionEvent(IEnumerable<CrashObject> crashObjects)
		{
			return new CrashSelectionEventArgs(false, crashObjects);
		}
	}
}
