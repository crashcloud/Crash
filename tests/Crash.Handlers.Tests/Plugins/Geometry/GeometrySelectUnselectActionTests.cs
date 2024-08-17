using System.Collections;

using Crash.Changes;
using Crash.Common.Document;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Geometry.Create;
using Crash.Handlers.Tests.Plugins.Geometry;

namespace Crash.Handlers.Tests.Plugins
{
	[RhinoFixture]
	public sealed class GeometrySelectUnselectActionTests
	{
		private readonly CrashDoc _cdoc;


		public GeometrySelectUnselectActionTests()
		{
			_cdoc = new CrashDoc();
		}

		public static IEnumerable SelectArgs
		{
			get
			{
				foreach (var crashObjeect in SharedUtils.SelectObjects)
				{
					yield return
						CrashSelectionEventArgs.CreateSelectionEvent(null, new List<CrashObject> { crashObjeect });
				}
			}
		}

		public static IEnumerable UnSelectArgs
		{
			get
			{
				foreach (var crashObjeect in SharedUtils.SelectObjects)
				{
					yield return
						CrashSelectionEventArgs.CreateDeSelectionEvent(null, new List<CrashObject> { crashObjeect });
				}
			}
		}

		[TestCaseSource(nameof(SelectArgs))]
		public void GeometrySelectAction_CanConvert(CrashSelectionEventArgs selectEventArgs)
		{
			var selectArgs = new CreateRecieveArgs(ChangeAction.Locked, selectEventArgs, _cdoc);
			var createAction = new GeometryLockAction();
			Assert.That(createAction.CanConvert(null, selectArgs), Is.True);
		}

		[TestCaseSource(nameof(SelectArgs))]
		public void GeometrySelectAction_TryConvert(CrashSelectionEventArgs selectEventArgs)
		{
			var selectArgs = new CreateRecieveArgs(ChangeAction.Locked, selectEventArgs, _cdoc);
			var createAction = new GeometryLockAction();
			Assert.That(createAction.TryConvert(null, selectArgs, out var changes), Is.True);
			Assert.That(changes, Is.Not.Empty);
			foreach (var change in changes)
			{
				Assert.That(change.Action, Is.EqualTo(selectArgs.Action));
			}
		}

		[TestCaseSource(nameof(UnSelectArgs))]
		public void GeometryUnSelectAction_CanConvert(CrashSelectionEventArgs selectEventArgs)
		{
			var selectArgs = new CreateRecieveArgs(ChangeAction.Unlocked, selectEventArgs, _cdoc);
			var createAction = new GeometryUnlockAction();
			Assert.That(createAction.CanConvert(null, selectArgs), Is.True);
		}

		[TestCaseSource(nameof(UnSelectArgs))]
		public void GeometryUnSelectAction_TryConvert(CrashSelectionEventArgs selectEventArgs)
		{
			var selectArgs = new CreateRecieveArgs(ChangeAction.Unlocked, selectEventArgs, _cdoc);
			var createAction = new GeometryUnlockAction();
			Assert.That(createAction.TryConvert(null, selectArgs, out var changes), Is.True);
			Assert.That(changes, Is.Not.Empty);
			foreach (var change in changes)
			{
				Assert.That(change.Action, Is.EqualTo(selectArgs.Action));
			}
		}


		[OneTimeTearDown]
		public void TearDown()
		{
			_cdoc.Dispose();
		}
	}
}
