using System.Collections;
using System.Collections.Generic;

using Crash.Changes;
using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Geometry;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Camera.Create;

using Rhino.Display;

namespace Crash.Handlers.Tests.Plugins
{

	[RhinoFixture]
	public sealed class CameraCreateActionTests
	{
		private readonly CrashDoc _cdoc;

		[TestCaseSource(nameof(ViewArgs))]
		public void CameraCreateAction_CanConvert(object sender, ViewEventArgs viewArgs)
		{
			var cameraArgs = new CreateRecieveArgs(ChangeAction.Camera, viewArgs, _cdoc);
			var createAction = new CameraCreateAction();
			Assert.That(createAction.CanConvert(sender, cameraArgs), Is.True);
		}

		[TestCaseSource(nameof(ViewArgs))]

		public void CameraCreateAction_TryConvert(object sender, ViewEventArgs viewargs)
		{
			var createArgs = new CreateRecieveArgs(ChangeAction.Add, viewargs, _cdoc);
			var createAction = new CameraCreateAction();
			Assert.That(createAction.TryConvert(sender, createArgs, out IEnumerable<IChange> changes), Is.True);
			Assert.That(changes, Is.Not.Empty);
			foreach (var change in changes)
			{
				Assert.That(change.Action, Is.EqualTo(ChangeAction.Camera));
				Assert.That(change is CameraChange, Is.True);
			}
		}


		public CameraCreateActionTests()
		{
			_cdoc = new CrashDoc();
		}

		public static IEnumerable ViewArgs
		{
			get
			{
				for (int i = 0; i < 100; i++)
				{
					CPoint location = NRhino.Random.Geometry.NPoint3d.Any().ToCrash();
					CPoint target = NRhino.Random.Geometry.NPoint3d.Any().ToCrash();

					yield return new ValueTuple<object, CrashViewArgs>(new object(), new CrashViewArgs(location, target));
				}
			}
		}

	}

}
