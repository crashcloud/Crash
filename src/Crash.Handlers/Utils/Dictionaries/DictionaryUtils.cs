using System.Drawing;

using Rhino.DocObjects;

namespace Crash.Handlers.Utils
{
	internal static class Extents
	{
		private static bool TryGetUserSpecificLayerValue(this Dictionary<string, string> dict, string key,
			string userName, out string returnValue)
		{
			var fullKey = DictionaryUtils.GetNewKey(key, userName);
			dict.TryGetValue(fullKey, out var foundValue);

			if (string.IsNullOrEmpty(foundValue))
			{
				returnValue = null;
				return false;
			}

			returnValue = foundValue;
			return true;
		}

		internal static bool TryGetLayerValue(this Dictionary<string, string> dict, string key,
			string userName, double defaultValue, out double value)
		{
			if (!dict.TryGetUserSpecificLayerValue(key, userName, out var foundValue))
			{
				value = defaultValue;
				return false;
			}

			if (double.TryParse(foundValue, out value))
			{
				return true;
			}

			value = defaultValue;
			return false;
		}

		internal static bool TryGetLayerValue(this Dictionary<string, string> dict, string key,
			string userName, bool defaultValue, out bool value)
		{
			if (!dict.TryGetUserSpecificLayerValue(key, userName, out var foundValue))
			{
				value = defaultValue;
				return false;
			}

			if (bool.TryParse(foundValue, out value))
			{
				return true;
			}

			value = defaultValue;
			return false;
		}

		internal static bool TryGetLayerValue(this Dictionary<string, string> dict, string key,
			string userName, Color defaultValue, out Color value)
		{
			if (!dict.TryGetUserSpecificLayerValue(key, userName, out var foundValue))
			{
				value = defaultValue;
				return false;
			}

			if (!int.TryParse(foundValue, out var intValue))
			{
				value = defaultValue;
				return false;
			}

			value = Color.FromArgb(intValue);
			return true;
		}

		internal static bool TryGetLayerValue(this Dictionary<string, string> dict, string key,
			string userName, int defaultValue, out int value)
		{
			if (!dict.TryGetUserSpecificLayerValue(key, userName, out var foundValue))
			{
				value = defaultValue;
				return false;
			}

			if (int.TryParse(foundValue, out value))
			{
				return true;
			}

			value = defaultValue;
			return false;
		}

		internal static bool TryGetLayerValue(this Dictionary<string, string> dict, string key,
			string userName, string defaultValue, out string value)
		{
			if (dict.TryGetUserSpecificLayerValue(key, userName, out value))
			{
				return true;
			}

			value = defaultValue;
			return false;
		}
	}


	internal class DictionaryUtils
	{
		private const string KeyDivider = ";";

		static DictionaryUtils()
		{
			s_userSpecificKeys = new HashSet<string> { nameof(Layer.IsLocked), nameof(Layer.IsVisible) };
		}

		private static HashSet<string> s_userSpecificKeys { get; }

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


		internal static string GetNewKey(string key, string userName)
		{
			return $"New{KeyDivider}{GetUserSpecificKey(key, userName)}{key}";
		}

		internal static string GetOldKey(string key, string userName)
		{
			return $"Old{KeyDivider}{GetUserSpecificKey(key, userName)}{key}";
		}

		private static string GetKey(string key, string userName)
		{
			return GetNewKey(key, userName) ?? GetOldKey(key, userName);
		}

		internal static string GetUserSpecificKey(string key, string userName)
		{
			if (s_userSpecificKeys.Contains(key))
			{
				return $"{userName}{KeyDivider}";
			}

			return null;
		}

		internal static string GetNeutralKey(string key, string userName)
		{
			var neutralKey = key.Replace("Old", "")
			                    .Replace("New", "")
			                    .Replace(KeyDivider, "")
			                    .Replace(userName, "");

			return neutralKey;
		}

		internal static bool IsDifferent<TObject>(Getter<TObject> getter, TObject oldState, TObject newState,
			out string oldValue,
			out string newValue)
		{
			oldValue = getter.Get(oldState);
			newValue = getter.Get(newState);

			if (!string.IsNullOrEmpty(oldValue))
			{
				return !oldValue.Equals(newValue);
			}

			return !string.IsNullOrEmpty(newValue);
		}
	}

	internal record struct Getter<TObject>(Func<TObject, string?> Get);

	internal record struct Setter<TObject>(Action<TObject, string?> Set);
}
