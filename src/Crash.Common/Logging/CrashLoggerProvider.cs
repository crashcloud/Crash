using Microsoft.Extensions.Logging;

namespace Crash.Common.Logging
{
	internal sealed class CrashLoggerProvider : ILoggerProvider
	{
		public ILogger CreateLogger(string categoryName)
		{
			return CrashLogger.Logger;
		}

		public void Dispose()
		{
			CrashLogger.Logger.Dispose();
		}
	}
}
