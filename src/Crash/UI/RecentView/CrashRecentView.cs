using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Handlers.Data;
using Crash.UI.JoinView;
using Crash.UI.UsersView;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI;

public partial class CrashRecentView : Dialog<SharedModel>
{
	private PixelLayout RecentModelContainer { get; set; }
	private StackLayout RecentModelGallery { get; set; }
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
		Size = new Size(1200, 600);
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
			TextColor = Colors.DarkGray,
			Font = SystemFonts.Default(24),
			Height = 32,
			TextAlignment = TextAlignment.Left,
		};

		var recentModelLayout = new DynamicLayout()
		{
			Spacing = new Size(12, 12),
			Padding = new Padding(12, 4),
		};
		recentModelLayout.BeginVertical();

		recentModelLayout.AddRow(headerLabel);
		recentModelLayout.Add(InitRecentModelGallery(), true, true);
		recentModelLayout.EndVertical();

		ModelInputBar = GetModelInputBar();

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
		};
		layout.BeginVertical();

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
			BackgroundColor = Color.FromArgb(36, 36, 36),
			Padding = 4,
		};
		StatusBar.BeginHorizontal();

		var copyrightLabel = new Label()
		{
			Text = "Copyright Crash Cloud 2021 - 2024",
			TextColor = Colors.DarkGray,
			Font = SystemFonts.Default(12),
			TextAlignment = TextAlignment.Left
		};
		var versionLabel = new Label()
		{
			TextColor = Colors.DarkGray,
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
			TextColor = Colors.DarkGray,
			Font = SystemFonts.Default(12),
			TextAlignment = TextAlignment.Left
		};

		var status2 = new Label()
		{
			Text = "BBB",
			TextColor = Colors.DarkGray,
			Font = SystemFonts.Default(12),
			TextAlignment = TextAlignment.Left
		};


		var status3 = new Label()
		{
			Text = "CCC",
			TextColor = Colors.DarkGray,
			Font = SystemFonts.Default(12),
			TextAlignment = TextAlignment.Left
		};

		StatusBar.Add(status1, false);
		StatusBar.Add(status2, false);
		StatusBar.Add(status3, false);

		StatusBar.EndHorizontal();

		return StatusBar;
	}

	private StackLayout InitRecentModelGallery()
	{
		RecentModelGallery = new StackLayout()
		{
			Orientation = Orientation.Horizontal,
			Spacing = 16,
			Width = -1,
			Height = -1,
			VerticalContentAlignment = VerticalAlignment.Top,
			HorizontalContentAlignment = HorizontalAlignment.Left
		};

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

			var brush = new SolidBrush(Color.FromArgb(120, 120, 120, 120));
			e.Graphics.FillRectangle(Palette.GetHashedTexture(6), e.ClipRectangle);

			var bar = RectangleF.FromCenter(inputBar.Parent.Bounds.Center, new SizeF(600, 80));
			bar.Y -= 60f;
			var b = new SolidBrush(Colors.White);
			e.Graphics.FillRectangle(b, bar);

			// var font = SystemFonts.Default(24f);
			// e.Graphics.DrawText(font, new SolidBrush(Colors.Black), bar, Model.TemporaryModel.ModelAddress, alignment: FormattedTextAlignment.Center);
			e.Graphics.TranslateTransform(300, -300);
			RecentModelControl.RenderAddress(e, Model.TemporaryModel.ModelAddress);
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
				Model.AddSharedModel(Model.TemporaryModel);
				Model.TemporaryModel = null;
				inputBar.Invalidate(true);
				Invalidate(true);
				Model.NotifyPropertyChanged(nameof(Model.SharedModels));
				InitBindings();
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
		RecentModelGallery.Items.Clear();
		// RecentModelGallery.BindDataContext(c => c.Controls, Binding.Property((JoinViewModel m) => m.SharedModels).Convert((models) => ConvetModelsToControls(models)));
		var controls = ConvertModelsToControls(Model.SharedModels);
		foreach (var control in controls)
		{
			RecentModelGallery.Items.Add(control);
		}
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

		controls.Add(new RecentModelControl(this, new SharedModel() { ModelAddress = "https://www.google.com" }));
		controls.Add(new RecentModelControl(this, new SharedModel() { ModelAddress = "https://www.text.com" }));
		// controls.Add(new RecentModelControl(this, new SharedModel() { ModelAddress = "https://www.words.com" }));
		// controls.Add(new RecentModelControl(this, new SharedModel() { ModelAddress = "https://www.yes.com" }));

		return controls;
	}

}
