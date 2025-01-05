using Crash.Common.Events;

using Microsoft.AspNetCore.SignalR.Client;

namespace Crash.Common.Communications;

/// <summary>
///     Crash client class
/// </summary>
public sealed partial class CrashClient
{

	private bool ChangesInitialized { get; set; }
	private bool UsersInitialized { get; set; }

	private async Task InitializeChangesAsyncOnce(IAsyncEnumerable<Change> changes)
	{
		var enumerableChanges = changes.ToEnumerable().ToList();
		OnStartInitialization?.Invoke(this, new CrashInitArgs(CrashDoc, enumerableChanges.Count));

		_connection.Remove(INITIALIZE_CHANGES);
		await this.CrashDoc.Dispatcher.NotifyClientAsync(enumerableChanges);

		ChangesInitialized = true;

		if (ChangesInitialized && UsersInitialized)
			OnFinishInitialization?.Invoke(this, EventArgs.Empty);
	}

	private async Task InitializeUsersAsyncOnce(IAsyncEnumerable<string> users)
	{
		_connection.Remove(INITIALIZE_USERS);
		await foreach (var user in users)
		{
			this.CrashDoc.Users.Add(user);
		}

		UsersInitialized = true;

		if (ChangesInitialized && UsersInitialized)
			OnFinishInitialization?.Invoke(this, EventArgs.Empty);
	}

	private async Task RecieveChangesFromServerToClientAsync(IAsyncEnumerable<Change> changes)
	{
		await this.CrashDoc.Dispatcher.NotifyClientAsync(changes.ToEnumerable());
	}

	/// <summary>Registers Local Events responding to Server calls</summary>
	private void RegisterConnections()
	{
		if (Options.DryRun) return;

		_connection.Closed += ConnectionClosedAsync;
		_connection.Reconnecting += ConnectionReconnectingAsync;

		_connection.On<IAsyncEnumerable<Change>>(INITIALIZE_CHANGES, InitializeChangesAsyncOnce);
		_connection.On<IAsyncEnumerable<string>>(INITIALIZE_USERS, InitializeUsersAsyncOnce);
		_connection.On<IAsyncEnumerable<Change>>(SEND_RECIEVE_STREAM, RecieveChangesFromServerToClientAsync);
	}

	public event EventHandler<CrashInitArgs> OnStartInitialization;

	public event EventHandler OnFinishInitialization;

}
