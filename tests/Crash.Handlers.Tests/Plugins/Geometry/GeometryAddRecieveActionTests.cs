using System.Collections;

using Crash.Changes;
using Crash.Common.Document;
using Crash.Handlers.Changes;
using Crash.Handlers.Plugins.Geometry.Recieve;

using Rhino;

namespace Crash.Handlers.Tests.Plugins.Geometry
{
	[RhinoFixture]
	public sealed class GeometryAddRecieveActionTests
	{
		private readonly CrashDoc _cdoc;
		private readonly RhinoDoc _rdoc;

		public GeometryAddRecieveActionTests()
		{
			RhinoDoc.ActiveDoc = _rdoc = RhinoDoc.CreateHeadless(null);
			_cdoc = CrashDocRegistry.CreateAndRegisterDocument(_rdoc);
		}

		public static IEnumerable AddChanges
		{
			get
			{
				for (var i = 0; i < 10; i++)
				{
					var owner = Path.GetRandomFileName().Replace(".", "");
					var lineCurve = NRhino.Random.Geometry.NLineCurve.Any();
					IChange change = GeometryChange.CreateNew(lineCurve, owner);

					yield return new Change(change);
				}
			}
		}

		[TestCaseSource(nameof(AddChanges))]
		public async Task TestGeometryAddRecieveAction(Change change)
		{
			var addAction = new GeometryAddRecieveAction();
			await addAction.OnRecieveAsync(_cdoc, change);
			while (_cdoc.Queue.Count > 0)
			{
				_cdoc.Queue.RunNextAction();
			}

			// ChangeUtils.TryGetChangeId() ?

			// Assert that RhinoDoc had something added 
			Assert.That(_rdoc.Objects, Is.Not.Empty);
			Assert.That(_cdoc.CacheTable, Is.Empty);
		}
	}
}
