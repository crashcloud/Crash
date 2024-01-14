using System.Runtime.CompilerServices;

using Crash.Common.App;
using Crash.Common.Communications;
using Crash.Common.Tables;
using Crash.Events;

[assembly: InternalsVisibleTo("Crash.Common.Tests")]

namespace Crash.Common.Document
{
	/// <summary>The Crash Document</summary>
	public sealed class CrashDoc : IEquatable<CrashDoc>, IDisposable
	{
		private readonly Guid _id;

		#region constructors

		/// <summary>Constructs a Crash Doc</summary>
		public CrashDoc()
		{
			_id = Guid.NewGuid();

			Users = new UserTable(this);
			TemporaryChangeTable = new TemporaryChangeTable(this);
			RealisedChangeTable = new RealisedChangeTable(this);
			Cameras = new CameraTable(this);

			LocalClient = new CrashClient(this);

			Queue = new IdleQueue(this);
		}

		#endregion

		private bool _documentIsBusy { get; set; }

		// TODO : What if someone DOES something when we're adding stuff?
		/// <summary>
		///     Marks the Document as in a "Busy State" state which means
		///     Nothing can be sent to the server
		/// </summary>
		public bool DocumentIsBusy
		{
			get => _documentIsBusy;
			set
			{
				_documentIsBusy = value;
				CrashApp.Log($"{nameof(DocumentIsBusy)} was set to {value}");
			}
		}

		#region Queue

		/// <summary>The Idle Queue for the Crash Document</summary>
		public IdleQueue Queue { get; private set; }

		#endregion

		public void Dispose()
		{
			LocalClient?.StopAsync();
		}

		#region Connectivity

		/// <summary>The Local Client for the Crash Doc</summary>
		public ICrashClient LocalClient { get; set; }

		/// <summary>The current Documents Dispatcher</summary>
		public IEventDispatcher Dispatcher { get; set; }

		#endregion

		#region Tables

		/// <summary>The Users Table for the Crash Doc</summary>
		public readonly UserTable Users;

		/// <summary>The Changes Table for the Crash Doc</summary>
		public readonly TemporaryChangeTable TemporaryChangeTable;

		public readonly RealisedChangeTable RealisedChangeTable;

		/// <summary>The Camera Table for the crash Doc</summary>
		public readonly CameraTable Cameras;

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
			return _id.GetHashCode();
		}

		#endregion
	}
}
