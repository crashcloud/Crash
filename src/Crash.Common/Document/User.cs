﻿using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace Crash.Common.Document
{
	/// <summary>The state of the Camera for this user</summary>
	public enum CameraState
	{
		None = 0,
		Visible = 1,
		Follow = 2
	}

	/// <summary>An external collaborator</summary>
	public struct User : IEquatable<User>
	{
		public static Color DefaultColour = Color.Gray;

		private string _name;

		/// <summary>Is this user Visible?</summary>
		public bool Visible { get; set; } = true;

		/// <summary>Name of the user</summary>
		public string Name
		{
			get => _name;
			private set => _name = CleanedUserName(value);
		}

		/// <summary>Color of the user</summary>
		public Color Color { get; set; }

		/// <summary>The current state of the Users Camera</summary>
		public CameraState Camera { get; set; } = CameraState.Visible;


		/// <summary>
		///     User Constructor
		/// </summary>
		/// <param name="inputName">the name of the user</param>
		public User(string inputName)
		{
			Name = inputName;

			if (string.IsNullOrEmpty(inputName))
			{
				Color = DefaultColour;
			}
			else
			{
				var md5 = MD5.Create();
				var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(Name));
				Color = Color.FromArgb(hash[0], hash[1], hash[2]);
			}
		}

		/// <summary>Checks user for being valid</summary>
		public bool IsValid()
		{
			return !string.IsNullOrEmpty(Name);
		}

		// TODO : Add cleaning methods
		public static string CleanedUserName(string username)
		{
			return username.ToLowerInvariant();
		}

		public override int GetHashCode()
		{
			return Name?.GetHashCode() ?? -1;
		}

		public override bool Equals(object? obj)
		{
			return obj is User user && Equals(user);
		}


		public bool Equals(User other)
		{
			return GetHashCode() == other.GetHashCode();
		}

		public static bool operator ==(User left, User right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(User left, User right)
		{
			return !(left == right);
		}
	}
}
