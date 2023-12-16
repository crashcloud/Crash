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
	}
}
