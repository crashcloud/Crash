using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Common.Logging;
using Crash.Events;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Crash.Common.Communications;

public record struct IClientOptions(bool DryRun = false);

internal sealed class DummyAction : IdleAction
{
	public DummyAction() : base((args) => { }, new IdleArgs(null, null), nameof(DummyAction)) { }
}

/// <summary>
///     Crash client class
/// </summary>
public sealed partial class CrashClient : ICrashClient
{

	#region Consts

	private const string SEND_RECIEVE_STREAM = "PushChangesThroughStream";
	private const string INITIALIZE_CHANGES = "InitializeChanges";
	private const string INITIALIZE_USERS = "InitializeUsers";

	public const string DefaultURL = "http://localhost";
	public const string DefaultPort = "8080";

	#endregion

	#region Properties

	private CrashDoc CrashDoc { get; }
	private IClientOptions Options { get; }
	public string Url { get; private set; }

	private HubConnection _connection;
	private string _user { get; set; }

	public bool ClosedByUser { get; set; } = false;

	public bool IsConnected => _connection is not null && _connection.State != HubConnectionState.Disconnected;

	#endregion

	/// <summary>
	///     Crash client constructor
	/// </summary>
	/// <param name="crashDoc">The Document to associate this Client to</param>
	public CrashClient(CrashDoc crashDoc, IClientOptions options = default)
	{
		CrashDoc = crashDoc;
		Options = options;
	}

	#region Create Connection

	private static JsonHubProtocolOptions JsonOptions()
	{
		return new JsonHubProtocolOptions
		{
			PayloadSerializerOptions = new JsonSerializerOptions
			{
				IgnoreReadOnlyFields = true,
				IgnoreReadOnlyProperties = true,
				NumberHandling = JsonNumberHandling
													  .AllowNamedFloatingPointLiterals
			}
		};
	}

	private static void LoggingConfigurer(ILoggingBuilder loggingBuilder)
	{
		var logLevel = Debugger.IsAttached ? LogLevel.Trace : LogLevel.Information;
		loggingBuilder.SetMinimumLevel(logLevel);
		var loggingProvider = new CrashLoggerProvider();
		loggingBuilder.AddProvider(loggingProvider);
	}

	#endregion

	#region StopStart

	/// <summary>Starts the Client</summary>
	/// <exception cref="ArgumentNullException">If CrashDoc is null</exception>
	/// <exception cref="Exception">If UserName is empty</exception>
	public async Task<Exception?> StartLocalClientAsync()
	{
		var userName = CrashDoc?.Users?.CurrentUser.Name;
		if (string.IsNullOrEmpty(userName))
		{
			return new Exception("A User has not been assigned!");
		}

		try
		{
			await _connection.StartAsync();
		}
		catch (Exception ex)
		{
			return ex;
		}

		return null;
	}

	/// <summary>Stops the Connection</summary>
	public async Task StopAsync()
	{
		await _connection?.StopAsync();
	}

	#endregion

}

