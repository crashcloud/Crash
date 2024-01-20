using System.Text.Json.Serialization;

using Crash.Common.Communications;
using Crash.Properties;

using Eto.Drawing;

using Rhino.UI;

namespace Crash.UI
{
	[Serializable]
	public sealed class SharedModel
	{
		public SharedModel() { }

		internal SharedModel(SharedModel sharedModel)
		{
			Loaded = sharedModel.Loaded;
			modelAddress = sharedModel.ModelAddress;
			Users = sharedModel.Users;
		}

		// Conditionals
		[JsonIgnore] internal bool? Loaded { get; set; } = false;

		[JsonIgnore]
		public Bitmap Signal => Loaded switch
		                        {
			                        true  => (Crash.UI.Palette.DarkMode ? Icons.Wifi_Light : Icons.Wifi_Dark).ToEto(),
			                        false => (Crash.UI.Palette.DarkMode ? Icons.WifiOff_Light : Icons.WifiOff_Dark).ToEto(),
			                        null => (Crash.UI.Palette.DarkMode ? Icons.WifiUnstable_Light : Icons.WifiUnstable_Dark)
				                        .ToEto()
		                        };

		[JsonIgnore] public Bitmap UserIcon => (Crash.UI.Palette.DarkMode ? Icons.User_Light : Icons.User_Dark).ToEto();

		[JsonIgnore] public string UserCount => Users?.Length.ToString() ?? "0";

		public Bitmap Thumbnail { get; set; }

		private string modelAddress { get; set; }

		public string ModelAddress
		{
			get => modelAddress;
			set
			{
				modelAddress = value;
			}
		}

		public string[] Users { get; set; } = Array.Empty<string>();

	}
}
