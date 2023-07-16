using Crash.Common.Document;
using Crash.Common.Tables;

namespace Crash.Common.Tests.Tables
{

	public class UserTableTests
	{
		[Test]
		[Parallelizable(ParallelScope.Self)]
		public void Add_User_Successfully()
		{
			// Arrange
			var crashDoc = new CrashDoc();
			var userTable = new UserTable(crashDoc);
			var user = new User("user1");

			// Act
			bool result = userTable.Add(user);

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		[Parallelizable(ParallelScope.Self)]
		public void Add_Duplicate_User_Failure()
		{
			// Arrange
			var crashDoc = new CrashDoc();
			var userTable = new UserTable(crashDoc);
			var user1 = new User("user1");
			var user2 = new User("user1");

			// Act
			bool result1 = userTable.Add(user1);
			bool result2 = userTable.Add(user2);

			// Assert
			Assert.IsTrue(result1);
			Assert.IsFalse(result2);
		}

		[Test]
		[Parallelizable(ParallelScope.Self)]
		public void Add_Current_User_Failure()
		{
			// Arrange
			var crashDoc = new CrashDoc();
			var userTable = new UserTable(crashDoc);
			var user = new User("user1");
			userTable.CurrentUser = user;

			// Act
			bool result = userTable.Add(user);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		[Parallelizable(ParallelScope.Self)]
		public void Add_User_Invokes_OnUserAdded()
		{
			// Arrange
			var crashDoc = new CrashDoc();
			var userTable = new UserTable(crashDoc);
			var user = new User("user1");
			bool eventRaised = false;
			UserTable.OnUserAdded += (sender, args) => eventRaised = true;

			// Act
			bool result = userTable.Add(user);

			// Assert
			Assert.IsTrue(eventRaised);
		}

		[Test]
		[Parallelizable(ParallelScope.Self)]
		public void Remove_User_Successfully()
		{
			// Arrange
			var crashDoc = new CrashDoc();
			var userTable = new UserTable(crashDoc);
			var user = new User("user1");
			userTable.Add(user);

			// Act
			userTable.Remove(user);

			// Assert
			Assert.That(default(User), Is.EqualTo(userTable.Get("user1")));
		}

		[Test]
		[Parallelizable(ParallelScope.Self)]
		public void Remove_User_Invokes_OnUserRemoved()
		{
			// Arrange
			var crashDoc = new CrashDoc();
			var userTable = new UserTable(crashDoc);
			var user = new User("user1");
			userTable.Add(user);
			bool eventRaised = false;
			UserTable.OnUserRemoved += (sender, args) => eventRaised = true;

			// Act
			userTable.Remove(user);

			// Assert
			Assert.IsTrue(eventRaised);
		}

		[Test]
		[Parallelizable(ParallelScope.Self)]
		public void Get_User_Successfully()
		{
			// Arrange
			var crashDoc = new CrashDoc();
			var userTable = new UserTable(crashDoc);
			var user = new User("user1");
			userTable.Add(user);

			// Act
			User result = userTable.Get("user1");

			// Assert
			Assert.That(result, Is.EqualTo(user));
		}

		[Test]
		[Parallelizable(ParallelScope.Self)]
		public void Get_Nonexistent_User_Returns_Default()
		{
			// Arrange
			var crashDoc = new CrashDoc();
			var userTable = new UserTable(crashDoc);

			// Act
			User result = userTable.Get("user1");

			// Assert
			Assert.That(default(User), Is.EqualTo(result));
		}
	}
}
