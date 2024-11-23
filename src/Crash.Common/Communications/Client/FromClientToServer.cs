using Crash.Common.Events;

using Microsoft.AspNetCore.SignalR.Client;

namespace Crash.Common.Communications;

/// <summary>
///     Crash client class
/// </summary>
public sealed partial class CrashClient
{

	public Exception RegisterConnection(string userName, Uri url)
	{
		if (string.IsNullOrEmpty(userName?.Replace(" ", "")))
		{
			return new ArgumentException("Username cannot be empty or null");
		}

		if (url is null)
		{
			return new UriFormatException("URL Cannot be null");
		}

		if (!url.AbsoluteUri.Contains("/Crash"))
		{
			return new UriFormatException("URL must end in /Crash to connect!");
		}

		try
		{
			_user = userName;
			_connection = GetHubConnection(url);
			_connection.Reconnecting += InformUserOfReconnect;
			Url = url.AbsoluteUri;
			RegisterConnections();
		}
		catch (Exception ex)
		{
			return ex;
		}

		return null!;
	}

	public async Task SendChangesToServerAsync(IAsyncEnumerable<Change> changeStream)
	{
		try
		{
			await _connection.InvokeAsync(SEND_RECIEVE_STREAM, changeStream);
		}
		catch
		{
			OnPushChangeFailed?.Invoke(this, new CrashChangeArgs(CrashDoc, changeStream.ToEnumerable()));
		}
	}

	/// <summary>
	///     Fires when a Change fails to be sent to the Server
	/// </summary>
	public event EventHandler<CrashChangeArgs> OnPushChangeFailed;

}
