namespace Crash.Common.Communications
{
	public interface ICrashClient
	{
		/// <summary>Tests for an Active Connection</summary>
		public bool IsConnected { get; }

		/// <summary>Registers the client and its connection url</summary>
		/// <param name="userName">The User of the Client</param>
		/// <param name="url">url of the server the client will talk to</param>
		void RegisterConnection(string userName, Uri url);

		/// <summary>Stops the Connection</summary>
		public Task StopAsync();

		/// <summary>Starts the Client</summary>
		/// <exception cref="NullReferenceException">If CrashDoc is null</exception>
		/// <exception cref="Exception">If UserName is empty</exception>
		public Task StartLocalClientAsync();

		/// <summary>
		///     Pushes an Update/Transform/Payload which applies to many Changes
		///     An example of this is arraying the same item or deleting many items at once
		/// </summary>
		/// <param name="ids">The records to update</param>
		/// <param name="change">The newest changes</param>
		Task PushIdenticalChangesAsync(IEnumerable<Guid> ids, Change change);

		/// <summary>Pushes a single Change</summary>
		Task PushChangeAsync(Change change);

		/// <summary>
		///     Pushes many unique changes at once
		///     An example of this may be copying 10 unique items
		/// </summary>
		Task PushChangesAsync(IEnumerable<Change> changes);

		/// <summary>Pushes a single Change</summary>
		public event Func<IEnumerable<Guid>, Change, Task> OnRecieveIdentical;

		/// <summary>Pushes a single Change</summary>
		public event Func<Change, Task> OnRecieveChange;

		/// <summary>Pushes a single Change</summary>
		public event Func<IEnumerable<Change>, Task> OnRecieveChanges;

		/// <summary>Local Event corresponding to a Server call for Initialize</summary>
		public event Func<IEnumerable<Change>, Task> OnInitializeChanges;

		/// <summary>Local Event corresponding to a Server call for Initialize Users</summary>
		public event Func<IEnumerable<string>, Task> OnInitializeUsers;

		/// <summary>Local event wrapping Crash Args with Initialization</summary>
		public event EventHandler<CrashClient.CrashInitArgs> OnInit;
	}
}
