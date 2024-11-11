using System;

using Crash.Commands;
using Crash.Common.App;
using Crash.Common.Document;
using Crash.Handlers;
using Crash.Handlers.Data;
using Crash.Resources;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI.RecentView;

internal class CrashCommands : ICrashInstance
{

	public class CrashCommand : Command
	{
		private string IconKey { get; }

		public string Name { get; }

		public bool Hover { get; set; } = false;

		public Color ColourOverride { get; set; } = Colors.Transparent;

		public CrashCommand(string name, string iconKey, Action action) : base((s, e) => action?.Invoke())
		{
			Name = name;
			MenuText = name;
			ToolTip = name;
			ToolBarText = name;
			IconKey = iconKey;
		}

		public Bitmap GetIcon(int size) => ColourOverride == Colors.Transparent ?
												CrashIcons.Icon(IconKey, size) :
												CrashIcons.Icon(IconKey, size, ColourOverride);

		public Bitmap GetIcon(int size, Color colour) => CrashIcons.Icon(IconKey, size, colour);

	}

	private RecentModelDialog Host { get; }
	public CrashCommands(RecentModelDialog host)
	{
		Host = host;

		Add = new("Add", "plus", () => Host.Model.AddSharedModel(new SharedModel() { ModelAddress = "Yep!" }));
		Join = new("Join", "join", () => { });
		ReloadAll = new("Reload All", "reload", () =>
		{
			foreach (var rmc in Host.Children.OfType<RecentModelControl>())
			{
				rmc.ViewModel.AttemptToConnect();
				rmc.Invalidate(true);
			}
			// Host.Model.SharedModels
		});
	}

	public CrashCommand Add { get; }
	public CrashCommand Join { get; }
	public CrashCommand Reload { get; } = new("Reload", "reload", () => { });
	public CrashCommand Remove { get; } = new("Remove", "close", () => { }) { ColourOverride = Palette.Red };
	public CrashCommand ReloadAll { get; }

}
