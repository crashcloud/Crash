using Crash.Common.Document;

namespace Crash.Common.Tests.Document
{
	public sealed class DocumentTests
	{
		public static IEnumerable EqualCrashDocs
		{
			get
			{
				var docOne = new CrashDoc();
				yield return new[] { docOne, docOne };

				var docTwo = new CrashDoc();
				yield return new[] { docTwo, docTwo };
			}
		}

		public static IEnumerable NotEqualCrashDocs
		{
			get
			{
				yield return new CrashDoc[] { new(), new() };
				yield return new CrashDoc[] { new(), new() };
			}
		}

		[TestCaseSource(nameof(EqualCrashDocs))]
		[Parallelizable(ParallelScope.All)]
		public void CrashDocsAreEqual(CrashDoc left, CrashDoc right)
		{
			Assert.That(left, Is.EqualTo(right));
			Assert.That(left == right, Is.True);
			Assert.That(left.Equals(right), Is.True);
			Assert.That(left.Equals((object)right), Is.True);
			Assert.That(left.GetHashCode() == right.GetHashCode());
		}

		[TestCaseSource(nameof(NotEqualCrashDocs))]
		[Parallelizable(ParallelScope.All)]
		public void CrashDocsAreNotEqual(CrashDoc left, CrashDoc right)
		{
			Assert.That(left, Is.Not.EqualTo(right));
			Assert.That(left != right, Is.True);
			Assert.That(left.Equals(right), Is.False);
			Assert.That(left.Equals((object)right), Is.False);
			Assert.That(left.GetHashCode() != right.GetHashCode());
		}
	}
}
