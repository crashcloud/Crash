using System.Collections;

using Crash.Changes;
using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins;
using Crash.Handlers.Plugins.Camera;
using Crash.Handlers.Plugins.Geometry;
using Crash.Handlers.Plugins.Initializers;

using Rhino;
using Rhino.Geometry;

namespace Crash.Handlers.Tests.Plugins
{
	[RhinoFixture]
	public sealed class EventDispatcherTests
	{
		private static readonly RhinoDoc rhinoDoc = RhinoDoc.CreateHeadless(null);
		private CrashDoc crashDoc;
		private EventDispatcher eventDispatcher;

		public static IEnumerable DispatchEvents
		{
			get
			{
				var point = new Point(Point3d.Origin);
				var rhinoId = rhinoDoc.Objects.Add(point);

				var addArgs = new CrashObjectEventArgs(point, rhinoId, Guid.NewGuid());
				yield return new object[]
				             {
					             nameof(ICrashClient.PushChangeAsync), ChangeAction.Add | ChangeAction.Temporary,
					             addArgs
				             };

				var deleteArgs = new CrashObjectEventArgs(null, Guid.NewGuid(), Guid.NewGuid());
				yield return new object[] { nameof(ICrashClient.PushChangeAsync), ChangeAction.Remove, deleteArgs };

				var crashObject = new CrashObject(Guid.NewGuid(), Guid.NewGuid());
				var selectArgs = CrashSelectionEventArgs.CreateSelectionEvent(new[] { crashObject });
				yield return new object[] { nameof(ICrashClient.PushChangeAsync), ChangeAction.Locked, selectArgs };


				var deSelectArgs = CrashSelectionEventArgs.CreateDeSelectionEvent(new[] { crashObject });
				yield return new object[] { nameof(ICrashClient.PushChangeAsync), ChangeAction.Unlocked, deSelectArgs };

				// Where do Transforms go?
				// nameof(ICrashClient)

				//
				yield return new object[] { nameof(ICrashClient.PushChangeAsync), ChangeAction.Update, null };

				/*
				var args = EventArgs.Empty;
				yield return new object[] { nameof(ICrashClient.OnUpdate), ChangeAction.Update, args };
				yield return new object[] { nameof(ICrashClient.OnDone), ChangeAction.None, args };
				yield return new object[] { nameof(ICrashClient.OnDoneRange), ChangeAction.None, args };
				yield return new object[] { nameof(ICrashClient.OnLock), ChangeAction.Locked, args };
				yield return new object[] { nameof(ICrashClient.OnUnlock), ChangeAction.Unlocked, args };
				yield return new object[] { nameof(ICrashClient.OnInitialize), ChangeAction.None, args };
				yield return new object[] { nameof(ICrashClient.OnInitializeUsers), ChangeAction.None, args };
				yield return new object[] { nameof(ICrashClient.OnCameraChange), ChangeAction.Add, args };
				*/
			}
		}

		[OneTimeSetUp]
		public void Setup()
		{
			crashDoc = CrashDocRegistry.CreateAndRegisterDocument(rhinoDoc);
			crashDoc.LocalClient = new DispatcherTestClient();

			eventDispatcher = new EventDispatcher();
			eventDispatcher.RegisterDefinition(new GeometryChangeDefinition());
			eventDispatcher.RegisterDefinition(new CameraChangeDefinition());
			eventDispatcher.RegisterDefinition(new DoneDefinition());
		}

		[SetUp]
		public void SetUp()
		{
			var client = (crashDoc.LocalClient = new DispatcherTestClient()) as DispatcherTestClient;
			Assert.That(client.CallCount, Is.Not.Empty);
			foreach (var callCounter in client.CallCount)
			{
				Assert.That(callCounter.Value, Is.EqualTo(0));
			}
		}

		[TearDown]
		public void TearDown()
		{
			crashDoc.LocalClient = null;
		}

		[TestCaseSource(nameof(DispatchEvents))]
		public async Task TestAddDispatch(string callName, ChangeAction action, EventArgs args)
		{
			await eventDispatcher.NotifyServerAsync(action, this, args, rhinoDoc);
			AssertCallCount(callName, 1);
		}

		private void AssertCallCount(string callName, int callCount)
		{
			var client = crashDoc.LocalClient as DispatcherTestClient;
			Assert.That(client.CallCount[callName], Is.EqualTo(callCount));
		}
	}

	internal sealed class DispatcherTestClient : ICrashClient
	{
		public Dictionary<string, int> CallCount = new()
		                                           {
			                                           { nameof(ICrashClient.StopAsync), 0 },
			                                           { nameof(ICrashClient.StartLocalClientAsync), 0 },
			                                           { nameof(ICrashClient.PushIdenticalChangesAsync), 0 },
			                                           { nameof(ICrashClient.PushChangeAsync), 0 },
			                                           { nameof(ICrashClient.PushChangesAsync), 0 },
			                                           { nameof(ICrashClient.InitializeChangesAsync), 0 },
			                                           { nameof(ICrashClient.InitializeUsersAsync), 0 },
			                                           { nameof(ICrashClient.OnPushIdentical), 0 },
			                                           { nameof(ICrashClient.OnPushChange), 0 },
			                                           { nameof(ICrashClient.OnPushChanges), 0 },
			                                           { nameof(ICrashClient.OnInitializeChanges), 0 },
			                                           { nameof(ICrashClient.OnInitializeUsers), 0 }
		                                           };

		internal DispatcherTestClient()
		{
			// On InitialiseChanges
			OnInitializeUsers += args => { IncrementCallCount(nameof(ICrashClient.OnInitializeUsers)); };
		}

		public bool IsConnected { get; } = true;
		public event Action<IEnumerable<string>>? OnInitializeUsers;
		public event EventHandler<CrashClient.CrashInitArgs>? OnInit;
		public event Action<IEnumerable<Guid>, Change> OnPushIdentical;
		public event Action<Change> OnPushChange;
		public event Action<IEnumerable<Change>> OnPushChanges;
		public event Action<IEnumerable<Change>> OnInitializeChanges;

		public async Task InitializeChangesAsync(IEnumerable<Change> changes)
		{
			IncrementCallCount(nameof(ICrashClient.InitializeChangesAsync));
			await Task.CompletedTask;
		}

		public async Task InitializeUsersAsync(IEnumerable<string> users)
		{
			IncrementCallCount(nameof(ICrashClient.InitializeUsersAsync));
			await Task.CompletedTask;
		}

		public void RegisterConnection(string userName, Uri url)
		{
			throw new NotImplementedException();
		}

		public async Task StopAsync()
		{
			IncrementCallCount(nameof(ICrashClient.StopAsync));
			await Task.CompletedTask;
		}

		public async Task StartLocalClientAsync()
		{
			IncrementCallCount(nameof(ICrashClient.StartLocalClientAsync));
			await Task.CompletedTask;
		}

		public async Task PushIdenticalChangesAsync(IEnumerable<Guid> ids, Change change)
		{
			IncrementCallCount(nameof(ICrashClient.PushIdenticalChangesAsync));
			await Task.CompletedTask;
		}

		public async Task PushChangeAsync(Change change)
		{
			IncrementCallCount(nameof(ICrashClient.PushChangeAsync));
			await Task.CompletedTask;
		}

		public async Task PushChangesAsync(IEnumerable<Change> changes)
		{
			IncrementCallCount(nameof(ICrashClient.PushChangesAsync));
			await Task.CompletedTask;
		}

		public event Action<string>? OnDone;
		public event Action<IEnumerable<Guid>>? OnDoneRange;

		public async Task InitializeUsersAsync(IEnumerable<Change> changes)
		{
			IncrementCallCount(nameof(ICrashClient.InitializeUsersAsync));
			await Task.CompletedTask;
		}

		public event Action<Change>? OnAdd;
		public event Action<Guid>? OnDelete;
		public event Action<Change>? OnUpdate;
		public event Action<string, Guid>? OnLock;
		public event Action<string, Guid>? OnUnlock;
		public event Action<IEnumerable<Change>>? OnInitialize;
		public event Action<Change>? OnCameraChange;

		private void IncrementCallCount(string name)
		{
			var count = CallCount[name];
			count += 1;
			CallCount[name] = count;
			;
		}
	}
}
