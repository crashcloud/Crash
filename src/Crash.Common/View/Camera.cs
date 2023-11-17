using System.Text.Json.Serialization;

using Crash.Common.Serialization;
using Crash.Geometry;

namespace Crash.Common.View
{
	/// <summary>The Camera represents a user view with two points.</summary>
	[JsonConverter(typeof(CameraConverter))]
	public struct Camera : IEquatable<Camera>
	{
		/// <summary>The location of the viewpont of the camera</summary>
		public CPoint Location { get; set; }

		/// <summary>The target viewpoint of the camera</summary>
		public CPoint Target { get; set; }

		/// <summary>A datetime stamp for the Camera</summary>
		public DateTime Stamp { get; internal set; }

		/// <summary>Creates a Camera</summary>
		public Camera(CPoint location, CPoint target)
		{
			Location = location;
			Target = target;
			Stamp = DateTime.UtcNow;
		}

		/// <summary>A non-existant Camera</summary>
		public static Camera None => new(CPoint.None, CPoint.None);

		/// <summary>Checks for Validity</summary>
		public bool IsValid()
		{
			return Location != Target &&
			       Location != CPoint.None &&
			       Target != CPoint.None &&
			       Stamp > DateTime.MinValue &&
			       Stamp < DateTime.MaxValue;
		}

		
		public override int GetHashCode()
		{
			return Location.GetHashCode() ^ Target.GetHashCode();
		}

		/// <summary>Equality Comparison</summary>
		public override bool Equals(object? obj)
		{
			return obj is Camera camera && camera == this;
		}

		/// <summary>Equality Comparison</summary>
		public bool Equals(Camera other)
		{
			return this == other;
		}

		/// <summary>Equality Comparison</summary>
		public static bool operator ==(Camera c1, Camera c2)
		{
			return c1.Location.Round(3).Equals(c2.Location.Round(3)) &&
			       c1.Target.Round(3).Equals(c2.Target.Round(3));
		}

		/// <summary>Inqquality Comparison</summary>
		public static bool operator !=(Camera c1, Camera c2)
		{
			return !(c1 == c2);
		}

		
		public override string ToString()
		{
			return $"Camera {Location}/{Target}";
		}
	}
}
