using Crash.Properties;

using Eto.Drawing;

using Rhino.Runtime;
using Rhino.UI;

namespace Crash.UI
{
	public static class Palette
	{
		private static readonly Color s_black = Color.FromArgb(0, 0, 0);
		private static readonly Color s_white = Color.FromArgb(255, 255, 255);
		private static readonly Color s_darkestGrey = Color.FromArgb(29, 29, 27);
		private static readonly Color s_darkGrey = Color.FromArgb(125, 125, 125);
		private static readonly Color s_lightGrey = Color.FromArgb(200, 200, 200);
		private static readonly Color s_lightestGrey = Color.FromArgb(220, 220, 222);

		internal static bool DarkMode => HostUtils.RunningInDarkMode;

		// Text & Highlights
		public static Color TextColour => DarkMode ? s_lightestGrey : s_darkestGrey;
		public static Color BackgroundColour => DarkMode ? s_black : s_white;
		public static Color Transparent => Color.FromArgb(0, 0, 0, 0);

		public static Color SubtleGrey => DarkMode ? s_lightGrey : s_darkGrey;

		// Crash Colours
		public static Color NavyBlue => Color.FromArgb(29, 48, 146);
		public static Color SeaGreen => Color.FromArgb(129, 226, 200);
		
	}
}
