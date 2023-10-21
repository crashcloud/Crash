using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Common.Logging;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
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

		private readonly HubConnection _connection;
		private readonly CrashDoc _crashDoc;
		private readonly string _user;

		/// <summary>
		///     Crash client constructor
		/// </summary>
		/// <param name="crashDoc">The Document to associate this Client to</param>
		/// <param name="userName">The User of the Client</param>
		/// <param name="url">url of the server the client will talk to</param>
		public CrashClient(CrashDoc crashDoc, string userName, Uri url)
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

			_crashDoc = crashDoc;
			_user = userName;
			_connection = GetHubConnection(url);
			RegisterConnections();
		}

		public HubConnectionState State => _connection.State;

		public bool IsConnected => _connection.State != HubConnectionState.Disconnected;

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

			OnInitializeChanges += Init;
			OnInitializeUsers += InitUsers;

			await StartAsync();
		}

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

		public async Task InitializeChangesAsync(IEnumerable<Change> changes)
		{
			;
		}

		public async Task InitializeUsersAsync(IEnumerable<string> users)
		{
			;
		}

		/// <summary>Local Event corresponding to a Server call for Initialize</summary>
		public event Action<IEnumerable<Change>> OnInitializeChanges;

		/// <summary>Local Event corresponding to a Server call for Initialize Users</summary>
		public event Action<IEnumerable<string>> OnInitializeUsers;

		public event Action<IEnumerable<Guid>, Change> OnPushIdentical;

		public event Action<Change> OnPushChange;

		public event Action<IEnumerable<Change>> OnPushChanges;

		/// <summary>Done</summary>
		public async Task DoneAsync()
		{
			var doneChange = new Change { Owner = _user, Action = ChangeAction.Release, Type = "Crash.DoneChange" };
			await PushChangeAsync(doneChange);
		}

		/// <summary>Releases a collection of changes</summary>
		public async Task DoneRangeAsync(IEnumerable<Guid> changeIds)
		{
			var doneChange = new Change
			{
				Owner = string.Empty,
				Action = ChangeAction.Release,
				Type = "Crash.DoneChange"
			};
			await PushIdenticalChangesAsync(changeIds, doneChange);
		}

		/// <summary>Local Event corresponding to a Server call for Done</summary>
		public event Action<string> OnDone;

		/// <summary>Local Event corresponding to a Server call for Done Range</summary>
		public event Action<IEnumerable<Guid>> OnDoneRange;

		/// <summary>
		///     Closed event
		/// </summary>
		public event Func<Exception, Task> Closed
		{
			add => _connection.Closed += value;
			remove => _connection.Closed -= value;
		}


		/// <summary>Creates a connection to the Crash Server</summary>
		internal static HubConnection GetHubConnection(Uri url)
		{
			return new HubConnectionBuilder()
				.WithUrl(url).AddJsonProtocol()
				.AddJsonProtocol(opts => JsonOptions())
				// .ConfigureLogging(LoggingConfigurer)
				.WithAutomaticReconnect(new[]
				{
					TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(10)
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
		internal void RegisterConnections()
		{
			_connection.On<IEnumerable<Change>>(INITIALIZE, changes => OnInitializeChanges?.Invoke(changes));
			_connection.On<IEnumerable<string>>(INITIALIZEUSERS, users => OnInitializeUsers?.Invoke(users));

			_connection.On<IEnumerable<Guid>, Change>(PUSH_IDENTICAL,
				(ids, change) => OnPushIdentical?.Invoke(ids, change));
			_connection.On<Change>(PUSH_SINGLE, change => OnPushChange?.Invoke(change));
			_connection.On<IEnumerable<Change>>(PUSH_MANY, changes => OnPushChanges?.Invoke(changes));

			_connection.Reconnected += ConnectionReconnectedAsync;
			_connection.Closed += ConnectionClosedAsync;
			_connection.Reconnecting += ConnectionReconnectingAsync;
		}

		// This isn't calling, and needs to call the Event Dispatcher
		private void Init(IEnumerable<Change> changes)
		{
			OnInitializeChanges -= Init;
			OnInit?.Invoke(this, new CrashInitArgs(_crashDoc, changes));
		}

		private void InitUsers(IEnumerable<string> users)
		{
			OnInitializeUsers -= InitUsers;
			// User Init
			foreach (var user in users)
			{
				_crashDoc.Users.Add(user);
			}
		}

		public static void CloseLocalServer(CrashDoc crashDoc)
		{
			crashDoc?.LocalServer?.Stop();
			crashDoc?.LocalServer?.Dispose();
		}

		private Task ConnectionReconnectingAsync(Exception? arg)
		{
			Console.WriteLine(arg);
			return Task.CompletedTask;
		}

		private Task ConnectionClosedAsync(Exception? arg)
		{
			Console.WriteLine(arg);
			return Task.CompletedTask;
		}

		private Task ConnectionReconnectedAsync(string? arg)
		{
			Console.WriteLine(arg);
			return Task.CompletedTask;
		}

		/// <summary>Start the async connection</summary>
		private Task StartAsync()
		{
			return _connection.StartAsync();
		}

		public static event EventHandler<CrashInitArgs> OnInit;

		public sealed class CrashInitArgs : CrashEventArgs
		{
			public readonly IEnumerable<Change> Changes;

			public CrashInitArgs(CrashDoc crashDoc, IEnumerable<Change> changes)
				: base(crashDoc)
			{
				Changes = changes;
			}
		}

		#region consts

		private const string PUSH_IDENTICAL = "PushIdenticalChanges";
		private const string PUSH_SINGLE = "PushChange";
		private const string PUSH_MANY = "PushChanges";
		private const string INITIALIZE = "InitializeChanges";
		private const string INITIALIZEUSERS = "InitializeUsers";

		#endregion
	}
}
