using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Crash.Changes;
using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Geometry;
using Crash.Handlers.Plugins.Camera.Recieve;

namespace Crash.Handlers.Tests.Plugins.Camera
{

	[RhinoFixture]
	public sealed class CameraRecieveActionTests
	{

		[TestCaseSource(nameof(CameraChanges))]
		public async Task CameraRecieveAction_CanConvert(Crash.Common.View.Camera camera)
		{
			string username = Path.GetRandomFileName().Replace(".", "");
			IChange change = CameraChange.CreateNew(camera, username);
			Change serverChange = new Change(change);

			CrashDoc crashDoc = new CrashDoc();

			CameraRecieveAction recieveAction = new CameraRecieveAction();

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

		public static IEnumerable CameraChanges
		{
			get
			{
				for (int i = 0; i < 10; i++)
				{
					CPoint location = NRhino.Random.Geometry.NPoint3d.Any().ToCrash();
					CPoint target = NRhino.Random.Geometry.NPoint3d.Any().ToCrash();
					yield return new Crash.Common.View.Camera(location, target);
				}
			}
		}


	}

}
