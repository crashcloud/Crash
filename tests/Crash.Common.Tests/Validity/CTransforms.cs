using Crash.Geometry;

namespace Crash.Common.Tests.Validity
{
	[TestFixture]
	public sealed class CTransformValidity
	{
		[TestCase(1)]
		[TestCase(10)]
		[TestCase(100)]
		public void IsValidExplicit(int count)
		{
			for (var i = 0; i < count; i++)
			{
				var m00 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m01 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m02 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m03 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);

				var m10 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m11 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m12 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m13 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);

				var m20 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m21 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m22 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m23 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);

				var m30 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m31 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m32 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);
				var m33 = TestContext.CurrentContext.Random.NextDouble(short.MinValue, short.MaxValue);

				var transform = new CTransform(m00, m01, m02, m03,
				                               m10, m11, m12, m13,
				                               m20, m21, m22, m23,
				                               m30, m31, m32, m33);


				Assert.That(transform.IsValid(), Is.True);
			}
		}

		[TestCaseSource(typeof(InvalidTransformValues), nameof(InvalidValues.TestCases))]
		public bool IsNotValid(double[] mValues)
		{
			var transform = new CTransform(mValues);
			return transform.IsValid();
		}
	}

	public sealed class InvalidTransformValues
	{
		public static IEnumerable TestCases
		{
			get
			{
				yield return new TestCaseData(new[] { double.MinValue }).Returns(false);
				yield return new TestCaseData(new[] { double.MaxValue }).Returns(false);
				yield return new TestCaseData(new[] { double.NaN }).Returns(false);
				yield return new TestCaseData(new[] { double.NaN, double.MaxValue }).Returns(false);
			}
		}
	}
}
