using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;

using Crash.Handlers.Data;
using Crash.UI.JoinView;
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
	private PixelLayout RecentModelContainer { get; set; }
	private DynamicLayout RecentModelGallery { get; set; }
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

		Resizable = false;
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
			ExpandContentHeight = false,
			ExpandContentWidth = true,
			Height = 500,
		};

		var recentModelLayout = new DynamicLayout()
		{
			Spacing = new Size(16, 16),
			Padding = new Padding(16, 16),
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

		ModelInputBar = GetModelInputBar();

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
			// TODO : Make sure this only appears when clicking the empty area
			return;
			if (e.Buttons != MouseButtons.Alternate) return;
			EmptySpaceRightClick.Visible = !EmptySpaceRightClick.Visible;
			EmptySpaceRightClick.Tag = e.Location;
			EmptySpaceRightClick.Invalidate(true);
		};

		var layout = new DynamicLayout()
		{
			Spacing = new Size(0, 0),
			Padding = 0,
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

	private DynamicLayout InitRecentModelGallery()
	{
		RecentModelGallery = new DynamicLayout()
		{
			Spacing = new Size(16, 16),
			Width = -1,
			Height = -1,
			MinimumSize = new Size(800, 500),
		};
		RecentModelGallery.Focus();

		return RecentModelGallery;
	}

	private Drawable GetModelInputBar()
	{
		Drawable? inputBar = new Drawable()
		{
			CanFocus = true,
			AllowDrop = false,
		};

		inputBar.Paint += (s, e) =>
		{
			e.Graphics.Clear(Colors.Transparent);
			// Draw nothing until we need to add!
			// Temporary?
			if (Model.TemporaryModel is null) return;

			inputBar.Width = inputBar.Parent.Width;
			inputBar.Height = inputBar.Parent.Height;

			var brush = new SolidBrush(Color.FromArgb(160, 160, 160, 160));
			e.Graphics.FillRectangle(Palette.GetHashedTexture(6, 0.75f), e.ClipRectangle);

			var bar = RectangleF.FromCenter(inputBar.Parent.Bounds.Center, new SizeF(600, 80));
			bar.Y -= 60f;
			var b = new SolidBrush(Palette.White);
			e.Graphics.FillRectangle(b, bar);

			// var font = SystemFonts.Default(24f);
			// e.Graphics.DrawText(font, new SolidBrush(Palette.Black), bar, Model.TemporaryModel.ModelAddress, alignment: FormattedTextAlignment.Center);
			PillLayer.RenderAddress(e, Model.TemporaryModel.ModelAddress, bar, true);
		};

		inputBar.MouseDown += (s, e) =>
		{
			if (Model.TemporaryModel is null) return;
			inputBar.Invalidate(true);
		};

		inputBar.KeyDown += (s, e) =>
		{
			if (Model.TemporaryModel is null) return;

			string c = string.Empty;
			if (e.Key == Keys.Backspace || e.Key == Keys.Delete)
			{
				if (Model.TemporaryModel.ModelAddress.Length > 0)
				{
					Model.TemporaryModel.ModelAddress = Model.TemporaryModel.ModelAddress.Substring(0, Model.TemporaryModel.ModelAddress.Length - 1);
				}
			}
			else if (e.Key >= Keys.A && e.Key <= Keys.Z)
			{
				c = e.Key.ToShortcutString("");
				Model.TemporaryModel.ModelAddress += c;
			}
			else if (e.Key >= Keys.D0 && e.Key <= Keys.D9)
			{
				c = e.Key.ToShortcutString("");
				Model.TemporaryModel.ModelAddress += c;
			}
			else if (e.Key == Keys.Semicolon)
			{
				Model.TemporaryModel.ModelAddress += ":";
			}
			else if (e.Key == Keys.Period)
			{
				Model.TemporaryModel.ModelAddress += ".";
			}
			else if (e.Key == Keys.Slash)
			{
				Model.TemporaryModel.ModelAddress += "/";
			}
			else if (e.Key == Keys.Escape)
			{
				Model.TemporaryModel = null;
				inputBar.Invalidate(true);
				e.Handled = true;
				return;
			}
			else if (e.Key == Keys.Enter)
			{
				if (string.IsNullOrEmpty(Model?.TemporaryModel?.ModelAddress))
					return;

				// Attempt to connect
				if (Model.AddSharedModel(Model.TemporaryModel))
				{
					Model.NotifyPropertyChanged(nameof(Model.SharedModels));
					InitBindings();

					// var controls = ConvertModelsToControls(Model.SharedModels).ToList();
				}
				Model.TemporaryModel = null;
				inputBar.Invalidate(true);
				Invalidate(true);

				e.Handled = true;
				return;
			}
			else
			{
				e.Handled = true;
				return;
			}

			e.Handled = true;
			Model.TemporaryModel.ModelAddress = Model.TemporaryModel.ModelAddress.ToLowerInvariant();

			inputBar.Invalidate(true);
		};

		Model.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(JoinViewModel.TemporaryModel))
			{
				inputBar.Invalidate();
			}
		};

		return inputBar;
	}

	private void InitBindings()
	{
		RecentModelGallery.Clear();
		RecentModelGallery.BeginVertical();

		var controls = ConvertModelsToControls(Model.SharedModels).ToList();

		for (int i = 0; i < controls.Count; i += 5)
		{
			var stackLayout = new StackLayout()
			{
				VerticalContentAlignment = VerticalAlignment.Center,
				Spacing = 16,
				HorizontalContentAlignment = HorizontalAlignment.Left,
				Orientation = Orientation.Horizontal,
				Padding = new Padding(0, 8),
			};
			for (int j = 0; j < 5; j++)
			{
				int index = i + j;
				if (index >= controls.Count) break;
				stackLayout.Items.Add(controls[index]);
			}

			RecentModelGallery.Add(stackLayout, true, false);
		}

		// RecentModelGallery.Height = (int)(controls.Count / 5.0) * 200;
		RecentModelGallery.AddSpace(true, true);
		RecentModelGallery.EndVertical();
	}

	private IEnumerable<Control>? ConvertModelsToControls(ObservableCollection<SharedModel> models)
	{
		var controls = new List<Control>();

		// [+]
		controls.Add(new RecentModelControl(this, null));

		// We should likely sort by, or allow for sort-by
		foreach (var model in models)
		{
			var control = new RecentModelControl(this, model);
			controls.Add(control);
		}

		return controls;
	}

}
