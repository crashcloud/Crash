﻿using System.Collections;

using Crash.Common.Document;

namespace Crash.Common.Tables
{
	/// <summary>The Table of Users</summary>
	public sealed class UserTable : IEnumerable<User>
	{
		private readonly CrashDoc _crashDoc;

		private readonly Dictionary<string, User> _users;

		/// <summary>Creates a new empty User Table</summary>
		/// <param name="hostDoc">The CrashDoc this UserTable is attached to</param>
		public UserTable(CrashDoc hostDoc)
		{
			_users = new Dictionary<string, User>();
			_crashDoc = hostDoc;
		}

		/// <summary>The current User of the Document</summary>
		public User CurrentUser { get; set; }

		public IEnumerator<User> GetEnumerator()
		{
			return _users.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>Adds a User only if they are not the Current User</summary>
		public bool Add(User user)
		{
			if (CurrentUser.Equals(user))
			{
				return false;
			}

			if (!_users.ContainsKey(user.Name))
			{
				_users.Add(user.Name, user);

				try
				{
					OnUserAdded?.Invoke(this, new UserEventArgs(user));
				}
				catch { }

				return true;
			}

			return false;
		}

		/// <summary>Adds a User only if they are not the Current User</summary>
		public bool Add(string userName)
		{
			var user = new User(userName);
			return Add(user);
		}

		/// <summary>Removes a User</summary>
		public void Remove(User user)
		{
			Remove(user.Name);
		}

		/// <summary>Removes a User</summary>
		public void Remove(string userName)
		{
			var cleaned = User.CleanedUserName(userName);
			if (_users.TryGetValue(cleaned, out var user))
			{
				OnUserRemoved?.Invoke(this, new UserEventArgs(user));
			}

			_users.Remove(cleaned);
		}

		/// <summary>Get a User by name</summary>
		public User Get(string userName)
		{
			var cleanUserName = User.CleanedUserName(userName);
			if (_users.ContainsKey(cleanUserName))
			{
				return _users[cleanUserName];
			}

			return default;
		}

		public void Update(User user)
		{
			_users.Remove(user.Name);
			_users.Add(user.Name, user);
		}

		/// <summary>Fires each time a User is successfully added via Add</summary>
		public static event EventHandler<UserEventArgs> OnUserAdded;

		/// <summary>Fires each time a User is successfuly removed via Remove</summary>
		public static event EventHandler<UserEventArgs> OnUserRemoved;
	}

	public sealed class UserEventArgs : EventArgs
	{
		public readonly User User;

		public UserEventArgs(User user)
		{
			User = user;
		}
	}
}
