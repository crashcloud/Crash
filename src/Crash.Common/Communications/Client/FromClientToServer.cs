using Crash.Common.Events;

using Microsoft.AspNetCore.SignalR.Client;

namespace Crash.Common.Communications;

public class MissingHubConnection : Exception
{
	public MissingHubConnection() : base("Hub Connection could not be found or created!") {}
}

/// <summary>
///     Crash client class
/// </summary>
public sealed partial class CrashClient
{

	internal Func<Uri, IRetryPolicy, Task<HubConnection>> GetHubConnection { get; set; }

	internal static IRetryPolicy RetryPolicy => new CrashRetryPolicy();

	public async Task<Exception> RegisterConnection(string userName, Uri url)
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
			if (GetHubConnection is null)
				return new MissingHubConnection();

			_connection = await GetHubConnection(url, RetryPolicy);
			if (_connection is null)
				return new MissingHubConnection();

			_connection.Reconnecting += HandleReconnectAttempt;
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
