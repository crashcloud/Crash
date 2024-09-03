using System.Text.Json;
using System.Text.Json.Serialization;

using Eto.Drawing;

namespace Crash.Handlers.Data
{
	public class BitmapConverter : JsonConverter<Eto.Drawing.Bitmap>
	{

		public override Eto.Drawing.Bitmap? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var base64String = reader.GetString();
			var base64Data = Convert.FromBase64String(base64String);

			return new Bitmap(base64Data); ;
		}

		public override void Write(Utf8JsonWriter writer, Eto.Drawing.Bitmap bitmap, JsonSerializerOptions options)
		{
			var byteImage = bitmap.ToByteArray(ImageFormat.Png);
			var base64String = Convert.ToBase64String(byteImage);
			writer.WriteStringValue(base64String);
		}
	}
}
