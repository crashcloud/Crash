using Crash.Common.Document;

namespace Crash.Common.Events
{
	/// <summary>The Crash Event Args</summary>
	public class CrashEventArgs : EventArgs
	{
		/// <summary>The Crash Doc of these Args</summary>
		public readonly CrashDoc CrashDoc;

		private Dictionary<string, object> _args { get; } = new Dictionary<string, object>();

		/// <summary>Default Constructor</summary>
		public CrashEventArgs(CrashDoc crashDoc)
		{
			CrashDoc = crashDoc;
		}

		public CrashEventArgs(CrashDoc crashDoc, Dictionary<string, object> args)
		{
			CrashDoc = crashDoc;
			_args = args ?? new Dictionary<string, object>();
		}

		/// <summary>Get the Value of a Key</summary>
		public T TryGet<T>(string name)
		{
			try
			{
				if (_args.TryGetValue(name, out var value))
				{
					return (T)value;
				}
			}
			catch
			{
				// ignored
			}

			return default;
		}
	}
}
