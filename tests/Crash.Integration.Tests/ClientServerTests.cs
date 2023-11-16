using System.Diagnostics;
using System.Net;

using Crash.Common.Communications;
using Crash.Common.Document;

namespace Crash.Integration.Tests
{
	public sealed class ClientServerTests
	{
		private const int timeout = 3000;
		private const int pollTime = 100;

		private CrashDoc _crashDoc;
		private string clientUrl => $"{CrashClient.DefaultURL}:{CrashServer.DefaultPort}";
		private string clientEndpoint => $"{clientUrl}/Crash";
		private string serverUrl => $"{CrashServer.DefaultUrl}:{CrashServer.DefaultPort}";
		private Uri clientUri => new(clientEndpoint);

		private static User user => new("Marcio");

		[SetUp]
		public void Setup()
		{
			if (_crashDoc?.LocalServer is object)
			{
				_crashDoc.LocalServer.CloseLocalServer();
				Thread.Sleep(1000);
			}

			Assert.That(CrashServer.ForceCloselocalServers(1000),
			            "Not all server instances are closed",
			            Is.True);
		}

		[TearDown]
		public void TearDown()
		{
			if (_crashDoc?.LocalServer is object)
			{
				_crashDoc.LocalServer.CloseLocalServer();
				Thread.Sleep(1000);
			}

			Assert.That(CrashServer.ForceCloselocalServers(1000),
			            "Not all server instances are closed",
			            Is.True);
		}

		[Test]
		public async Task ServerProcess()
		{
			var onConnected = false;
			var onFailure = false;

			_crashDoc = new CrashDoc();
			_crashDoc.Users.CurrentUser = user;
			_crashDoc.LocalServer = new CrashServer(_crashDoc);

			_crashDoc.LocalServer.OnConnected += (sender, args) => onConnected = true;
			_crashDoc.LocalServer.OnFailure += (sender, args) => onFailure = true;

			var crashServer = StartServer(_crashDoc);

			Assert.That(crashServer.Connected, Is.True);

			Assert.That(onConnected, Is.True, "Server Connection failed");
			Assert.That(onFailure, Is.False, "Server failed!");
			onConnected = false;
			onFailure = false;

			await EnsureSiteIsUp();

			crashServer.Stop();

			Assert.That(crashServer.Process, Is.Null, "Process is not null");
			Assert.That(crashServer.IsRunning, Is.False, "Server process is still running");
			Assert.That(onConnected, Is.False, "Server Connection failed");
			Assert.That(onFailure, Is.False, "Server failed!");
		}

		[Test]
		public async Task ServerAndClient()
		{
			_crashDoc = new CrashDoc();
			_crashDoc.Users.CurrentUser = user;

			var crashServer = StartServer(_crashDoc);

			await EnsureSiteIsUp();

			var crashClient = await StartClientAsync(_crashDoc);
		}

		private async Task EnsureSiteIsUp()
		{
			// Perform a URL ping
			var httpClient = new HttpClient();
			var response = await httpClient.GetAsync(clientUrl);
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		}

		private CrashServer StartServer(CrashDoc crashDoc)
		{
			crashDoc.Users.CurrentUser = user;

			var crashServer = crashDoc.LocalServer ?? new CrashServer(crashDoc);

			var processInfo = GetStartInfo(serverUrl);
			Assert.DoesNotThrow(() => crashServer.Start(processInfo));

			var messages = crashServer._messages;
			Assert.That(crashServer.IsRunning, string.Join("\r\n", messages), Is.True);
			Assert.That(crashServer.Process, Is.Not.Null);

			return crashServer;
		}

		private async Task<ICrashClient> StartClientAsync(CrashDoc crashDoc)
		{
			var initRan = false;
			Action<IEnumerable<Change>> func = changes => initRan = true;

			crashDoc.LocalClient.RegisterConnection(user.Name, clientUri);

			// TODO : FIX INIT CHECK
			await crashDoc.LocalClient.StartLocalClientAsync();

			// Wait(() => crashClient.IsConnected);
			Assert.That(crashDoc.LocalClient.IsConnected, Is.True, "Client is not connected");

			Wait(() => initRan);
			Assert.That(initRan, Is.True, "Init did not run!");

			return crashDoc.LocalClient;
		}

		private ProcessStartInfo GetStartInfo(string url)
		{
			var net60 = Path.GetDirectoryName(typeof(ClientServerTests).Assembly.Location);
			var debug = Path.GetDirectoryName(net60);
			var bin = Path.GetDirectoryName(debug);
			var project = Path.GetDirectoryName(bin);
			var tests = Path.GetDirectoryName(project);
			var source = Path.GetDirectoryName(tests);

			var crashServerPath = Path.Combine(source, "src", "Crash.Server", "bin", "debug");

			// C:\Users\csykes\Documents\cloned_gits\Crash\src\Crash.Server\bin\Debug
			// C:\Users\csykes\Documents\cloned_gits\Crash\tests\Crash.Integration.Tests\bin\Debug\net6.0

			var exes = Directory.GetFiles(crashServerPath, "Crash.Server.exe");
			var serverExecutable = exes.FirstOrDefault();
			var serverExePath = Path.GetDirectoryName(serverExecutable);
			var newDbName = "database.db";
			var dbPath = Path.Combine(net60, newDbName);

			var startInfo = new ProcessStartInfo
			                {
				                FileName = serverExecutable,
				                Arguments = $"--urls \"{url}\" --path \"{dbPath}\" --reset true",
				                CreateNoWindow = true, // !Debugger.IsAttached,
				                RedirectStandardOutput = true,
				                RedirectStandardError = true,
				                UseShellExecute = false
			                };

			return startInfo;
		}

		private void Wait(Func<bool> condition)
		{
			for (var i = 0; i < timeout; i += pollTime)
			{
				Thread.Sleep(pollTime);
				if (condition.Invoke())
				{
					break;
				}
			}
		}
	}
}
