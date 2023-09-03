namespace Crash.Common.Communications
{
	public interface ICrashClient
	{

		/// <summary>Tests for an Active Connection</summary>
		public bool IsConnected { get; }

		/// <summary>Stops the Connection</summary>
		public Task StopAsync();

		/// <summary>Starts the Client</summary>
		/// <exception cref="NullReferenceException">If CrashDoc is null</exception>
		/// <exception cref="Exception">If UserName is empty</exception>
		public Task StartLocalClientAsync();

		/// <summary>
		///     Update task
		/// </summary>
		/// <param name="id">id</param>
		/// <param name="Change">Change</param>
		/// <returns></returns>
		public Task UpdateAsync(Change Change);

		/// <summary>
		///     Delete task
		/// </summary>
		/// <param name="id">id</param>
		/// <returns>returns task</returns>
		public Task DeleteAsync(Guid id);

		/// <summary>Adds a change to databiase </summary>
		public Task AddAsync(Change change);

		/// <summary>Done</summary>
		public Task DoneAsync();

		/// <summary>Releases a collection of changes</summary>
		public Task DoneAsync(IEnumerable<Guid> changeIds);

		/// <summary>Lock event</summary>
		public Task LockAsync(Guid id);

		/// <summary>Unlock event</summary>
		public Task UnlockAsync(Guid id);

		/// <summary>CameraChange event</summary>
		public Task CameraChangeAsync(Change change);

		/// <summary>Local Event corresponding to a Server call for Add</summary>
		public event Action<Change> OnAdd;

		/// <summary>Local Event corresponding to a Server call for Delete</summary>
		public event Action<Guid> OnDelete;

		/// <summary>Local Event corresponding to a Server call for Update</summary>
		public event Action<Change> OnUpdate;

		/// <summary>Local Event corresponding to a Server call for Done</summary>
		public event Action<string> OnDone;

		/// <summary>Local Event corresponding to a Server call for Done Range</summary>
		public event Action<IEnumerable<Guid>> OnDoneRange;

		/// <summary>Local Event corresponding to a Server call for Lock</summary>
		public event Action<string, Guid> OnLock;

		/// <summary>Local Event corresponding to a Server call for Unlock</summary>
		public event Action<string, Guid> OnUnlock;

		/// <summary>Local Event corresponding to a Server call for Initialize</summary>
		public event Action<IEnumerable<Change>> OnInitialize;

		/// <summary>Local Event corresponding to a Server call for Initialize Users</summary>
		public event Action<IEnumerable<string>> OnInitializeUsers;

		/// <summary>Local Event corresponding to a Server call for Camera Change</summary>
		public event Action<Change> OnCameraChange;

	}
}
