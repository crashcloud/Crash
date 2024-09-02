using System.Collections;

using Crash.Changes;
using Crash.Common.Document;
using Crash.Common.Tables;
using Crash.Handlers.Changes;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Geometry.Create;

using Rhino;

namespace Crash.Handlers.Tests.Plugins
{
	[RhinoFixture]
	public sealed class GeometryCreateRemoveActionTests
	{
		private readonly CrashDoc _cdoc;
		private readonly RhinoDoc _rdoc;

		public GeometryCreateRemoveActionTests()
		{
			RhinoDoc.ActiveDoc = _rdoc = RhinoDoc.CreateHeadless(null);
			_cdoc = CrashDocRegistry.CreateAndRegisterDocument(_rdoc);
		}


		public static IEnumerable GeometryCreateArgs
		{
			get
			{
				for (var i = 0; i < 10; i++)
				{
					var geom = NRhino.Random.Geometry.NLineCurve.Any();
					Func<RhinoDoc, CrashObjectEventArgs> func = rdoc =>
																{
																	var id = rdoc.Objects.Add(geom);
																	var rhinoObject = rdoc.Objects.FindId(id);
																	return new CrashObjectEventArgs(null, rhinoObject,
																			 Guid.NewGuid());
																};
					yield return func;
				}
			}
		}

		public static IEnumerable GeometryRemoveArgs
		{
			get
			{
				for (var i = 0; i < 10; i++)
				{
					var geom = NRhino.Random.Geometry.NLineCurve.Any();
					Func<RhinoDoc, CrashObjectEventArgs> func = rdoc =>
																{
																	var id = rdoc.Objects.Add(geom);
																	var rhinoObject = rdoc.Objects.FindId(id);
																	return new CrashObjectEventArgs(null, rhinoObject,
																			 Guid.NewGuid());
																};
					yield return func;
				}
			}
		}

		[TestCaseSource(nameof(GeometryCreateArgs))]
		public void GeometryCreateAction_CanConvert(Func<RhinoDoc, CrashObjectEventArgs> argsFunction)
		{
			var createRecieveArgs = argsFunction(_rdoc);
			var createArgs = new CreateRecieveArgs(ChangeAction.Add, createRecieveArgs, _cdoc);
			var createAction = new GeometryCreateAction();
			Assert.That(createAction.CanConvert(null, createArgs), Is.True);
		}

		[TestCaseSource(nameof(GeometryCreateArgs))]
		public void GeometryCreateAction_TryConvert(Func<RhinoDoc, CrashObjectEventArgs> argsFunction)
		{
			var createRecieveArgs = argsFunction(_rdoc);
			var createArgs = new CreateRecieveArgs(ChangeAction.Add | ChangeAction.Temporary, createRecieveArgs, _cdoc);
			var createAction = new GeometryCreateAction();
			Assert.That(createAction.TryConvert(null, createArgs, out var changes), Is.True);
			Assert.That(changes, Is.Not.Empty);
			foreach (var change in changes)
			{
				Assert.That(change.Action, Is.EqualTo(createArgs.Action));
			}
		}

		[TestCaseSource(nameof(GeometryRemoveArgs))]
		public void GeometryRemoveAction_CanConvert(Func<RhinoDoc, CrashObjectEventArgs> argsFunction)
		{
			var createRecieveArgs = argsFunction(_rdoc);
			var createArgs = new CreateRecieveArgs(ChangeAction.Remove, createRecieveArgs, _cdoc);
			var createAction = new GeometryRemoveAction();
			Assert.That(createAction.CanConvert(null, createArgs), Is.True);
		}

		[TestCaseSource(nameof(GeometryRemoveArgs))]
		public async Task GeometryRemoveAction_TryConvert(Func<RhinoDoc, CrashObjectEventArgs> argsFunction)
		{
			var createRecieveArgs = argsFunction(_rdoc);
			var createArgs = new CreateRecieveArgs(ChangeAction.Remove, createRecieveArgs, _cdoc);
			var createAction = new GeometryRemoveAction();

			var cache = GeometryChange.CreateNew(createRecieveArgs.Geometry, "Test");
			cache.Id = createRecieveArgs.ChangeId;

			Assert.That(_cdoc.Tables.TryGet<TemporaryChangeTable>(out var table), Is.True);
			table.UpdateChange(cache);

			Assert.That(createAction.TryConvert(null, createArgs, out var changes), Is.True);
			Assert.That(changes, Is.Not.Empty);
			foreach (var change in changes)
			{
				Assert.That(change.Action, Is.EqualTo(createArgs.Action));
				Assert.That(change is Change, Is.True);
			}
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_cdoc.Dispose();
		}
	}
}
