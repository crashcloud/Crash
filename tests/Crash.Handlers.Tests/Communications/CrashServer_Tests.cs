using Crash.Common.Document;
using Crash.Communications;
using Crash.Handlers.Server;

namespace Crash.Handlers.Tests.Communications
{

	public sealed class CrashServer_Tests : IDisposable
	{

		[Test]
		public async Task StartServer()
		{
			string url = $"{CrashServer.DefaultURL}:{CrashServer.DefaultPort}";
			CrashServer server = new CrashServer(new CrashDoc());

			await ServerInstaller.EnsureServerExecutableExists();

			Assert.DoesNotThrow(() => server.Start(url));

			Assert.That(server.process, Is.Not.Null);
			Assert.That(server.IsRunning, Is.True);
		}

		public void Dispose()
		{
			CrashServer.ForceCloselocalServers();
		}

	}

}
