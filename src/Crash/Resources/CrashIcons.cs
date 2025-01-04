using Eto.Drawing;

using Rhino.Runtime;

using CPalette = Crash.UI.Palette;

namespace Crash.Resources;

public static class CrashIcons
{
	private const int DefaultSize = 64;

	private record struct IconKey(string Key, int Size, Color Colour);

	// TODO : Improve Icon Size Caching
	static Dictionary<IconKey, Bitmap> Icons { get; set; }

	private static string[] Extensions { get; } = new[] { ".png", ".ico", ".jpg", ".bmp" };

	static CrashIcons()
	{
		Icons = new();
		var assembly = typeof(CrashIcons).Assembly;
		var resourceNames = assembly.GetManifestResourceNames();

		foreach (var resourceName in resourceNames)
		{
			try
			{
				var splitName = resourceName.Split(new[] { "Crash.Resources." }, StringSplitOptions.RemoveEmptyEntries)?.LastOrDefault();
				if (string.IsNullOrEmpty(splitName)) continue;
				bool validExtension = false;
				foreach (var ext in Extensions)
				{
					if (!string.Equals(Path.GetExtension(splitName), ext, StringComparison.OrdinalIgnoreCase)) continue;
					validExtension = true;
					break;
				}
				if (!validExtension) continue;

				var key = Path.GetFileNameWithoutExtension(splitName);

				using (Stream stream = assembly.GetManifestResourceStream(resourceName))
				{
					var bitmap = new Bitmap(stream);
					Icons.Add(new(key, DefaultSize, Colors.Transparent), bitmap);
				}
			}
			catch { }
		}

	}

	public static Bitmap Icon(string key, int size, Eto.Drawing.Color color) => GetIconAtSize(key, size, color);
	public static Bitmap Icon(string key, int size) => GetIconAtSize(key, size, Default);

	private static Bitmap Empty(int size) => new Bitmap(size, size, PixelFormat.Format32bppRgba);

	private static Bitmap GetIconAtSize(string key, int size, Color colour)
	{
		try
		{
			var iconKey = new IconKey(key, size, colour);
			if (!Icons.TryGetValue(iconKey, out var bitmap))
			{
				var resized = Resize(iconKey);
				if (resized is null) return Empty(size);
				Icons.Add(iconKey, resized);
				return resized;
			}
			if (bitmap.Width == size) return bitmap;
			return new Bitmap(bitmap, size, size, ImageInterpolation.High);
		}
		catch { }
		return Empty(size);
	}

	private static Bitmap Resize(IconKey key)
	{
		if (Icons.TryGetValue(key, out var bitmap)) return bitmap;
		if (!Icons.TryGetValue(new(key.Key, DefaultSize, Colors.Transparent), out var defaultBitmap)) return Empty(key.Size);
		RecolourImage(defaultBitmap, key.Colour);

		return new Bitmap(defaultBitmap, key.Size, key.Size, ImageInterpolation.High);
	}

	private static Color Default => HostUtils.RunningInDarkMode ? CPalette.LightGray : CPalette.Black;

	private static void RecolourImage(Bitmap bitmap, Color color)
	{
		if (color.Ab < 10) return;

		using (var data = bitmap.Lock())
		{
			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Height; y++)
				{
					var pixel = data.GetPixel(x, y);
					if (pixel.Ab < 10) continue;
					if (pixel.Rb < 50 && pixel.Gb < 50 && pixel.Bb < 50)
					{
						data.SetPixel(x, y, color);
					}
				}
			}
		}

	}
}
