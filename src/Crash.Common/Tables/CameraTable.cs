using System.Collections;

using Crash.Common.Changes;
using Crash.Common.Collections;
using Crash.Common.Document;
using Crash.Common.View;

namespace Crash.Common.Tables
{
	public sealed class CameraTable : IEnumerable<Camera>
	{
		internal const int MAX_CAMERAS_IN_QUEUE = 3;

		private readonly Dictionary<string, FixedSizedQueue<Camera>> _cameraLocations;

		private readonly CrashDoc _crashDoc;


		internal CameraTable(CrashDoc hostDoc)
		{
			_cameraLocations = new Dictionary<string, FixedSizedQueue<Camera>>();
			_crashDoc = hostDoc;
		}

		public IEnumerator<Camera> GetEnumerator()
		{
			return _cameraLocations.Values.SelectMany(c => c).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		///     Returns all of the Active <see cref="Camera" />s paired to <see cref="User" />s
		/// </summary>
		public Dictionary<User, Camera> GetActiveCameras()
		{
			var cameras = new Dictionary<User, Camera>(_cameraLocations.Count);
			foreach (var cameraLocation in _cameraLocations)
			{
				var user = _crashDoc.Users.Get(cameraLocation.Key);
				if (string.IsNullOrEmpty(user.Name))
				{
					continue;
				}

				cameras.Add(user, cameraLocation.Value.FirstOrDefault());
			}

			return cameras;
		}

		/// <summary>
		///     Adds a new Camera to the table.
		///     If there are 3 in the table it will supersede one
		/// </summary>
		/// <returns>True on success, false otherwise</returns>
		public bool TryAddCamera(CameraChange cameraChange)
		{
			var user = new User(cameraChange.Owner);
			FixedSizedQueue<Camera>? queue;

			if (!_cameraLocations.ContainsKey(user.Name))
			{
				queue = new FixedSizedQueue<Camera>(MAX_CAMERAS_IN_QUEUE);
				queue.Enqueue(cameraChange.Camera);
				_cameraLocations.Add(user.Name, queue);
			}
			else
			{
				if (!_cameraLocations.TryGetValue(user.Name, out queue))
				{
					return false;
				}

				queue.Enqueue(cameraChange.Camera);
			}

			return true;
		}

		/// <summary>
		///     Attempts to retrieve the current queue of <see cref="Camera" />s based on the given User
		/// </summary>
		/// <returns>True if any found</returns>
		public bool TryGetCamera(User user, out FixedSizedQueue<Camera> cameras)
		{
			return _cameraLocations.TryGetValue(user.Name, out cameras);
		}
	}
}
