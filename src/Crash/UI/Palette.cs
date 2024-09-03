using Eto.Drawing;

using Rhino.Runtime;

namespace Crash.UI
{
	public static class Palette
	{
		private static readonly Color s_darkGrey = Color.FromArgb(125, 125, 125);
		private static readonly Color s_lightGrey = Color.FromArgb(200, 200, 200);

		internal static bool DarkMode => HostUtils.RunningInDarkMode;

		// Text & Highlights

		public static Color SubtleGrey => DarkMode ? s_lightGrey : s_darkGrey;

		// Crash Colours
		public static Color NavyBlue => Color.FromArgb(29, 48, 146);
		public static Color SeaGreen => Color.FromArgb(129, 226, 200);

		public static TextureBrush GetHashedTexture(int size)
		{
			var image = new Bitmap(new Size(size * 4, size * 4), PixelFormat.Format32bppRgba);
			using (var g = new Graphics(image))
			{
				g.Clear(Colors.Black);
				int i = 1;
				for (int y = -4; y < size * 8; y++)
				{
					bool draw = false;
					i--;
					for (int x = -4; x < size * 8; x += size)
					{
						draw = !draw;
						if (!draw) continue;

						g.FillRectangle(Colors.DarkSlateGray, new Rectangle(x + i, y, size, 1));
					}
				}
			}

			TextureBrush brush = new TextureBrush(image, 0.5f);
			return brush;
		}
	}
}
