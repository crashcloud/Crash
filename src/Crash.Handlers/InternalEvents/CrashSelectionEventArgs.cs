using Crash.Common.Document;

namespace Crash.Handlers.InternalEvents
{
	/// <summary>Wraps the RhinoSelection and Deselection Events</summary>
	public sealed class CrashSelectionEventArgs : EventArgs
	{
		/// <summary>Related Event Objets</summary>
		public readonly IEnumerable<CrashObject> CrashObjects;

		/// <summary>The Crash Doc of these Args</summary>
		public readonly CrashDoc Doc;

		/// <summary>Was this a Selection Event or Deselection Event</summary>
		public readonly bool Selected;

		/// <summary>Singular Selection/Deselection Event Constructor</summary>
		private CrashSelectionEventArgs(CrashDoc crashDoc, bool selected,
			IEnumerable<CrashObject> crashObjects)
		{
			Doc = crashDoc;
			CrashObjects = crashObjects;
			Selected = selected;
		}

		/// <summary>Creates a new DeSelection Event for One item</summary>
		public static CrashSelectionEventArgs CreateSelectionEvent(CrashDoc crashDoc,
			IEnumerable<CrashObject> crashObjects)
		{
			return new CrashSelectionEventArgs(crashDoc, true, crashObjects);
		}

		/// <summary>Creates a new Selection Event for One item</summary>
		public static CrashSelectionEventArgs CreateDeSelectionEvent(CrashDoc crashDoc,
			IEnumerable<CrashObject> crashObjects)
		{
			return new CrashSelectionEventArgs(crashDoc, false, crashObjects);
		}
	}
}
