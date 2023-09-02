using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace Crash.Common.Logging
{
	/// <summary>Enables logging for Crash</summary>
	public sealed class CrashLogger : ILogger, IDisposable
	{
		private readonly LogLevel _currentLevel;

		static CrashLogger()
		{
			Logger = new CrashLogger();
		}

		internal CrashLogger()
		{
			_currentLevel = Debugger.IsAttached ? LogLevel.Trace : LogLevel.Information;
		}

		public static CrashLogger Logger { get; private set; }

		public void Dispose()
		{
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return this;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel >= _currentLevel;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			var _eventId = eventId.Name;
			var formattedMessage = formatter.Invoke(state, exception);
			var message = $"{logLevel} : {formattedMessage} : {_eventId}";

			LogFiles.writeLogMessage(message);
		}

		internal sealed class LogFiles
		{
			private static readonly string _logDirectory;
			private static readonly string _logFileName;
			private static readonly string _logFilePath;

			private static CrashLogger _logger;

			static LogFiles()
			{
				var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				_logDirectory = Path.Combine(appData, "Crash", "Logs");
				_logFileName = $"{DateTime.UtcNow:yyyy_MM_dd_HH_mm_ss}.log";
				_logFilePath = Path.Combine(_logDirectory, _logFileName);

				createLogFile();
			}

			internal LogFiles(CrashLogger logger)
			{
				_logger = logger;
			}

			private static void createLogFile()
			{
				if (!Directory.Exists(_logDirectory))
				{
					Directory.CreateDirectory(_logDirectory);
				}

				if (!File.Exists(_logFilePath))
				{
					File.Create(_logFilePath);
				}
			}

			internal static void writeLogMessage(string message)
			{
				try
				{
					File.AppendAllLines(_logFilePath, new[] { message });
				}
				catch (Exception)
				{
				}
			}
		}
	}
}
