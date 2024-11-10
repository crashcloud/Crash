using System;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;

using Eto.Drawing;

namespace Crash.Resources;

public static class CrashIcons
{
	private const int DefaultSize = 64;

	private record struct IconKey(string Key, int Size);

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
				Icons.Add(new(key, DefaultSize), bitmap);
			}
		}

	}

	public static Bitmap Close(int size) => GetIconAtSize("close.png", size);
	public static Bitmap Join(int size) => GetIconAtSize("join.png", size);
	public static Bitmap Reload(int size) => GetIconAtSize("reload.png", size);

	private static Bitmap Empty(int size) => new Bitmap(size, size, PixelFormat.Format32bppRgba);

	private static Bitmap GetIconAtSize(string key, int size)
	{
		try
		{
			if (!Icons.TryGetValue(new(key, size), out var bitmap))
			{
				var resized = Resize(key, size);
				if (resized is null) return Empty(size);
				return resized;
			}
			if (bitmap.Width == size) return bitmap;
			return new Bitmap(bitmap, size, size, ImageInterpolation.High);
		}
		catch { }
		return Empty(size);
	}

	private static Bitmap Resize(string key, int newSize)
	{
		if (Icons.TryGetValue(new(key, newSize), out var bitmap)) return bitmap;
		if (!Icons.TryGetValue(new(key, DefaultSize), out var defaultBitmap)) return Empty(newSize);

		return new Bitmap(defaultBitmap, newSize, newSize, ImageInterpolation.High);
	}

}
