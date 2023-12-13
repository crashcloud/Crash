using Microsoft.Extensions.Logging;

namespace Crash.Common.App
{
	/// <summary>
	///     A global Application class to handle Application specific logic
	/// </summary>
	public static class CrashApp
	{
		/// <summary>The current Log Level, anything lower won't be logged</summary>
		private static LogLevel Level { get; } = LogLevel.Information;

		/// <summary>Logs a Message</summary>
		public static void Log(string message, LogLevel level = LogLevel.Information)
		{
			LogMessage?.Invoke(null, new CrashLog(message, level, DateTime.UtcNow));

			if (level < Level || string.IsNullOrEmpty(message))
			{
			}

			// Add Log logic here
		}

		public static void LogError(string message)
		{
			Log(message, LogLevel.Error);
		}

		public static void LogInformation(string message)
		{
			Log(message);
		}

		public static void LogCritical(string message)
		{
			Log(message, LogLevel.Critical);
		}

		public static void LogWarning(string message)
		{
			Log(message, LogLevel.Warning);
		}


		/// <summary>Fired every time a Log is called</summary>
		public static event EventHandler<CrashLog> LogMessage;

		/// <summary>A Log structure</summary>
		/// <param name="Message"></param>
		/// <param name="Level"></param>
		/// <param name="Stamp"></param>
		public record struct CrashLog(string Message, LogLevel Level, DateTime Stamp);
	}
}
