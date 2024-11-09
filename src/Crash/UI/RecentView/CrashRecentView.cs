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

namespace Crash.UI;


/// <summary>
/// View for showing Recent Crash Models
/// Summoned by using <see cref="Crash.Commands.JoinSharedModel"/>
/// </summary>
internal sealed class CrashRecentView : Dialog<SharedModel>
{
	private OverflowLayout<SharedModel> RecentModelGallery { get; set; }
	private Drawable EmptySpaceRightClick { get; set; }
	internal Drawable ModelInputBar { get; private set; }
	private DynamicLayout StatusBar { get; set; }

	internal JoinViewModel Model => DataContext as JoinViewModel;

	public CrashRecentView()
	{
		WindowStyle = WindowStyle.Utility;
		Minimizable = false;
		Maximizable = false;

		AllowDrop = false;

		AbortButton = null;
		DefaultButton = null;

		Resizable = true;
		AutoSize = true;
		MinimumSize = new Size(800, 400);
		Size = new Size(1296, 600);
		Padding = new Padding(0);

		DataContext = new JoinViewModel();

		InitLayout();
		InitBindings();
	}

	private void InitLayout()
	{
		var headerLabel = new Label()
		{
			Text = "Recent Models",
			TextColor = Palette.White,
			Font = SystemFonts.Default(24),
			Height = 32,
			TextAlignment = TextAlignment.Left,
		};

		var scrollable = new Scrollable()
		{
			Border = BorderType.None,
			BackgroundColor = Colors.Transparent,
			AllowDrop = false,
			Content = InitRecentModelGallery(),
			ExpandContentHeight = true,
			ExpandContentWidth = true,
			Height = -1,
			Width = -1,
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

		EmptySpaceRightClick = GetEmptySpaceRightClick();

		ModelInputBar = new AddressInputBar(Model);

		var pixelLayout = new PixelLayout()
		{
			Width = -1,
			Height = -1,
		};
		pixelLayout.Add(recentModelLayout, 0, 0);
		pixelLayout.Add(EmptySpaceRightClick, 0, 0);
		pixelLayout.Add(ModelInputBar, 0, 0);

		pixelLayout.MouseDown += (s, e) =>
		{
			if (e.Buttons != MouseButtons.Alternate) return;
			EmptySpaceRightClick.Visible = !EmptySpaceRightClick.Visible;
			EmptySpaceRightClick.Tag = e.Location;
			EmptySpaceRightClick.Invalidate(true);
		};

		var layout = new DynamicLayout()
		{
			Spacing = new Size(0, 0),
			Padding = 0,
			Width = -1,
			Height = -1,
		};
		layout.BeginVertical();

		layout.Add(pixelLayout, true, true);
		layout.AddRow(InitStatusBar());
		layout.EndVertical();

		Content = layout;
	}

	private Drawable GetEmptySpaceRightClick()
	{
		var menu = new Drawable()
		{
			Visible = false,
			Tag = new PointF(0, 0)
		};
		menu.Paint += (s, e) =>
		{
			if (menu.Parent is null) return;

			menu.Width = menu.Parent.Width;
			menu.Height = menu.Parent.Height;

			if (menu.Tag is not PointF location) return;

			e.Graphics.TranslateTransform(location.X, location.Y);

			e.Graphics.FillRectangle(Palette.White, new RectangleF(0, 0, 120, 240));
		};

		return menu;
	}

	private DynamicLayout InitStatusBar()
	{
		StatusBar = new DynamicLayout()
		{
			Spacing = new Size(32, 0),
			BackgroundColor = Color.FromArgb(36, 36, 36),
			Padding = 4,
		};
		StatusBar.BeginHorizontal();

		var copyrightLabel = new Label()
		{
			Text = "Copyright Crash Cloud 2021 - 2024",
			TextColor = Palette.White,
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

	private OverflowLayout<SharedModel> InitRecentModelGallery()
	{
		RecentModelGallery = new OverflowLayout<SharedModel>(240, Model.SharedModels, (model) => new RecentModelControl(this, model))
		{
			Spacing = new Size(16, 16),
			Width = -1,
			Height = -1,
			Padding = new Padding(16),
			MinimumSize = new Size(800, 500),
		};
		RecentModelGallery.Focus();

		RecentModelGallery.Clear();
		var width = this.GetPreferredSize().Width;
		int HorizontalControlCount = (int)(width / (RecentModelGallery.ControlWidth)); // + Spacing.Value.Width));

		RecentModelGallery.BeginVertical();

		for (int i = 0; i < RecentModelGallery.DataControls.Count; i++)
		{
			int controlWidth = 0;
			var stackLayout = new DynamicLayout()
			{
				Padding = new Padding(0, 4),
				Width = -1,
				Spacing = new Size(8, 0),
			};

			stackLayout.BeginHorizontal();
			while (controlWidth < width)
			{
				if (i >= RecentModelGallery.DataStore.Count)
					break;

				var control = RecentModelGallery.DataControls[i];
				if (control.Width + controlWidth > width)
					break;

				stackLayout.Add(control, false, false);
				stackLayout.Height = control.Height;
				controlWidth += control.Width;
				i++;
			}
			stackLayout.AddSpace(true, false);
			stackLayout.EndHorizontal();

			RecentModelGallery.Add(stackLayout, true, false);
		}

		RecentModelGallery.AddSpace(true, true);

		RecentModelGallery.EndVertical();
		RecentModelGallery.Invalidate();

		return RecentModelGallery;
	}

	private void InitBindings()
	{

	}

}
