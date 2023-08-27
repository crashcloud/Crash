using System.Collections;
using System.Collections.Generic;

using Crash.Changes;
using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Geometry;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Camera.Create;

namespace Crash.Handlers.Tests.Plugins
{

	[RhinoFixture]
	public sealed class CameraCreateActionTests
	{
		private readonly CrashDoc _cdoc;

		[TestCaseSource(nameof(ViewArgs))]
		public void CameraCreateAction_CanConvert(CrashViewArgs viewArgs)
		{
			var cameraArgs = new CreateRecieveArgs(ChangeAction.Add, viewArgs, _cdoc);
			var createAction = new CameraCreateAction();
			Assert.That(createAction.CanConvert(null, cameraArgs), Is.True);
		}

		[TestCaseSource(nameof(ViewArgs))]
		public void CameraCreateAction_TryConvert(CrashViewArgs viewargs)
		{
			var createArgs = new CreateRecieveArgs(ChangeAction.Add, viewargs, _cdoc);
			var createAction = new CameraCreateAction();
			Assert.That(createAction.TryConvert(null, createArgs, out IEnumerable<IChange> changes), Is.True);
			Assert.That(changes, Is.Not.Empty);
			foreach (var change in changes)
			{
				Assert.That(change.Action, Is.EqualTo(ChangeAction.Add));
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
				for (int i = 0; i < 10; i++)
				{
					CPoint location = NRhino.Random.Geometry.NPoint3d.Any().ToCrash();
					CPoint target = NRhino.Random.Geometry.NPoint3d.Any().ToCrash();

					yield return new CrashViewArgs(location, target);
				}
			}
		}

	}

}
