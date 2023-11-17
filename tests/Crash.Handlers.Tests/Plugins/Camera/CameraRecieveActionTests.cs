using System.Collections;

using Crash.Changes;
using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Handlers.Plugins.Camera.Recieve;

namespace Crash.Handlers.Tests.Plugins.Camera
{
	[RhinoFixture]
	public sealed class CameraRecieveActionTests
	{
		public static IEnumerable CameraChanges
		{
			get
			{
				for (var i = 0; i < 10; i++)
				{
					var location = NRhino.Random.Geometry.NPoint3d.Any().ToCrash();
					var target = NRhino.Random.Geometry.NPoint3d.Any().ToCrash();
					yield return new Common.View.Camera(location, target);
				}
			}
		}

		[TestCaseSource(nameof(CameraChanges))]
		public async Task CameraRecieveAction_CanConvert(Common.View.Camera camera)
		{
			var username = Path.GetRandomFileName().Replace(".", "");
			IChange change = CameraChange.CreateNew(camera, username);
			var serverChange = new Change(change);

			var crashDoc = new CrashDoc();

			var recieveAction = new CameraRecieveAction();

			Assert.That(crashDoc.Cameras, Is.Empty);
			await recieveAction.OnRecieveAsync(crashDoc, serverChange);
			while (crashDoc.Queue.Count > 0)
			{
				crashDoc.Queue.RunNextAction();
			}

			Assert.That(crashDoc.Cameras, Is.Not.Empty);

			Assert.That(crashDoc.Cameras.TryGetCamera(new User(username), out var cameras), Is.True);
			Assert.That(cameras, Has.Count.EqualTo(1));
			Assert.That(cameras.FirstOrDefault(), Is.EqualTo(camera));
		}
	}
}
