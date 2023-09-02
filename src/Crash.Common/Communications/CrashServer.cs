using System.Diagnostics;

using Crash.Common.Document;
using Crash.Common.Events;

namespace Crash.Common.Communications
{
	/// <summary>Crash server class to handle the ServerProcess</summary>
	public sealed class CrashServer : IDisposable
	{
		// TODO : Make all of this Rhino Settings
		public const int DefaultPort = 8080;
		public const string DefaultUrl = "http://0.0.0.0";
		private const string ProcessName = "Crash.Server";


		private static readonly string s_extractedServerFilename = $"{ProcessName}.exe";
		public static string BaseDirectory;

		private readonly CrashDoc _crashDoc;

		internal List<string> _messages = new();

		static CrashServer()
		{
			var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
			                                        Environment.SpecialFolderOption.Create);
			BaseDirectory = Path.Combine(appData, "Crash");
		}


		public CrashServer(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
		}

		public Process? Process { get; private set; }

		public bool IsRunning => Process is not null && !Process.HasExited;
		public bool Connected { get; private set; }
		public static string ServerDirectory => Path.Combine(BaseDirectory, "Server");
		public static string ServerFilepath => Path.Combine(ServerDirectory, s_extractedServerFilename);

		~CrashServer()
		{
			Dispose(false);
		}

		public void CloseLocalServer()
		{
			this?.Stop();
			this?.Dispose();
		}

		/// <summary>
		///     Method to start the server
		/// </summary>
		/// <param name="url">The URL for the Server</param>
		/// <returns>True on success, false otherwise</returns>
		public void Start(string url)
		{
			Start(GetStartInfo(GetServerExecutablePath(), url));
		}

		internal void Start(ProcessStartInfo startInfo, int timeout = 3000)
		{
			string errorMessage;
			if (CheckForPreExistingServer())
			{
				errorMessage = "Server Process is already running!";
				throw new Exception(errorMessage);
			}

			try
			{
				CreateAndRegisterServerProcess(startInfo);
			}
			catch (FileNotFoundException)
			{
				errorMessage = "Could not find Server exe";
				throw new FileNotFoundException(errorMessage);
			}

			for (var i = 0; i <= timeout; i += 100)
			{
				if (Connected)
				{
					break;
				}

				Thread.Sleep(100);
			}
		}

		private static bool CheckForPreExistingServer()
		{
			var processes = Process.GetProcessesByName(ProcessName, Environment.MachineName);
			return processes.Length != 0;
		}

		// TODO : Unit Test this
		public static bool ForceCloselocalServers(int timeout = 0)
		{
			var processes = Process.GetProcessesByName(ProcessName, Environment.MachineName);
			if (processes is null || processes.Length == 0)
			{
				return true;
			}

			foreach (var serverProcess in processes)
			{
				serverProcess?.Kill();
			}

			// TODO : should this use a cancellation token?
			var step = 100;
			for (var i = 0; i < timeout; i++)
			{
				Thread.Sleep(step);
				i += step;
			}

			return processes.All(p => null == p || p.HasExited);
		}

		/// <summary>Returns the Server Executable or an exception</summary>
		/// <exception cref="FileNotFoundException"></exception>
		internal static string GetServerExecutablePath()
		{
			if (!File.Exists(ServerFilepath))
			{
				throw new FileNotFoundException("Could not find server executable!");
			}

			return ServerFilepath;
		}

		// https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
		/// <summary>Returns the Start Info for creating a crash.server process instance</summary>
		/// <param name="serverExecutable">The path for the crash.server.exe</param>
		/// <param name="url">the server url</param>
		/// <returns>The standard Crash.Server start info</returns>
		internal static ProcessStartInfo GetStartInfo(string serverExecutable, string url)
		{
			var startInfo = new ProcessStartInfo
			                {
				                FileName = serverExecutable,
				                Arguments = $"--urls \"{url}\"",
#if DEBUG
				                CreateNoWindow = false,
#else
				CreateNoWindow = true,
#endif
				                RedirectStandardOutput = true,
				                RedirectStandardError = true,
				                UseShellExecute = false // Never enable this
			                };

			return startInfo;
		}

		// https://stackoverflow.com/questions/285760/how-to-spawn-a-process-and-capture-its-stdout-in-net
		internal void CreateAndRegisterServerProcess(ProcessStartInfo startInfo)
		{
			Process = new Process
			          {
				          StartInfo = startInfo ?? throw new ArgumentNullException("Process Info is null"),
				          EnableRaisingEvents = true
			          };

			// Register fresh
			Process.Disposed += Process_Exited;
			Process.Exited += Process_Exited;
			Process.OutputDataReceived += Process_OutputDataReceived;
			Process.ErrorDataReceived += Process_ErrorDataReceived;

			if (!Process.Start())
			{
				throw new ApplicationException("Failed to start server!");
			}

			if (startInfo.RedirectStandardOutput)
			{
				Process.BeginOutputReadLine();
			}

			if (startInfo.RedirectStandardError)
			{
				Process.BeginErrorReadLine();
			}
		}

		private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			var data = e.Data;
			if (string.IsNullOrEmpty(data))
			{
				return;
			}

			_messages.Add(data);

			if (data.Contains("failed to bind") ||
			    data.Contains("AddressInUseException") ||
			    data.Contains("SocketException "))
			{
				var portInUseMessage = "Given Port is already in use! Try another!";
				_messages.Add(portInUseMessage);
			}
			else if (data.Contains("hostpolicy"))
			{
				_messages.Add("Crash.Server.exe was referenced incorrectly");
			}

			Process.ErrorDataReceived -= Process_ErrorDataReceived;
			OnFailure?.Invoke(this, null);
			Stop();
		}

		private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			var data = e.Data;
			if (string.IsNullOrEmpty(data))
			{
				return;
			}

			_messages.Add(data);

			Console.WriteLine(data);
			var started = data.ToLower().Contains("now listening on: http");
			if (started)
			{
				Connected = true;
				OnConnected?.Invoke(this, new CrashEventArgs(_crashDoc));
			}
		}

		private void Process_Exited(object sender, EventArgs e)
		{
			// TODO : Capture Exit and either attempt restart or exit application
			_messages.Add("Exited!");
		}


		public event EventHandler<CrashEventArgs>? OnConnected;
		public event EventHandler<CrashEventArgs>? OnFailure;


		#region Stop/End

		/// <summary>
		///     Stop connection
		/// </summary>
		public void Stop()
		{
			try
			{
				if (Process is not null)
				{
					// De-Register first to avoid duplicate calls
					Process.Disposed -= Process_Exited;
					Process.Exited -= Process_Exited;
					Process.OutputDataReceived -= Process_OutputDataReceived;
					Process.ErrorDataReceived -= Process_ErrorDataReceived;
				}

				Process?.Kill();
			}
			catch
			{
			}
			finally
			{
				Process = null;
			}
		}

		/// <summary>
		///     Dispose
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		///     Disposes of the object, stopping the server if it is running
		/// </summary>
		/// <param name="disposing">true if disposing, false if GC'd</param>
		public void Dispose(bool disposing)
		{
			// stop the server!
			Stop();
		}

		#endregion
	}
}
