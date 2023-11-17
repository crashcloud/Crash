using Crash.Common.View;
using Crash.Geometry;

namespace Crash.Common.Test.Changes
{
	public sealed class CameraChangeTests
	{
		public static IEnumerable EqualCameras
		{
			get
			{
				var cameraOne = new Camera(CPoint.Origin, new CPoint(10, 20, 30));
				yield return new object[] { cameraOne, cameraOne };

				var cameraTwo = new Camera(new CPoint(-50, 20, 30.216), CPoint.Origin);
				yield return new object[] { cameraTwo, cameraTwo };
			}
		}

		public static IEnumerable NotEqualCameras
		{
			get
			{
				yield return new object[] { Camera.None, new Camera(CPoint.Origin, new CPoint(1, 2, 3)) };

				var cameraOne = new Camera(CPoint.Origin, new CPoint(10, 20, 30));
				var cameraTwo = new Camera(new CPoint(10, 20, 30), CPoint.Origin);
				yield return new object[] { cameraOne, cameraTwo };
			}
		}

		[TestCaseSource(nameof(EqualCameras))]
		[Parallelizable(ParallelScope.All)]
		public void CamerasAreEqual(Camera left, Camera right)
		{
			Assert.That(left, Is.EqualTo(right));
			Assert.That(left == right, Is.True);
			Assert.That(left.Equals(right), Is.True);
			Assert.That(left.Equals((object)right), Is.True);
			Assert.That(left.GetHashCode() == right.GetHashCode());
		}

		[TestCaseSource(nameof(NotEqualCameras))]
		[Parallelizable(ParallelScope.All)]
		public void CamerasAreNotEqual(Camera left, Camera right)
		{
			Assert.That(left, Is.Not.EqualTo(right));
			Assert.That(left != right, Is.True);
			Assert.That(left.Equals(right), Is.False);
			Assert.That(left.Equals((object)right), Is.False);
			Assert.That(left.GetHashCode() != right.GetHashCode());
		}
	}
}
