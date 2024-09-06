
using System.Text.Json.Serialization;

using Eto.Drawing;

namespace Crash.Handlers.Data
{

	public sealed class SharedModel
	{
		public SharedModel() { }

		public double UserCount { get; set; }

		[JsonConverter(typeof(BitmapConverter))]
		public Bitmap Thumbnail { get; set; }

		public string ModelAddress { get; set; }

		public DateTime LastOpened { get; set; } = DateTime.UtcNow;

		// Should prevent unecessary loadings on recalc!
		// public enumThingy State

	}

}
