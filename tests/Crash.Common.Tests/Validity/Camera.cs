using Crash.Common.View;
using Crash.Geometry;

namespace Crash.Common.Tests.Validity
{
	public sealed class CameraValidity
	{
		private const double MIN = -123456789.123456789;
		private const double MAX = MIN * -1;

		[TestCase(1)]
		[TestCase(10)]
		[TestCase(100)]
		public void IsValid(int count)
		{
			for (var i = 0; i < count; i++)
			{
				var xLocation = TestContext.CurrentContext.Random.NextDouble(MIN, MAX);
				var yLocation = TestContext.CurrentContext.Random.NextDouble(MIN, MAX);
				var zLocation = TestContext.CurrentContext.Random.NextDouble(MIN, MAX);
				var location = new CPoint(xLocation, yLocation, zLocation);

				var xTarget = TestContext.CurrentContext.Random.NextDouble(MIN, MAX);
				var yTarget = TestContext.CurrentContext.Random.NextDouble(MIN, MAX);
				var zTarget = TestContext.CurrentContext.Random.NextDouble(MIN, MAX);
				var target = new CPoint(xTarget, yTarget, zTarget);

				var ticks = TestContext.CurrentContext.Random.NextLong(DateTime.MinValue.Ticks,
				                                                       DateTime.MaxValue.Ticks);
				var time = new DateTime(ticks);

				var camera = new Camera(location, target) { Stamp = time };

				Assert.That(camera.IsValid(), Is.True);
			}
		}

		[TestCaseSource(typeof(InvalidValues), nameof(InvalidValues.TestCases))]
		public bool IsNotValid(CPoint point, CPoint target, DateTime time)
		{
			var camera = new Camera(point, target) { Stamp = time };

			return camera.IsValid();
		}
	}

	public sealed class InvalidValues
	{
		public static IEnumerable TestCases
		{
			get
			{
				yield return new TestCaseData(new CPoint(0), new CPoint(0), DateTime.Now).Returns(false);
				yield return new TestCaseData(CPoint.None, CPoint.None, DateTime.MaxValue).Returns(false);
				yield return new TestCaseData(CPoint.None, CPoint.None, DateTime.MinValue).Returns(false);
				yield return new TestCaseData(CPoint.Origin, new CPoint(1, 2, 3), DateTime.MinValue).Returns(false);
			}
		}
	}
}
