using Crash.Changes;
using Crash.Utils;

using Rhino;
using Rhino.Geometry;

namespace Crash.Handlers.Tests.Changes
{
	[RhinoFixture]
	public sealed class ChangeUtils_Tests
	{
		/// <summary>Test that nothing throws</summary>
		[Test]
		public void TryGetChangeId_NullInput()
		{
			Assert.That(ChangeUtils.TryGetChangeId(null, out var id), Is.False);
			Assert.That(id, Is.EqualTo(Guid.Empty));
		}

		/// <summary>Test that nothing throws</summary>
		[Test]
		public void TryGetChangeId_EmptyDictionary()
		{
			var doc = RhinoDoc.CreateHeadless(null);
			var lineCurve = new LineCurve(Point3d.Origin, new Point3d(100, 0, 0));
			var rhinoId = doc.Objects.Add(lineCurve);
			var rhinoObject = doc.Objects.FindId(rhinoId);

			Assert.That(rhinoObject.TryGetChangeId(out var id), Is.False);
			Assert.That(id, Is.EqualTo(Guid.Empty));
		}

		/// <summary>Test that nothing throws</summary>
		[Test]
		public void SyncHost_NullInputs()
		{
			ChangeUtils.SyncHost(null, null, null);

			ChangeUtils.SyncHost(null, new ExampleRhinoChange(), null);


			var doc = RhinoDoc.CreateHeadless(null);
			var lineCurve = new LineCurve(Point3d.Origin, new Point3d(100, 0, 0));
			var rhinoId = doc.Objects.Add(lineCurve);
			var rhinoObject = doc.Objects.FindId(rhinoId);
			rhinoObject.SyncHost(null, null);
		}

		private sealed class ExampleRhinoChange : IChange
		{
			public DateTime Stamp => DateTime.Now;

			public Guid Id => Guid.NewGuid();

			public string Owner => nameof(ExampleRhinoChange);

			public string Payload => "";

			public string Type => nameof(ExampleRhinoChange);

			public ChangeAction Action { get; set; } = ChangeAction.Add;
		}
	}
}
