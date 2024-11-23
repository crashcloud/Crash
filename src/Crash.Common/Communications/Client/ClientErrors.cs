using System.Net.WebSockets;

using Crash.Common.App;
using Crash.Common.Events;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Crash.Common.Communications;

/// <summary>
///     Crash client class
/// </summary>
public sealed partial class CrashClient
{

	private async Task ConnectionReconnectingAsync(Exception? arg)
	{
		var closedTask = arg switch
		{
			HubException => ChangesCouldNotBeSent(),
			_ => Task.CompletedTask
		};

		await closedTask;
	}

	private async Task ConnectionClosedAsync(Exception? arg)
	{
		var closedTask = arg switch
		{
			HubException => ChangesCouldNotBeSent(),
			WebSocketException => ServerIndicatedPossibleClosure(),
			OperationCanceledException => ServerClosedUnexpectidly(),
			_ => Task.CompletedTask
		};

		await closedTask;
	}

	private async Task ServerClosedUnexpectidly()
	{
		if (ClosedByUser) return;
		OnServerClosed?.Invoke(this, new CrashEventArgs(CrashDoc));
	}

	private async Task ServerIndicatedPossibleClosure()
	{

	}

	private async Task ChangesCouldNotBeSent()
	{
		// TODO : Remove
	}

	private Task ConnectionReconnectedAsync(string? arg)
	{
		return Task.CompletedTask;
	}

	private async Task InformUserOfReconnect(Exception? exception)
	{
		var timeoutTime = new TimeSpan(ReconnectTimes.Sum(s => s.Ticks)).Milliseconds;
		await Task.Delay(timeoutTime / 2);
		if (_connection.State == HubConnectionState.Connected) return;

		CrashApp.InformUser("Attempting to reconnect to Server ...");
		_connection.Reconnected += InformUserOfReconnection;
	}

	private async Task InformUserOfReconnection(string? arg)
	{
		_connection.Reconnected -= InformUserOfReconnection;
		CrashApp.InformUser("Reconnected successfully");
	}

	/// <summary>
	///     Fires when the connection to the Server closes and cannot be recovered
	/// </summary>
	public event EventHandler<CrashEventArgs> OnServerClosed;

}
