using System.Drawing;

namespace Crash.Handlers.Utils
{
	internal class RhinoObjectsUtils
	{
		internal static void EmptySetValue<T>(T thing, object value) { }

		internal static int GetIntOrDefault(string value, int _default = -1)
		{
			if (int.TryParse(value, out var result))
			{
				return result;
			}

			return _default;
		}

		internal static double GetDoubleOrDefault(string value, double _default = 0.0)
		{
			if (double.TryParse(value, out var result))
			{
				return result;
			}

			return _default;
		}

		internal static Color GetColourOrDefault(string value, Color defaultValue)
		{
			if (!int.TryParse(value, out var result))
			{
				return defaultValue;
			}

			return Color.FromArgb(result);
		}

		internal static string SerializeColour(Color colour)
		{
			return colour.ToArgb().ToString();
		}

		internal static bool GetBoolOrDefault(string value, bool defaultValue = true)
		{
			if (bool.TryParse(value, out var result))
			{
				return result;
			}

			return defaultValue;
		}

		internal static string GetStringOrDefault(string value, string defaultValue)
		{
			if (string.IsNullOrEmpty(value))
			{
				return defaultValue ?? string.Empty;
			}

			return value;
		}

		public static TEnum ParseEnumOrDefault<TEnum>(string value, TEnum _default) where TEnum : struct, Enum
		{
			if (Enum.TryParse(value, true, out TEnum enumValue))
			{
				return enumValue;
			}

			return _default;
		}
	}
}
