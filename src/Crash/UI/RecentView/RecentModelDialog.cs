using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;

using Crash.Handlers.Data;
using Crash.UI.JoinView;
using Crash.UI.RecentView;
using Crash.UI.RecentView.Layers;
using Crash.UI.UsersView;

using Eto.Drawing;
using Eto.Forms;

using Rhino.Runtime;

namespace Crash.UI;


/// <summary>
/// View for showing Recent Crash Models
/// Summoned by using <see cref="Crash.Commands.JoinSharedModel"/>
/// </summary>
internal sealed class RecentModelDialog : Dialog<ISharedModel>
{

	public static int PreviewWidth = 240;
	public static int PreviewPadding = 8;
	public static int PreviewHeight = 135;

	public static int WidthForPreviews(int count)
	{
		var width = (count * (PreviewWidth + PreviewPadding)) + PreviewPadding;
		// TODO : Check Current Screen Width
		return width;
	}

	private OverflowLayout<ISharedModel> RecentModelGallery { get; set; }
	internal Drawable ModelInputBar { get; private set; }
	private DynamicLayout StatusBar { get; set; }

	internal JoinViewModel Model => DataContext as JoinViewModel;

	internal CrashCommands CommandsInstance { get; }
	public RecentModelDialog()
	{
		WindowStyle = WindowStyle.Utility;
		Minimizable = false;
		Maximizable = false;

		AllowDrop = false;

		AbortButton = null;
		DefaultButton = null;

		Resizable = true;
		AutoSize = true;
		MinimumSize = new Size(WidthForPreviews(3), 400);
		Size = new Size(WidthForPreviews(5), 600);
		Padding = new Padding(0);

		DataContext = new JoinViewModel();

		CommandsInstance = new CrashCommands(this);
		InitLayout();
		InitBindings();

		this.StyleChanged += (s, e) =>
		{
			foreach (var label in this.Children.OfType<TextControl>())
			{
				label.TextColor = Palette.TextColour;
			}
		};
	}

	private void InitLayout()
	{
		var headerLabel = new Label()
		{
			Text = " Recent Models",
			TextColor = Palette.White,
			Font = SystemFonts.Default(24),
			Height = 32,
			TextAlignment = TextAlignment.Left,
		};

		var scrollable = new Scrollable()
		{
			Border = BorderType.None,

			AllowDrop = false,
			Content = InitRecentModelGallery(),
			ExpandContentHeight = true,
			ExpandContentWidth = true,
		};

		var recentModelLayout = new DynamicLayout()
		{
			Width = -1,
			Height = -1,
		};
		recentModelLayout.BeginVertical();

		recentModelLayout.AddRow(headerLabel);
		recentModelLayout.Add(scrollable, true, true);
		recentModelLayout.EndVertical();

		scrollable.MouseWheel += (s, e) =>
		{
			RecentModelGallery.Invalidate(true);
		};

		ModelInputBar = new AddressInputBar(Model);

		var pixelLayout = new PixelLayout()
		{
			Width = -1,
			Height = -1,
		};
		pixelLayout.Add(recentModelLayout, 0, 0);
		pixelLayout.Add(ModelInputBar, 0, 0);

		var layout = new DynamicLayout()
		{
			Spacing = new Size(0, 0),
			Padding = 0,
			Width = -1,
			Height = -1,
		};
		layout.BeginVertical();

		var drawableCanary = new Drawable();
		drawableCanary.Paint += (s, e) => this.OnStyleChanged(EventArgs.Empty);
		layout.Add(drawableCanary);
		layout.Add(pixelLayout, true, true);
		layout.AddRow(InitStatusBar());
		layout.EndVertical();

		Content = layout;
	}

	private DynamicLayout InitStatusBar()
	{
		StatusBar = new DynamicLayout()
		{
			Spacing = new Size(32, 0),
			BackgroundColor = HostUtils.RunningInDarkMode ? Palette.DarkGray : Palette.LightGray,
			Padding = 4,
		};
		StatusBar.BeginHorizontal();

		var copyrightLabel = new Label()
		{
			Text = "Crash Cloud",
			TextColor = Palette.TextColour,
			Font = SystemFonts.Default(12),
			TextAlignment = TextAlignment.Left
		};
		var versionLabel = new Label()
		{
			TextColor = Palette.White,
			Font = SystemFonts.Default(12),
			TextAlignment = TextAlignment.Left
		};
		versionLabel.BindDataContext(c => c.Text, Binding.Property((JoinViewModel m) => m.VersionText));

		StatusBar.Add(copyrightLabel, false);
		StatusBar.Add(versionLabel, false);

		StatusBar.AddSpace(true, false);

		var status1 = new Label()
		{
			Text = "AAA",
			TextColor = Palette.White,
			Font = SystemFonts.Default(12),
			TextAlignment = TextAlignment.Left
		};

		UpdateStatusOne(status1);

		var status2 = new Label()
		{
			Text = Environment.UserName,
			TextColor = Palette.White,
			Font = SystemFonts.Default(12),
			TextAlignment = TextAlignment.Left
		};


		var status3 = new Label()
		{
			Text = "</>",
			TextColor = Palette.White,
			Font = SystemFonts.Default(12),
			TextAlignment = TextAlignment.Left
		};

		StatusBar.Add(status1, false);
		StatusBar.Add(status2, false);
		StatusBar.Add(status3, false);

		StatusBar.EndHorizontal();

		this.StyleChanged += (s, e) =>
		{
			StatusBar.BackgroundColor = HostUtils.RunningInDarkMode ? Palette.DarkGray : Palette.LightGray;
		};

		return StatusBar;
	}

	private async Task UpdateStatusOne(Label status1)
	{
		Ping ping = new Ping();

		while (true)
		{
			await Task.Delay(2500);
			var reply = await ping.SendPingAsync("1.1.1.1");
			var status = reply?.Status ?? IPStatus.Unknown;
			status1.Text = status switch
			{
				IPStatus.Success => $"Ping: {reply?.RoundtripTime}ms",
				_ => $"No Connection"
			};
		}
	}

	private OverflowLayout<ISharedModel> InitRecentModelGallery()
	{
		// TODO : Dynamic Layouts are Shit
		// Use PixelLayouts
		RecentModelGallery = new OverflowLayout<ISharedModel>(Model.SharedModels, (model) => new RecentModelControl(this, model));
		RecentModelGallery.Focus();
		RecentModelGallery.Invalidate();

		return RecentModelGallery;
	}

	private void InitBindings()
	{

	}

}
