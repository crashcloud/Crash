using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

using Crash.Common.App;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Common.Logging;
using Crash.Common.Serialization;
using Crash.Events;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Crash.Common.Communications
{

	public record struct IClientOptions(bool DryRun = false);

	/// <summary>
	///     Crash client class
	/// </summary>
	public sealed class CrashClient : ICrashClient
	{
		private const string PUSH_STREAM = "PushChangesThroughStream";

		// TODO : Move to https
		public const string DefaultURL = "http://localhost";
		public const string DefaultPort = "8080";
		private CrashDoc _crashDoc { get; }
		private IClientOptions Options { get; }
		public string Url { get; private set; }

		private HubConnection _connection;
		private string _user { get; set; }

		public bool ClosedByUser { get; set; } = false;

		/// <summary>
		///     Crash client constructor
		/// </summary>
		/// <param name="crashDoc">The Document to associate this Client to</param>
		public CrashClient(CrashDoc crashDoc, IClientOptions options = default)
		{
			_crashDoc = crashDoc;
			Options = options;
		}

		/// <summary>Stops the Connection</summary>
		public async Task StopAsync()
		{
			await _connection?.StopAsync();
		}

		/// <summary>Starts the Client</summary>
		/// <exception cref="ArgumentNullException">If CrashDoc is null</exception>
		/// <exception cref="Exception">If UserName is empty</exception>
		public async Task<Exception?> StartLocalClientAsync()
		{
			if (_crashDoc is null)
			{
				return new ArgumentNullException("CrashDoc cannot be null!");
			}

			var userName = _crashDoc?.Users?.CurrentUser.Name;
			if (string.IsNullOrEmpty(userName))
			{
				return new Exception("A User has not been assigned!");
			}

			if (!Options.DryRun)
			{
				OnInitializeChanges += InitChangesAsync;
				OnInitializeUsers += InitUsersAsync;
			}

			try
			{
				await StartAsync();
			}
			catch (Exception ex)
			{
				return ex;
			}

			return null;
		}

		public void CancelConnection()
		{
		}

		/// <summary>
		///     Closed event
		/// </summary>
		public event Func<Exception, Task> Closed
		{
			add => _connection.Closed += value;
			remove => _connection.Closed -= value;
		}

		/// <summary>Creates a connection to the Crash Server</summary>
		private static HubConnection GetHubConnection(Uri url)
		{
			return new HubConnectionBuilder()
				   .WithUrl(url)
				   // .ConfigureLogging(LoggingConfigurer)
				   .WithAutomaticReconnect(new[]
										   {
											   TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(100),
											   TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10)
										   })
				   .Build();
		}

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

		/// <summary>Registers Local Events responding to Server calls</summary>
		private void RegisterConnections()
		{
			RegisterEndpoint<IEnumerable<Change>>(_connection, "InitializeChanges", null, InitializeChangesAsync);
			RegisterEndpoint<IEnumerable<string>>(_connection, "InitializeUsers", null, InitializeUsersAsync);
			RegisterEndpoint<IAsyncEnumerable<Change>>(_connection, PUSH_STREAM, OnRecieveChangeStream, SendChangesThroughStream);

			_connection.Reconnected += ConnectionReconnectedAsync;
			_connection.Closed += ConnectionClosedAsync;
			_connection.Reconnecting += ConnectionReconnectingAsync;
		}

		private void RegisterEndpoint<TValue>(HubConnection connection, string name, EventHandler<TValue> serverSender, EventHandler<TValue> clientReciever)
		{
			if (clientReciever is not null)
			{
				connection.On<TValue>(name, (changes) => clientReciever.Invoke(this, changes));
			}

			if (serverSender is not null)
			{
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
				serverSender += async (sender, args) =>
				{
					try
					{
						await connection.InvokeAsync(name, args);
					}
					catch { }
				};
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
			}
		}


		/// <summary>Start the async connection</summary>
		private async Task StartAsync()
		{
			await _connection.StartAsync();
		}

		private void SendChangesThroughStream(object? sender, IAsyncEnumerable<Change> changeStream)
		{
			if (OnRecieveChangeStream is null) return;
			OnRecieveChangeStream?.Invoke(this, changeStream);
		}


		#region Connection Watchers

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
			OnServerClosed?.Invoke(this, new CrashEventArgs(_crashDoc));
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

		/// <summary>
		///     Fires when the connection to the Server closes
		/// </summary>
		public event EventHandler<CrashEventArgs> OnServerClosed;

		/// <summary>
		///     Fires when a Change fails to be sent to the Server
		/// </summary>
		public event EventHandler<CrashChangeArgs> OnPushChangeFailed;

		#endregion

		#region Connection

		public bool IsConnected => _connection is not null && _connection.State != HubConnectionState.Disconnected;

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

			return null;
		}

		private async Task InformUserOfReconnect(Exception? exception)
		{
			await Task.Delay(3000);
			if (_connection.State == HubConnectionState.Connected) return;

			CrashApp.InformUser("Attempting to reconnect to Server ...");
			_connection.Reconnected += InformUserOfReconnection;
		}

		private async Task InformUserOfReconnection(string? arg)
		{
			_connection.Reconnected -= InformUserOfReconnection;
			CrashApp.InformUser("Reconnected successfully");
		}

		#endregion

		#region Push to Server

		public async Task StreamChangesAsync(IAsyncEnumerable<Change> changeStream)
		{
			try
			{
				await _connection.InvokeAsync(PUSH_STREAM, changeStream);
			}
			catch (Exception ex)
			{
				List<Change> changes = new List<Change>();
				await foreach (var change in changeStream)
				{
					changes.Add(change);
				}
				OnPushChangeFailed?.Invoke(this, new CrashChangeArgs(_crashDoc, changes));
			}
		}

		#endregion

		#region Recieve from Server

		public event EventHandler<IAsyncEnumerable<Change>> OnRecieveChangeStream;

		public event EventHandler<IEnumerable<Change>> OnInitializeChanges;

		public event EventHandler<IEnumerable<string>> OnInitializeUsers;

		public event EventHandler<CrashInitArgs> OnInit;

		private void InitializeChangesAsync(object? sender, IEnumerable<Change> changes)
		{
			if (OnInitializeChanges is null) return;
			OnInitializeChanges.Invoke(this, changes);

			// TODO : Seems Janky
			_crashDoc.Queue.AddAction(new DummyAction());
		}

		internal class DummyAction : IdleAction
		{
			public DummyAction() : base((args) => { }, new IdleArgs(null, null), nameof(DummyAction)) { }
		}

		private void InitializeUsersAsync(object? sender, IEnumerable<string> users)
		{
			if (OnInitializeUsers is null) return;
			OnInitializeUsers.Invoke(this, users);
		}

		private void InitChangesAsync(object? sender, IEnumerable<Change> changes)
		{
			OnInitializeChanges -= InitChangesAsync;

			OnInit?.Invoke(this, new CrashInitArgs(_crashDoc, changes));
		}

		private void InitUsersAsync(object? sender, IEnumerable<string> users)
		{
			OnInitializeUsers -= InitUsersAsync;
			// User Init
			foreach (var user in users)
			{
				_crashDoc.Users.Add(user);
			}
		}

		#endregion

	}

}
