using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Common.Logging;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Crash.Common.Communications
{
	/// <summary>
	///     Crash client class
	/// </summary>
	public sealed class CrashClient : ICrashClient
	{
		// TODO : Move to https
		public const string DefaultURL = "http://localhost";
		public const string DefaultPort = "8080";
		private readonly CrashDoc _crashDoc;

		private HubConnection _connection;
		private string _user;

		/// <summary>
		///     Crash client constructor
		/// </summary>
		/// <param name="crashDoc">The Document to associate this Client to</param>
		public CrashClient(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
		}

		/// <summary>Stops the Connection</summary>
		public async Task StopAsync()
		{
			await _connection?.StopAsync();
		}

		/// <summary>Starts the Client</summary>
		/// <exception cref="ArgumentNullException">If CrashDoc is null</exception>
		/// <exception cref="Exception">If UserName is empty</exception>
		public async Task StartLocalClientAsync()
		{
			if (_crashDoc is null)
			{
				throw new ArgumentNullException("CrashDoc cannot be null!");
			}

			var userName = _crashDoc?.Users?.CurrentUser.Name;
			if (string.IsNullOrEmpty(userName))
			{
				throw new Exception("A User has not been assigned!");
			}

			OnInitializeChanges += InitChangesAsync;
			OnInitializeUsers += InitUsersAsync;

			await StartAsync();
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
		public static HubConnection GetHubConnection(Uri url)
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

		public static JsonHubProtocolOptions JsonOptions()
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
			_connection.On<IEnumerable<Change>>(INITIALIZE, InitializeChangesAsync);
			_connection.On<IEnumerable<string>>(INITIALIZEUSERS, InitializeUsersAsync);
			_connection.On<IEnumerable<Guid>, Change>(PUSH_IDENTICAL, RecieveIdenticalChangesAsync);
			_connection.On<Change>(PUSH_SINGLE, RecieveChangeAsync);
			_connection.On<IEnumerable<Change>>(PUSH_MANY, RecieveManyUniqueChangesAsync);

			_connection.Reconnected += ConnectionReconnectedAsync;
			_connection.Closed += ConnectionClosedAsync;
			_connection.Reconnecting += ConnectionReconnectingAsync;
		}

		/// <summary>Start the async connection</summary>
		private Task StartAsync()
		{
			return _connection.StartAsync();
		}

		public sealed class CrashInitArgs : CrashEventArgs
		{
			public readonly IEnumerable<Change> Changes;

			public CrashInitArgs(CrashDoc crashDoc, IEnumerable<Change> changes)
				: base(crashDoc)
			{
				Changes = changes;
			}
		}


		#region Connection Watchers

		private async Task ConnectionReconnectingAsync(Exception? arg)
		{
		}

		private async Task ConnectionClosedAsync(Exception? arg)
		{
			var closedTask = arg switch
			                 {
				                 HubException               => ChangesCouldNotBeSent(),
				                 WebSocketException         => ServerIndicatedPossibleClosure(),
				                 OperationCanceledException => ServerClosedUnexpectidly(),
				                 _                          => Task.CompletedTask
			                 };

			await closedTask;
		}

		private async Task ServerClosedUnexpectidly()
		{
			OnServerClosed?.Invoke(this, EventArgs.Empty);
		}

		private async Task ServerIndicatedPossibleClosure()
		{
		}

		private async Task ChangesCouldNotBeSent()
		{
			OnPushChangeFailed?.Invoke(this, EventArgs.Empty);
		}

		private Task ConnectionReconnectedAsync(string? arg)
		{
			return Task.CompletedTask;
		}

		public event EventHandler OnServerClosed;
		public event EventHandler OnPushChangeFailed;

		#endregion

		#region Connection

		public HubConnectionState State => _connection.State;

		public bool IsConnected => _connection.State != HubConnectionState.Disconnected;

		public void RegisterConnection(string userName, Uri url)
		{
			if (string.IsNullOrEmpty(userName))
			{
				throw new ArgumentException("Username cannot be empty or null");
			}

			if (url is null)
			{
				throw new UriFormatException("URL Cannot be null");
			}

			if (!url.AbsoluteUri.Contains("/Crash"))
			{
				throw new UriFormatException("URL must end in /Crash to connect!");
			}

			_user = userName;
			_connection = GetHubConnection(url);
			RegisterConnections();
		}

		#endregion

		#region Push to Server

		/// <summary>
		///     Pushes an Update/Transform/Payload which applies to many Changes
		///     An example of this is arraying the same item or deleting many items at once
		/// </summary>
		/// <param name="ids">The records to update</param>
		/// <param name="change">The newest changes</param>
		public async Task PushIdenticalChangesAsync(IEnumerable<Guid> ids, Change change)
		{
			await _connection.InvokeAsync(PUSH_IDENTICAL, ids, change);
		}

		/// <summary>Pushes a single Change</summary>
		public async Task PushChangeAsync(Change change)
		{
			await _connection.InvokeAsync(PUSH_SINGLE, change);
		}

		/// <summary>
		///     Pushes many unique changes at once
		///     An example of this may be copying 10 unique items
		/// </summary>
		public async Task PushChangesAsync(IEnumerable<Change> changes)
		{
			await _connection.InvokeAsync(PUSH_MANY, changes);
		}

		#endregion

		#region Recieve from Server

		public event Func<IEnumerable<Guid>, Change, Task> OnRecieveIdentical;

		public event Func<Change, Task> OnRecieveChange;

		public event Func<IEnumerable<Change>, Task> OnRecieveChanges;

		public event Func<IEnumerable<Change>, Task> OnInitializeChanges;

		public event Func<IEnumerable<string>, Task> OnInitializeUsers;

		public event EventHandler<CrashInitArgs> OnInit;

		public async Task InitializeChangesAsync(IEnumerable<Change> changes)
		{
			if (OnInitializeChanges is null)
			{
				return;
			}

			await OnInitializeChanges.Invoke(changes);
		}

		public async Task InitializeUsersAsync(IEnumerable<string> users)
		{
			if (OnInitializeUsers is null)
			{
				return;
			}

			await OnInitializeUsers.Invoke(users);
		}

		// TODO : This isn't calling, and needs to call the Event Dispatcher
		// TODO : Resolve this and Init
		private async Task InitChangesAsync(IEnumerable<Change> changes)
		{
			OnInitializeChanges -= InitChangesAsync;
			OnInit?.Invoke(this, new CrashInitArgs(_crashDoc, changes));
		}

		private async Task InitUsersAsync(IEnumerable<string> users)
		{
			OnInitializeUsers -= InitUsersAsync;
			// User Init
			foreach (var user in users)
			{
				_crashDoc.Users.Add(user);
			}
		}

		private async Task RecieveIdenticalChangesAsync(IEnumerable<Guid> ids, Change change)
		{
			if (OnRecieveIdentical is null)
			{
				return;
			}

			await OnRecieveIdentical.Invoke(ids, change);
		}

		private async Task RecieveChangeAsync(Change change)
		{
			if (OnRecieveChange is null)
			{
				return;
			}

			await OnRecieveChange.Invoke(change);
		}

		private async Task RecieveManyUniqueChangesAsync(IEnumerable<Change> changes)
		{
			if (OnRecieveChanges is null)
			{
				return;
			}

			await OnRecieveChanges.Invoke(changes);
		}

		#endregion

		#region consts

		private const string PUSH_IDENTICAL = "PushIdenticalChanges";
		private const string PUSH_SINGLE = "PushChange";
		private const string PUSH_MANY = "PushChanges";
		private const string INITIALIZE = "InitializeChanges";
		private const string INITIALIZEUSERS = "InitializeUsers";

		#endregion
	}
}
