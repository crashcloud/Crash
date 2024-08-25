using Crash.Common.Events;

namespace Crash.Common.Communications
{
	public interface ICrashClient
	{
		/// <summary>Tests for an Active Connection</summary>
		public bool IsConnected { get; }

		/// <summary>The connection address</summary>
		public string Url { get; }

		/// <summary>Registers the client and its connection url</summary>
		/// <param name="userName">The User of the Client</param>
		/// <param name="url">url of the server the client will talk to</param>
		Exception RegisterConnection(string userName, Uri url);

		/// <summary>Stops the Connection</summary>
		public Task StopAsync();

		/// <summary>Starts the Client</summary>
		/// <exception cref="NullReferenceException">If CrashDoc is null</exception>
		/// <exception cref="Exception">If UserName is empty</exception>
		public Task<Exception?> StartLocalClientAsync();

		/// <summary>
		///     Pushes many unique changes at once
		///     An example of this may be copying 10 unique items
		/// </summary>
		public Task StreamChangesAsync(IAsyncEnumerable<Change> changeStream);

		/// <summary>Local Event corresponding to a Server call for Initialize</summary>
		public event Func<IEnumerable<Change>, Task> OnInitializeChanges;

		/// <summary>Local Event corresponding to a Server call for Initialize Users</summary>
		public event Func<IEnumerable<string>, Task> OnInitializeUsers;

		/// <summary>Local event wrapping Crash Args with Initialization</summary>
		public event EventHandler<CrashInitArgs> OnInit;
	}
}
