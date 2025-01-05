using System.Collections;

using Crash.Changes;
using Crash.Common.Document;
using Crash.Geometry;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Geometry.Create;
using Crash.Handlers.Tests.Plugins.Geometry;

namespace Crash.Handlers.Tests.Plugins
{
	[RhinoTestFixture]
	public sealed class GeometryTransformtActionTests
	{
		private readonly CrashDoc _cdoc;

		public GeometryTransformtActionTests()
		{
			_cdoc = new CrashDoc();
		}

		public static IEnumerable TransformArgs
		{
			get
			{
				foreach (var crashObject in SharedUtils.SelectObjects)
				{
					var doubles = new double[16];
					for (var i = 0; i < 16; i++)
					{
						var value = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
						doubles[i] = value;
					}

					var transform = new CTransform(doubles);
					var objects = new List<CrashObject> { crashObject };
					yield return new CrashTransformEventArgs(null, transform, objects, false);
				}
			}
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_cdoc.Dispose();
		}

		[TestCaseSource(nameof(TransformArgs))]
		public void GeometrySelectAction_CanConvert(CrashTransformEventArgs transformArgs)
		{
			var createArgs = new CreateRecieveArgs(ChangeAction.Transform, transformArgs, _cdoc);
			var createAction = new GeometryTransformAction();
			Assert.That(createAction.CanConvert(null, createArgs), Is.True);
		}

		[TestCaseSource(nameof(TransformArgs))]
		public void GeometryTransformAction_TryConvert(CrashTransformEventArgs transformArgs)
		{
			var createArgs = new CreateRecieveArgs(ChangeAction.Transform, transformArgs, _cdoc);
			var createAction = new GeometryTransformAction();
			Assert.That(createAction.TryConvert(null, createArgs, out var changes), Is.True);
			Assert.That(changes, Is.Not.Empty);
			foreach (var change in changes)
			{
				Assert.That(change.Action, Is.EqualTo(createArgs.Action));
			}
		}
	}
}
