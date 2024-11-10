using System;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;

using Eto.Drawing;

using Rhino.Runtime;

namespace Crash.Resources;

public static class CrashIcons
{
	private const int DefaultSize = 64;

	private record struct IconKey(string Key, int Size, bool DarkMode);

	// TODO : Improve Icon Size Caching
	static Dictionary<IconKey, Bitmap> Icons { get; set; }

	static CrashIcons()
	{
		Icons = new();
		var assembly = typeof(CrashIcons).Assembly;
		var resourceNames = assembly.GetManifestResourceNames();

		foreach (var resourceName in resourceNames)
		{
			var splitName = resourceName.Split(new[] { "Crash.Resources." }, StringSplitOptions.RemoveEmptyEntries)?.LastOrDefault();
			if (string.IsNullOrEmpty(splitName)) continue;
			if (!splitName.Contains(".png")) continue;
			var key = splitName;

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				var bitmap = new Bitmap(stream);
				Icons.Add(new(key, DefaultSize, false), bitmap);
			}
		}

	}

	public static Bitmap Icon(string key, int size) => GetIconAtSize(key, size);

	public static Bitmap Close(int size) => GetIconAtSize("close.png", size);
	public static Bitmap Join(int size) => GetIconAtSize("join.png", size);
	public static Bitmap Reload(int size) => GetIconAtSize("reload.png", size);

	private static Bitmap Empty(int size) => new Bitmap(size, size, PixelFormat.Format32bppRgba);

	private static Bitmap GetIconAtSize(string key, int size)
	{
		try
		{
			var iconKey = new IconKey(key, size, HostUtils.RunningInDarkMode);
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
		catch (Exception ex)
		{
			;
		}
		return Empty(size);
	}

	private static Bitmap Resize(IconKey key)
	{
		if (Icons.TryGetValue(key, out var bitmap)) return bitmap;
		if (!Icons.TryGetValue(new(key.Key, DefaultSize, false), out var defaultBitmap)) return Empty(key.Size);
		if (HostUtils.RunningInDarkMode)
		{
			RecolourImage(defaultBitmap);
		}

		return new Bitmap(defaultBitmap, key.Size, key.Size, ImageInterpolation.High);
	}

	private static void RecolourImage(Bitmap bitmap)
	{
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
						data.SetPixel(x, y, Colors.White);
					}
				}
			}
		}

	}
}
