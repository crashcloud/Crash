using System.Runtime.CompilerServices;

using Crash.Common.Communications;
using Crash.Common.Tables;
using Crash.Events;

[assembly: InternalsVisibleTo("Crash.Common.Tests")]

namespace Crash.Common.Document
{
	/// <summary>The Crash Document</summary>
	public sealed class CrashDoc : IEquatable<CrashDoc>, IDisposable
	{
		public readonly Guid Id;

		#region constructors

		/// <summary>Constructs a Crash Doc</summary>
		public CrashDoc()
		{
			Id = Guid.NewGuid();

			Users = new UserTable(this);
			CacheTable = new ChangeTable(this);
			Cameras = new CameraTable(this);

			Queue = new IdleQueue(this);
		}

		#endregion

		#region Queue

		/// <summary>The Idle Queue for the Crash Document</summary>
		public IdleQueue Queue { get; private set; }

		#endregion


		public void Dispose()
		{
			LocalClient?.StopAsync();
			LocalServer?.Stop();
		}

		#region Tables

		/// <summary>The Users Table for the Crash Doc</summary>
		public readonly UserTable Users;

		/// <summary>The Changes Table for the Crash Doc</summary>
		public readonly ChangeTable CacheTable;

		/// <summary>The Camera Table for the crash Doc</summary>
		public readonly CameraTable Cameras;

		#endregion

		#region Connectivity

		/// <summary>The Local Client for the Crash Doc</summary>
		public ICrashClient LocalClient { get; set; }

		/// <summary>The Local Server for the Crash Doc</summary>
		public CrashServer? LocalServer { get; set; }

		#endregion

		#region Methods

		public bool Equals(CrashDoc? other)
		{
			return other?.GetHashCode() == GetHashCode();
		}


		public override bool Equals(object? obj)
		{
			return obj is CrashDoc other && Equals(other);
		}


		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		#endregion
	}
}
