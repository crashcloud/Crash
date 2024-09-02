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
		private void SetValue(ref Utf8JsonReader reader, SharedModel model)
		{
			var propertyName = reader.GetString();
			reader.Read();

			if (propertyName.Equals(nameof(SharedModel.Thumbnail)))
			{
				var thumbnailString = reader.GetString();
				if (!string.IsNullOrEmpty(thumbnailString))
				{
					var thumbnail = Convert.FromBase64String(thumbnailString);
					model.Thumbnail = new Bitmap(thumbnail);
				}
			}
			else if (propertyName.Equals(nameof(SharedModel.UserCount)))
			{
				model.UserCount = reader.GetDouble();
			}
			else if (propertyName.Equals(nameof(SharedModel.ModelAddress)))
			{
				model.ModelAddress = reader.GetString();
			}
		}

		public override SharedModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var model = new SharedModel();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					return model;
				}
				else if (reader.TokenType == JsonTokenType.PropertyName)
				{
					SetValue(ref reader, model);
				}
			}

			return model;
		}

		public override void Write(Utf8JsonWriter writer, SharedModel model, JsonSerializerOptions options)
		{
			// Model Address
			var modelAddress = "No model address found";
			if (!string.IsNullOrEmpty(model.ModelAddress))
			{
				modelAddress = model.ModelAddress;
			}
			writer.WriteString(nameof(SharedModel.ModelAddress), modelAddress);

			// User Count
			writer.WriteNumber(nameof(SharedModel.UserCount), model.UserCount);


			if (model.Thumbnail == null || model.Thumbnail.IsDisposed)
			{
				var byteImage = model.Thumbnail.ToByteArray(ImageFormat.Png);
				var base64String = Convert.ToBase64String(byteImage);
				writer.WriteString(nameof(SharedModel.Thumbnail), base64String);
			}
		}
	}
}
