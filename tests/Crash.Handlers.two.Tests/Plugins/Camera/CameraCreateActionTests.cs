using System.Collections;
using System.Collections.Generic;

using Crash.Changes;
using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Geometry;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Camera.Create;

using Rhino;
using Rhino.Display;
using Rhino.Geometry;

namespace Crash.Handlers.Tests.Plugins
{

	public sealed class CameraCreateActionTests
	{
		private readonly RhinoDoc _doc;
		private readonly CrashDoc _cdoc;

		[TestCaseSource(nameof(ViewArgs))]
		public void GeometryCreateAction_CanConvert(object sender, ViewEventArgs createRecieveArgs)
		{
			var cameraArgs = new CreateRecieveArgs(ChangeAction.Camera, createRecieveArgs, _cdoc);
			var createAction = new CameraCreateAction();
			Assert.That(createAction.CanConvert(sender, cameraArgs), Is.True);
		}

		[TestCaseSource(nameof(ViewArgs))]
#
		public void GeometryCreateAction_TryConvert(object sender, ViewEventArgs createRecieveArgs)
		{
			var createArgs = new CreateRecieveArgs(ChangeAction.Add, createRecieveArgs, _cdoc);
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
			//  Use the existing open docs
			_doc = RhinoDoc.CreateHeadless(null);
			_cdoc = CrashDocRegistry.CreateAndRegisterDocument(_doc);
		}

		public static IEnumerable ViewArgs
		{
			get
			{
				for (int i = 0; i < 100; i++)
				{
					CPoint location = RandomPoint().ToCrash();
					CPoint target = RandomPoint().ToCrash();

					yield return new ValueTuple<object, CrashViewArgs>(new object(), new CrashViewArgs(location, target));
				}
			}
		}

		private static Point3d RandomPoint()
		{
			double x = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
			double y = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
			double z = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);

			return new Point3d(x, y, z);
		}

	}

}
