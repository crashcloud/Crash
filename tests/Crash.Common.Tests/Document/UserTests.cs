using Crash.Common.Document;

namespace Crash.Common.Tests.Document
{
	public sealed class UserTests
	{
		public static IEnumerable EqualUsers
		{
			get
			{
				yield return new User[] { new("Jack"), new("Jack") };
				yield return new User[] { new("James"), new("James") };
			}
		}

		public static IEnumerable NotEqualUsers
		{
			get
			{
				yield return new User[] { new("James"), new("Jack") };
				yield return new User[] { new("Jack"), new("James") };
			}
		}

		[TestCaseSource(nameof(EqualUsers))]
		[Parallelizable(ParallelScope.All)]
		public void UsersAreEqual(User left, User right)
		{
			Assert.That(left, Is.EqualTo(right));
			Assert.That(left == right, Is.True);
			Assert.That(left.Equals(right), Is.True);
			Assert.That(left.Equals((object)right), Is.True);
			Assert.That(left.GetHashCode() == right.GetHashCode());
		}

		[TestCaseSource(nameof(NotEqualUsers))]
		[Parallelizable(ParallelScope.All)]
		public void UsersAreNotEqual(User left, User right)
		{
			Assert.That(left, Is.Not.EqualTo(right));
			Assert.That(left != right, Is.True);
			Assert.That(left.Equals(right), Is.False);
			Assert.That(left.Equals((object)right), Is.False);
			Assert.That(left.GetHashCode() != right.GetHashCode());
		}

		[Theory]
		[TestCase("James")]
		[TestCase("Jack")]
		[TestCase("A")]
		[Parallelizable(ParallelScope.All)]
		public void ValidUsers(string? name)
		{
			var user = new User(name);
			Assert.That(user.IsValid(), Is.EqualTo(true));
			Assert.That(user.Color, Is.Not.EqualTo(User.DefaultColour));
		}

		[Theory]
		[TestCase(null)]
		[TestCase("")]
		[Parallelizable(ParallelScope.All)]
		public void InValidUsers(string? name)
		{
			var user = new User(name);
			Assert.That(user.IsValid(), Is.EqualTo(false));
			Assert.That(user.Color, Is.EqualTo(User.DefaultColour));
		}
	}
}
