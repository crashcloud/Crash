using System.Collections;
using System.Text.Json;

using Crash.Common.Changes;
using Crash.Common.Collections;
using Crash.Common.Document;
using Crash.Common.View;

namespace Crash.Common.Tables
{
	public sealed class CameraTable : IEnumerable<Camera>
	{
		public const int MAX_CAMERAS_IN_QUEUE = 3;

		private readonly CrashDoc _crashDoc;

		private readonly Dictionary<string, FixedSizedQueue<Camera>> cameraLocations;


		public CameraTable(CrashDoc hostDoc)
		{
			cameraLocations = new Dictionary<string, FixedSizedQueue<Camera>>();
			_crashDoc = hostDoc;
		}

		public bool CameraIsInvalid { get; set; }

		public IEnumerator<Camera> GetEnumerator()
		{
			return cameraLocations.Values.SelectMany(c => c).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		public void OnCameraChange(string userName, Change cameraChange)
		{
			if (string.IsNullOrEmpty(userName))
			{
				return;
			}

			var user = _crashDoc.Users.Get(userName);
			if (!user.IsValid())
			{
				return;
			}

			var newCamera = JsonSerializer.Deserialize<Camera>(cameraChange.Payload);
			if (!newCamera.IsValid())
			{
				return;
			}

			CameraIsInvalid = true;

			// Add to Cache
			if (cameraLocations.TryGetValue(user.Name, out var previousCameras))
			{
				previousCameras.Enqueue(newCamera);
			}
			else
			{
				var newStack = new FixedSizedQueue<Camera>(MAX_CAMERAS_IN_QUEUE);
				newStack.Enqueue(newCamera);
				cameraLocations.Add(user.Name, newStack);
			}
		}

		public Dictionary<User, Camera> GetActiveCameras()
		{
			var cameras = new Dictionary<User, Camera>(cameraLocations.Count);
			foreach (var cameraLocation in cameraLocations)
			{
				var user = _crashDoc.Users.Get(cameraLocation.Key);
				cameras.Add(user, cameraLocation.Value.FirstOrDefault());
			}

			return cameras;
		}

		public bool TryAddCamera(CameraChange cameraChange)
		{
			var user = new User(cameraChange.Owner);
			FixedSizedQueue<Camera>? queue;

			if (!cameraLocations.ContainsKey(user.Name))
			{
				queue = new FixedSizedQueue<Camera>(MAX_CAMERAS_IN_QUEUE);
				queue.Enqueue(cameraChange.Camera);
				cameraLocations.Add(user.Name, queue);
			}
			else
			{
				if (!cameraLocations.TryGetValue(user.Name, out queue))
				{
					return false;
				}

				queue.Enqueue(cameraChange.Camera);
			}

			return true;
		}

		public void TryAddCamera(IEnumerable<CameraChange> cameraChanges)
		{
			foreach (var camaeraChange in cameraChanges.OrderBy(cam => cam.Stamp))
			{
				TryAddCamera(camaeraChange);
			}
		}

		public bool TryGetCamera(User user, out FixedSizedQueue<Camera> cameras)
		{
			return cameraLocations.TryGetValue(user.Name, out cameras);
		}
	}
}
