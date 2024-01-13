using Crash.Common.Document;

namespace Crash.Common.Events
{
	/// <summary>
	///     Captures the initialization of <see cref="Change" />s
	/// </summary>
	public sealed class CrashInitArgs : CrashEventArgs
	{
		/// <summary>
		///     The updated <see cref="Change" />s
		/// </summary>
		public readonly IEnumerable<Change> Changes;

		public CrashInitArgs(CrashDoc crashDoc, IEnumerable<Change> changes)
			: base(crashDoc)
		{
			Changes = changes;
		}
	}
}
