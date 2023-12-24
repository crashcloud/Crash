using Crash.Common.Changes;
using Crash.Geometry;
using Crash.Handlers.InternalEvents;

namespace Crash.Handlers.Plugins.Camera.Create
{
	/// <summary>Creates a Camera from a View Event</summary>
	internal sealed class CameraCreateAction : IChangeCreateAction
	{
		private static readonly TimeSpan maxPerSecond = TimeSpan.FromMilliseconds(250);
		private CPoint lastLocation;

		private DateTime lastSentTime;
		private CPoint lastTarget;

		/// <summary>Default Constructor</summary>
		internal CameraCreateAction()
		{
			lastSentTime = DateTime.MinValue;
			lastLocation = CPoint.None;
			lastTarget = CPoint.None;
		}


		public ChangeAction Action => ChangeAction.Add;


		public bool CanConvert(object sender, CreateRecieveArgs crashArgs)
		{
			if (crashArgs.Args is not CrashViewArgs viewArgs)
			{
				return false;
			}

			var now = DateTime.UtcNow;
			var timeSinceLastSent = now - lastSentTime;
			if (timeSinceLastSent < maxPerSecond)
			{
				return false;
			}

			if (DistanceBetween(viewArgs.Location, lastLocation) < 10 &&
				DistanceBetween(viewArgs.Target, lastTarget) < 10)
			{
				return false;
			}

			lastLocation = viewArgs.Location;
			lastTarget = viewArgs.Target;
			lastSentTime = DateTime.UtcNow;

			return true;
		}


		public bool TryConvert(object sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes)
		{
			changes = Array.Empty<Change>();
			if (crashArgs.Args is not CrashViewArgs viewArgs)
			{
				changes = null;
				return false;
			}

			var userName = crashArgs.Doc.Users.CurrentUser.Name;
			var camera = new Common.View.Camera(viewArgs.Location, viewArgs.Target);
			var change = CameraChange.CreateChange(camera, userName);
			changes = new List<Change> { change };

			return true;
		}

		// TODO : Move to Crash.Changes?
		private double DistanceBetween(CPoint p1, CPoint p2)
		{
			// https://www.mathsisfun.com/algebra/distance-2-points.html
			var dist = Math.Sqrt(
								 Math.Pow(p1.X - p2.X, 2) +
								 Math.Pow(p1.Y - p2.Y, 2) +
								 Math.Pow(p1.Z - p2.Z, 2)
								);

			return dist;
		}
	}
}
