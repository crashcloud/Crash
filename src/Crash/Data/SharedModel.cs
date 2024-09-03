using System.Text.Json.Serialization;

using Eto.Drawing;

namespace Crash.Data
{

	[JsonConverter(typeof(SharedModelConverter))]
	public record class SharedModel
	{
		public SharedModel() { }

		public double UserCount { get; set; }

		public Bitmap Thumbnail { get; set; }

		public string ModelAddress { get; set; }

	}
}
