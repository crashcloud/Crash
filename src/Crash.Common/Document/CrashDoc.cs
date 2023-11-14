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
			TemporaryChangeTable = new TemporaryChangeTable(this);
			Cameras = new CameraTable(this);
			RealisedChangeTable = new RealisedChangeTable(this);

			Queue = new IdleQueue(this);
		}

		#endregion

		/// <summary>
		///     Marks the Document as in an "Init" state which means
		///     Nothing added or changed in the Document will affect anything else
		/// </summary>
		public bool IsInit { get; set; } = false;

		// TODO : What if someone DOES something when we're adding stuff?
		/// <summary>
		///     Marks the Document as in a "Someone Is Done" state which means
		///     that a Release action from someone else is currently happening,
		///     this puts the document in a similar state to IsInit where nothing that happens here will tell the server
		/// </summary>
		public bool SomeoneIsDone { get; set; } = false;

		/// <summary>
		///     Marks the Document as in a "Transform" state which means
		///     anything that happens during the transform will not be sent to the server
		///     This is because Transforms in Rhino are quite complex.
		/// </summary>
		public bool IsTransformActive { get; set; }

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
		public readonly TemporaryChangeTable TemporaryChangeTable;

		public readonly RealisedChangeTable RealisedChangeTable;

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
