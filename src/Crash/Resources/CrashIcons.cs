using System;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;

using Eto.Drawing;

namespace Crash.Resources;

public static class CrashIcons
{

	private record struct IconCache(Bitmap Image, int Size);

	// TODO : Improve Icon Size Caching
	static Dictionary<string, Bitmap> Icons { get; set; }

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
				Icons.Add(key, bitmap);
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
			if (!Icons.TryGetValue(key, out var bitmap)) return Empty(size);
			if (bitmap.Width == size) return bitmap;
			return new Bitmap(bitmap, size, size, ImageInterpolation.High);

		}
		catch { }
		return Empty(size);
	}
}
