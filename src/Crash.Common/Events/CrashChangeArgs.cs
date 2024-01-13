using Crash.Common.Document;

namespace Crash.Common.Events
{
	/// <summary>
	///     Captures changes in a set of <see cref="Change" />s
	/// </summary>
	public sealed class CrashChangeArgs : CrashEventArgs
	{
		/// <summary>
		///     The updated <see cref="Change" />s
		/// </summary>
		public readonly IEnumerable<Change> Changes;

		public CrashChangeArgs(CrashDoc crashDoc, IEnumerable<Change> changes)
			: base(crashDoc)
		{
			Changes = changes;
		}
	}
}
