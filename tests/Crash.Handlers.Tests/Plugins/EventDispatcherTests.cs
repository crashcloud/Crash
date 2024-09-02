using System.Collections;

using Crash.Changes;
using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Common.Events;
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

				var addArgs = new CrashObjectEventArgs(null, point, rhinoId, Guid.NewGuid());
				yield return new object[]
							 {
								 nameof(ICrashClient.PushChangeAsync), ChangeAction.Add | ChangeAction.Temporary,
								 addArgs
							 };

				var deleteArgs = new CrashObjectEventArgs(null, null, Guid.NewGuid(), Guid.NewGuid());
				yield return new object[] { nameof(ICrashClient.PushChangeAsync), ChangeAction.Remove, deleteArgs };

				var crashObject = new CrashObject(Guid.NewGuid(), Guid.NewGuid());
				var selectArgs = CrashSelectionEventArgs.CreateSelectionEvent(null, new[] { crashObject });
				yield return new object[] { nameof(ICrashClient.PushChangeAsync), ChangeAction.Locked, selectArgs };


				var deSelectArgs = CrashSelectionEventArgs.CreateDeSelectionEvent(null, new[] { crashObject });
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

			eventDispatcher = new EventDispatcher(crashDoc);
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			rhinoDoc.Dispose();
			crashDoc.Dispose();
		}

		[TestCaseSource(nameof(DispatchEvents))]
		public async Task TestAddDispatch(string callName, ChangeAction action, EventArgs args)
		{
			await eventDispatcher.NotifyServerAsync(action, this, args);
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
													   { nameof(ICrashClient.StreamChangesAsync), 0 },
													   { nameof(ICrashClient.OnInitializeChanges), 0 },
													   { nameof(ICrashClient.OnInitializeUsers), 0 }
												   };

		public string Url { get; }

		internal DispatcherTestClient()
		{
			// On InitialiseChanges
			OnInitializeUsers += async args => { IncrementCallCount(nameof(ICrashClient.OnInitializeUsers)); };
		}

		public bool IsConnected { get; } = true;
		public event EventHandler<CrashInitArgs>? OnInit;

		public Exception RegisterConnection(string userName, Uri url)
		{
			throw new NotImplementedException();
		}

		public async Task StopAsync()
		{
			IncrementCallCount(nameof(ICrashClient.StopAsync));
			await Task.CompletedTask;
		}

		public async Task<Exception?> StartLocalClientAsync()
		{
			IncrementCallCount(nameof(ICrashClient.StartLocalClientAsync));
			await Task.CompletedTask;
			return null;
		}

		public async Task StreamChangesAsync(IAsyncEnumerable<Change> changes)
		{
			IncrementCallCount(nameof(ICrashClient.StreamChangesAsync));
			await Task.CompletedTask;
		}

		public event Func<IEnumerable<string>, Task>? OnInitializeUsers;
		public event Func<IEnumerable<Guid>, Change, Task> OnRecieveIdentical;
		public event Func<Change, Task> OnRecieveChange;
		public event Func<IEnumerable<Change>, Task> OnRecieveChanges;
		public event Func<IEnumerable<Change>, Task> OnInitializeChanges;

		public event Action<string>? OnDone;
		public event Action<IEnumerable<Guid>>? OnDoneRange;
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
		}
	}
}
