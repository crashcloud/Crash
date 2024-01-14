using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Common.View;
using Crash.Geometry;

namespace Crash.Common.Serialization
{
	/// <summary>
	///     Converts the Camera class to and from JSON efficiently
	/// </summary>
	public sealed class CameraConverter : JsonConverter<Camera>
	{
		public override Camera Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartArray)
			{
				throw new JsonException();
			}

			if (reader.Read() && reader.TryGetDouble(out var targetX) &&
			    reader.Read() && reader.TryGetDouble(out var targetY) &&
			    reader.Read() && reader.TryGetDouble(out var targetZ) &&
			    reader.Read() && reader.TryGetDouble(out var locationX) &&
			    reader.Read() && reader.TryGetDouble(out var locationY) &&
			    reader.Read() && reader.TryGetDouble(out var locationZ) &&
			    reader.Read() && reader.TryGetInt64(out var ticks) &&
			    reader.Read() && reader.TokenType == JsonTokenType.EndArray)
			{
				var location = new CPoint(locationX, locationY, locationZ);
				var target = new CPoint(targetX, targetY, targetZ);

				return new Camera(location, target) { Stamp = new DateTime(ticks) };
			}

			throw new JsonException();
		}

		public override void Write(Utf8JsonWriter writer, Camera value, JsonSerializerOptions options)
		{
			var target = value.Target;
			var location = value.Location;

			writer.WriteStartArray();

			writer.WriteNumberValue(target.X);
			writer.WriteNumberValue(target.Y);
			writer.WriteNumberValue(target.Z);

			writer.WriteNumberValue(location.X);
			writer.WriteNumberValue(location.Y);
			writer.WriteNumberValue(location.Z);

			writer.WriteNumberValue(value.Stamp.Ticks);

			writer.WriteEndArray();
		}
	}
}
