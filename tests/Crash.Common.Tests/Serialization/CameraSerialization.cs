using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Common.View;
using Crash.Geometry;

namespace Crash.Changes.Tests.Serialization
{
	[TestFixture]
	public sealed class CameraSerialization
	{
		private const double MIN = -123456789.123456789;
		private const double MAX = MIN * -1;

		internal static readonly JsonSerializerOptions TestOptions;

		static CameraSerialization()
		{
			TestOptions = new JsonSerializerOptions
			              {
				              IgnoreReadOnlyFields = true,
				              IgnoreReadOnlyProperties = true,
				              IncludeFields = true,
				              NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
				              ReadCommentHandling = JsonCommentHandling.Skip,
				              WriteIndented = true // TODO : Should this be avoided? Does it add extra memory?
			              };
		}

		[TestCase(1)]
		[TestCase(10)]
		[TestCase(100)]
		[Parallelizable]
		public void TestCameraSerializationRandom(int count)
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

				TestCameraSerializtion(camera);
			}
		}

		private void TestCameraSerializtion(Camera Camera)
		{
			var json = JsonSerializer.Serialize(Camera, TestOptions);
			var CameraOut = JsonSerializer.Deserialize<Camera>(json, TestOptions);
			Assert.That(Camera, Is.EqualTo(CameraOut));
			Assert.That(Camera.Stamp, Is.EqualTo(CameraOut.Stamp));
		}
	}
}
