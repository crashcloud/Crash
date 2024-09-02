using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Common.Communications;
using Crash.Properties;

using Eto.Drawing;

using Rhino.UI;

namespace Crash.UI
{
	
	[JsonConverter(typeof(SharedModelConverter))]
	public sealed class SharedModel
	{
		public SharedModel() { }

		public double UserCount { get; set; }

		public Bitmap Thumbnail { get; set; }

		public string ModelAddress { get; set; }

	}

	public class SharedModelConverter : JsonConverter<SharedModel>
	{
		public override SharedModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var model = new SharedModel();
			reader.Read();

			model.ModelAddress = reader.GetString();
			reader.Read();

			model.UserCount = reader.GetDouble();
			reader.Read();

			var thumbnailString = reader.GetString();
			if (!string.IsNullOrEmpty(thumbnailString))
			{
				var thumbnail = Convert.FromBase64String(thumbnailString);
				model.Thumbnail = new Bitmap(thumbnail);
				reader.Read();
			}

			reader.Read();
			return model;
		}

		public override void Write(Utf8JsonWriter writer, SharedModel model, JsonSerializerOptions options)
		{
			if (string.IsNullOrEmpty(model.ModelAddress))
			{
				writer.WriteStringValue("No Model Address Found");
			}
			else
			{
				writer.WriteStringValue(model.ModelAddress);
			}

			writer.WriteNumberValue(model.UserCount);

			if (model.Thumbnail == null || model.Thumbnail.IsDisposed)
			{
				var byteImage = model.Thumbnail.ToByteArray(ImageFormat.Png);
				var base64String = Convert.ToBase64String(byteImage);
				writer.WriteStringValue(base64String);
			}
			else
			{
				writer.WriteStringValue("");
			}
		}
	}
}
