using System.Collections;
using System.Collections.Generic;

using Crash.Changes;
using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Geometry;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Geometry.Create;
using Crash.Handlers.Tests.Plugins.Geometry;

namespace Crash.Handlers.Tests.Plugins
{

	[RhinoFixture]
	public sealed class GeometryTransformtActionTests
	{
		private readonly CrashDoc _cdoc;

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
			Assert.That(createAction.TryConvert(null, createArgs, out IEnumerable<IChange> changes), Is.True);
			Assert.That(changes, Is.Not.Empty);
			foreach (var change in changes)
			{
				Assert.That(change.Action, Is.EqualTo(createArgs.Action));
				Assert.That(change is TransformChange, Is.True);
			}
		}

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
					double[] doubles = new double[16];
					for (int i = 0; i < 16; i++)
					{
						double value = TestContext.CurrentContext.Random.NextDouble(Int16.MinValue, Int16.MaxValue);
						doubles[i] = value;
					}

					CTransform transform = new CTransform(doubles);
					var objects = new List<CrashObject> { crashObject };
					yield return new CrashTransformEventArgs(transform, objects, false);
				}
			}
		}

	}

}
