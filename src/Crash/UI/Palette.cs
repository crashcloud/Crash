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

		// public static Color SeaGreen => Color.FromArgb(0, 38, 38);

		public static Color White => Color.FromArgb(233, 241, 247);

		public static Color Yellow => Color.FromArgb(246, 174, 45);

		public static Color Black => Color.FromArgb(0, 21, 20); // 19, 27, 35
		public static Color DarkGray => Color.FromArgb(33, 39, 56);
		public static Color Gray => Color.FromArgb(65, 67, 97);

		public static Color Shadow => new Color(0f, 0f, 0f, 0.2f);

		public static Color Green => Color.FromArgb(9, 129, 74);

		public static Color Lime => Color.FromArgb(209, 214, 70);

		public static Color Purple => Color.FromArgb(109, 89, 122);

		public static Color Red => Color.FromArgb(215, 38, 56);

		public static Color Blue => Color.FromArgb(90, 177, 187);

		public static Pen GetDashedPen(Color color, float width = 4f)
		{
			var pen = new Pen(color, width);
			pen.DashStyle = new DashStyle(4, 4);
			return pen;
		}

		public static TextureBrush GetHashedTexture(int size = 6, float opaciy = 0.5f)
		{
			var image = new Bitmap(new Size(size * 4, size * 4), PixelFormat.Format32bppRgba);
			using (var g = new Graphics(image))
			{
				g.Clear(Palette.Black);
				int i = 1;
				for (int y = -4; y < size * 8; y++)
				{
					bool draw = false;
					i--;
					for (int x = -4; x < size * 8; x += size)
					{
						draw = !draw;
						if (!draw) continue;

						g.FillRectangle(Palette.Gray, new Rectangle(x + i, y, size, 1));
					}
				}
			}

			TextureBrush brush = new TextureBrush(image, opaciy);
			return brush;
		}
	}
}
