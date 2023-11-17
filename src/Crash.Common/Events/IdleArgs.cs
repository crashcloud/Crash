using Crash.Common.Document;

namespace Crash.Common.Events
{
	/// <summary>Arguments for an Idle Action</summary>
	public class IdleArgs : EventArgs
	{
		/// <summary>The affected Change</summary>
		public readonly IChange Change;

		/// <summary>The CrashDoc this event was called in</summary>
		public readonly CrashDoc Doc;

		/// <summary>Constructs Args for an Idle Event</summary>
		public IdleArgs(CrashDoc crashDoc, IChange change)
		{
			Doc = crashDoc;
			Change = change;
		}
	}
}
