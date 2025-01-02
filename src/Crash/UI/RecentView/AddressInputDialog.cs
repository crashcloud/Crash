using System;
using System.Linq.Expressions;
using System.Reflection.Metadata;

using Crash.Handlers.Data;
using Crash.UI.JoinView;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI.RecentView;

public class AddressInputDialog : Dialog<string>
{

	private TextBox AddressInput { get; set; }
	private Label Message { get; set; }
	private Button DoneButton { get; set; }
	private Drawable Fade { get; set; }

	private RecentViewModel Model => DataContext as RecentViewModel;

	public AddressInputDialog(Control parent)
	{
		Width = 600;
		Height = 180;
		Padding = 20;
		MovableByWindowBackground = false;
		Maximizable = false;
		Minimizable = false;
		DataContext = parent.DataContext;

		InitLayout(parent);
		SetLocation(parent);
		InitBindings(parent);
	}

	private void InitLayout(Control parent)
	{
		AddressInput = new TextBox() { TextAlignment = TextAlignment.Center, Font = SystemFonts.Bold(24f), Height = 40 };
		Message = new Label() { Height = 20, TextAlignment = TextAlignment.Center };
		DoneButton = new Button() { Text = "Done", Enabled = false };

		var layout = new DynamicLayout();
		layout.BeginVertical();
		layout.Add(AddressInput, true, false);
		layout.AddSpace();
		layout.Add(Message, true, false);
		layout.AddSpace();
		layout.Add(DoneButton, true, false);
		layout.EndVertical();

		Content = layout;

		Fade = new Drawable() { Width = 10_000, Height = 10_000 };
		Fade.Paint += (s, e) => { e.Graphics.Clear(Palette.Shadow); };
		if (parent is PixelLayout pixel)
		{
			pixel.Add(Fade, 0, 0);
		}
	}

	private void SetLocation(Control parent)
	{
		Location = (Point)RectangleF.FromCenter(parent.Bounds.Center, new(Width, Height)).TopLeft;
	}

	private void InitBindings(Control parent)
	{
		DoneButton.Click += ExitWithResult;
		AddressInput.KeyDown += HandleKeyDown;
		KeyDown += HandleKeyDown;
		Shown += (s, e) => { AddressInput.Focus(); };
		parent.ParentWindow.LocationChanged += (s, e) =>
		{
			SetLocation(parent);
		};

		if (parent is PixelLayout pixel)
		{
			pixel.Add(Fade, 0, 0);
			Closing += (s, e) =>
			{
				pixel.Remove(Fade);
				Fade.Dispose();
			};
		}

		AddressInput.TextChanged += ValidateText;
		parent.StyleChanged += ValidateText;
	}

	private enum UriResult { Valid, File, Failure, MissingHttp, InProgress, Duplicate, Empty };
	private UriResult GetResult(out Exception exception)
	{
		exception = null;
		if (Model is null) return UriResult.Failure;
		try
		{
			var text = AddressInput.Text;
			if (string.IsNullOrEmpty(text)) return UriResult.Empty;

			var model = new SharedModel(text);
			if (model is null) return UriResult.Failure;

			if (text.Equals("h") || text.Equals("ht") ||
					text.Equals("htt") || text.Equals("http") ||
					text.Equals("http:") || text.Equals("http:/") ||
					text.Equals("http://"))
				return UriResult.InProgress;

			if ((!text.Contains("http://") && !text.Contains("https://"))) return UriResult.MissingHttp;

			var uri = new Uri(text);
			if (uri?.IsFile == true) return UriResult.File;

			if (!Uri.IsWellFormedUriString(text, UriKind.Absolute))
				return UriResult.Failure;

			if (Model.SharedModels.Any(m => SharedModel.Equals(m, model)))
				return UriResult.Duplicate;

			return UriResult.Valid;
		}
		catch (Exception ex)
		{
			exception = ex;
			if (!AddressInput.Text.Contains(".")) return UriResult.InProgress;
		}
		return UriResult.Failure;
	}

	private void ValidateText(object? sender, EventArgs e)
	{
		var result = GetResult(out var exception);
		AddressInput.TextColor = result switch
		{
			UriResult.Valid or UriResult.InProgress => Palette.TextColour,
			_ => Palette.Yellow,
		};
		Message.Text = result switch
		{
			UriResult.Valid or UriResult.InProgress => string.Empty,
			UriResult.MissingHttp => "Url requires a http:// or https://",
			UriResult.Duplicate => "Url has already been added",
			UriResult.File => "That's a file there pal...",

			_ => exception switch
			{
				UriFormatException or null => $"URL is incomplete {exception?.Message}",
				_ => exception?.Message,
			},
		};
		DoneButton.Enabled = result == UriResult.Valid;

		Invalidate(true);
	}

	private void ExitWithResult(object? sender, EventArgs e)
	{
		if (!DoneButton.Enabled) return;
		Close(AddressInput.Text);
	}

	private void ExitWithNoResult(object? sender, EventArgs e)
	{
		Close(string.Empty);
	}

	private void HandleKeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Keys.Enter)
			ExitWithResult(sender, e);

		else if (e.Key == Keys.Escape)
			ExitWithNoResult(sender, e);
	}
}
