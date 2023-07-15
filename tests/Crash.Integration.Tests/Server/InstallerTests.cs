using System.Threading.Tasks;

using Crash.Handlers.Server;

namespace Crash.Handlers.Tests.Server
{

	public sealed class InstallerTests
	{
		[SetUp]
		public void Init()
		{
			ServerInstaller.RemoveOldServer();
			Assert.That(ServerInstaller.ServerExecutableExists, Is.False);
		}

		// TODO : Make this an Integration test. It's slow!
		// TODO : Does this test need running EVERY Run?
		// TODO : Can this test be mocked? It should take maximum 1 second.
		[Test]
		public async Task DownloadTests()
		{
			Assert.That(await ServerInstaller.EnsureServerExecutableExists(), Is.True);
			Assert.That(ServerInstaller.ServerExecutableExists, Is.True);
		}

		[TearDown]
		public void Dispose()
		{
			ServerInstaller.RemoveOldServer();
			Assert.That(ServerInstaller.ServerExecutableExists, Is.False);
		}

	}

}
