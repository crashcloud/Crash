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
					             nameof(ICrashClient.AddAsync), ChangeAction.Add | ChangeAction.Temporary, addArgs
				             };

				var deleteArgs = new CrashObjectEventArgs(null, Guid.NewGuid(), Guid.NewGuid());
				yield return new object[] { nameof(ICrashClient.DeleteAsync), ChangeAction.Remove, deleteArgs };

				var crashObject = new CrashObject(Guid.NewGuid(), Guid.NewGuid(), null);
				var selectArgs = new CrashSelectionEventArgs(true, new[] { crashObject });
				yield return new object[] { nameof(ICrashClient.LockAsync), ChangeAction.Locked, selectArgs };


				var deSelectArgs = new CrashSelectionEventArgs(false, new[] { crashObject });
				yield return new object[] { nameof(ICrashClient.UnlockAsync), ChangeAction.Unlocked, deSelectArgs };

				// Where do Transforms go?
				// nameof(ICrashClient)

				//
				yield return new object[] { nameof(ICrashClient.UpdateAsync), ChangeAction.Update, null };

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
			await eventDispatcher.NotifyDispatcher(action, this, args, rhinoDoc);
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
			                                           { nameof(ICrashClient.UpdateAsync), 0 },
			                                           { nameof(ICrashClient.DeleteAsync), 0 },
			                                           { nameof(ICrashClient.AddAsync), 0 },
			                                           { nameof(ICrashClient.DoneAsync), 0 },
			                                           { nameof(ICrashClient.LockAsync), 0 },
			                                           { nameof(ICrashClient.UnlockAsync), 0 },
			                                           { nameof(ICrashClient.CameraChangeAsync), 0 },
			                                           { nameof(ICrashClient.OnAdd), 0 },
			                                           { nameof(ICrashClient.OnDelete), 0 },
			                                           { nameof(ICrashClient.OnUpdate), 0 },
			                                           { nameof(ICrashClient.OnDone), 0 },
			                                           { nameof(ICrashClient.OnDoneRange), 0 },
			                                           { nameof(ICrashClient.OnLock), 0 },
			                                           { nameof(ICrashClient.OnUnlock), 0 },
			                                           { nameof(ICrashClient.OnInitialize), 0 },
			                                           { nameof(ICrashClient.OnInitializeUsers), 0 },
			                                           { nameof(ICrashClient.OnCameraChange), 0 }
		                                           };

		internal DispatcherTestClient()
		{
			OnAdd += args => { IncrementCallCount(nameof(ICrashClient.OnAdd)); };
			OnDelete += args => { IncrementCallCount(nameof(ICrashClient.OnDelete)); };
			OnUpdate += args => { IncrementCallCount(nameof(ICrashClient.OnUpdate)); };
			OnDone += args => { IncrementCallCount(nameof(ICrashClient.OnDone)); };
			OnDoneRange += args => { IncrementCallCount(nameof(ICrashClient.OnDoneRange)); };
			OnLock += (args, id) => { IncrementCallCount(nameof(ICrashClient.OnLock)); };
			OnUnlock += (args, id) => { IncrementCallCount(nameof(ICrashClient.OnUnlock)); };
			OnInitialize += args => { IncrementCallCount(nameof(ICrashClient.OnInitialize)); };
			OnInitializeUsers += args => { IncrementCallCount(nameof(ICrashClient.OnInitializeUsers)); };
			OnCameraChange += args => { IncrementCallCount(nameof(ICrashClient.OnCameraChange)); };
		}

		public bool IsConnected { get; } = true;

		public event Action<Change>? OnAdd;
		public event Action<Guid>? OnDelete;
		public event Action<Change>? OnUpdate;
		public event Action<string>? OnDone;
		public event Action<IEnumerable<Guid>>? OnDoneRange;
		public event Action<string, Guid>? OnLock;
		public event Action<string, Guid>? OnUnlock;
		public event Action<IEnumerable<Change>>? OnInitialize;
		public event Action<IEnumerable<string>>? OnInitializeUsers;
		public event Action<Change>? OnCameraChange;

		public Task StopAsync()
		{
			IncrementCallCount(nameof(ICrashClient.StopAsync));
			return Task.CompletedTask;
		}

		public Task StartLocalClientAsync()
		{
			IncrementCallCount(nameof(ICrashClient.StartLocalClientAsync));
			return Task.CompletedTask;
		}

		public Task UpdateAsync(Change Change)
		{
			IncrementCallCount(nameof(ICrashClient.UpdateAsync));
			return Task.CompletedTask;
		}

		public Task DeleteAsync(Guid id)
		{
			IncrementCallCount(nameof(ICrashClient.DeleteAsync));
			return Task.CompletedTask;
		}

		public Task AddAsync(Change change)
		{
			IncrementCallCount(nameof(ICrashClient.AddAsync));
			return Task.CompletedTask;
		}

		public Task DoneAsync()
		{
			IncrementCallCount(nameof(ICrashClient.DoneAsync));
			return Task.CompletedTask;
		}

		public Task DoneAsync(IEnumerable<Guid> changeIds)
		{
			IncrementCallCount(nameof(ICrashClient.DoneAsync));
			return Task.CompletedTask;
		}

		public Task LockAsync(Guid id)
		{
			IncrementCallCount(nameof(ICrashClient.LockAsync));
			return Task.CompletedTask;
		}

		public Task UnlockAsync(Guid id)
		{
			IncrementCallCount(nameof(ICrashClient.UnlockAsync));
			return Task.CompletedTask;
		}

		public Task CameraChangeAsync(Change change)
		{
			IncrementCallCount(nameof(ICrashClient.CameraChangeAsync));
			return Task.CompletedTask;
		}

		private void IncrementCallCount(string name)
		{
			var count = CallCount[name];
			count += 1;
			CallCount[name] = count;
			;
		}
	}
}
