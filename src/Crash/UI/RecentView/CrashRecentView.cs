using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
	private PixelLayout RecentModelGallery { get; set; }
	private DynamicLayout StatusBar { get; set; }

	private JoinViewModel Model => DataContext as JoinViewModel;

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
			TextAlignment = TextAlignment.Left
		};

		var layout = new DynamicLayout();
		layout.BeginVertical();
		layout.AddRow(headerLabel);
		layout.Add(InitRecentModelGallery(), true, true);
		layout.AddRow(InitStatusBar());
		layout.EndVertical();

		Content = layout;
	}

	private DynamicLayout InitStatusBar()
	{
		StatusBar = new DynamicLayout()
		{
			Spacing = new Size(32, 0)
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
			Text = "Version v.1.5.0 - wip",
			TextColor = Colors.DarkGray,
			Font = SystemFonts.Default(12),
			TextAlignment = TextAlignment.Left
		};

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

	private PixelLayout InitRecentModelGallery()
	{
		PixelLayout layout = new PixelLayout();
		var recentModels = new StackLayout()
		{
			Orientation = Orientation.Horizontal,
			Spacing = 16,
			Padding = new Padding(16),
			BackgroundColor = Colors.White
		};

		var modelInputBar = GetModelInputBar();

		layout.Add(recentModels, 0, 0);
		layout.Add(modelInputBar, 0, 0);

		return RecentModelGallery = layout;
	}

	private Drawable GetModelInputBar()
	{
		Drawable? inputBar = new Drawable();
		inputBar.Paint += (s, e) =>
		{
			// Draw nothing until we need to add!
			// Temporary?
			if (Model.TemporaryModel is null) return;

			// Draw Input!

			var brush = new SolidBrush(Color.FromArgb(120, 120, 120, 120));
			e.Graphics.FillRectangle(brush, e.ClipRectangle);
		};

		return inputBar;
	}

	private void InitBindings()
	{
		RecentModelGallery.BindDataContext(c => c.Controls, Binding.Property((JoinViewModel m) => m.SharedModels).Convert((models) => ConvetModelsToControls(models)));
	}

	private IEnumerable<Control>? ConvetModelsToControls(ObservableCollection<SharedModel> models)
	{
		var controls = new List<Control>();
		foreach (var model in models)
		{
			var control = new RecentModelControl(model);
			controls.Add(control);
		}

		// [+] Control
		controls.Add(new RecentModelControl(null));

		return controls;
	}

}
