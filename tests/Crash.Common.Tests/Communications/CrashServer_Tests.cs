using System.Diagnostics;

using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Handlers.Server;

namespace Crash.Common.Tests.Communications
{
	public sealed class CrashServer_Tests : IDisposable
	{
		public void Dispose()
		{
			CrashServer.ForceCloselocalServers();
		}

		[SetUp]
		public void SetUp()
		{
			CrashServer.ForceCloselocalServers();
		}

		[TearDown]
		public void TearDown()
		{
			CrashServer.ForceCloselocalServers();
		}

		[Test]
		[NonParallelizable]
		public void GetExePath()
		{
			var server = new CrashServer(new CrashDoc());
			var serverPath = CrashServer.GetServerExecutablePath();

			Assert.That(File.Exists(serverPath), Is.True);
		}

		[Test]
		[NonParallelizable]
		public void RegisterServerProcess()
		{
			var server = new CrashServer(new CrashDoc());
			var exe = CrashServer.GetServerExecutablePath();
			var url = $"{CrashServer.DefaultUrl}:{CrashServer.DefaultPort}";

			var startInfo = CrashServer.GetStartInfo(exe, url);

			Assert.DoesNotThrow(() => server.CreateAndRegisterServerProcess(startInfo));

			Assert.That(server.Process, Is.Not.Null);
			Assert.That(server.IsRunning, Is.True);
		}

		[Test]
		[NonParallelizable]
		public void RegisterServerProcess_InvalidInputs()
		{
			var server = new CrashServer(new CrashDoc());
			Assert.Throws<ArgumentNullException>(() => server.CreateAndRegisterServerProcess(null));
		}

		[Test]
		[NonParallelizable]
		public async Task StartServer()
		{
			var url = $"{CrashServer.DefaultUrl}:{CrashServer.DefaultPort}";
			var server = new CrashServer(new CrashDoc());

			await ServerInstaller.EnsureServerExecutableExists();

			Assert.DoesNotThrow(() => server.Start(url));

			var msgs = server._messages;

			Assert.That(server.Process, Is.Not.Null);
			Assert.That(server.IsRunning, Is.True);
		}

		[Test]
		[NonParallelizable]
		public void VerifyFunctionalServer()
		{
			var server = new CrashServer(new CrashDoc());
			var exe = CrashServer.GetServerExecutablePath();
			var url = $"{CrashServer.DefaultUrl}:{CrashServer.DefaultPort}";

			var startInfo = new ProcessStartInfo
			                {
				                FileName = exe,
				                Arguments = $"--urls \"{url}\"",
				                CreateNoWindow = false,
				                RedirectStandardOutput = true,
				                RedirectStandardError = true,
				                UseShellExecute = false
			                };

			Assert.DoesNotThrow(() => server.CreateAndRegisterServerProcess(startInfo));

			Assert.That(server.Process, Is.Not.Null);
			Assert.That(server.IsRunning, Is.True);
		}

		private ProcessStartInfo GetStartInfo(string url)
		{
			var net60 = Path.GetDirectoryName(typeof(CrashServer_Tests).Assembly.Location);
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
			var newDbName = $"{Guid.NewGuid()}.db";
			var dbPath = Path.Combine(net60, newDbName);

			var startInfo = new ProcessStartInfo
			                {
				                FileName = serverExecutable,
				                Arguments = $"--urls \"{url}\" --path \"{dbPath}\"",
				                CreateNoWindow = true, // !Debugger.IsAttached,
				                RedirectStandardOutput = true,
				                RedirectStandardError = true,
				                UseShellExecute = false
			                };

			return startInfo;
		}
	}
}
