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

	private JoinViewModel Model => DataContext as JoinViewModel;

	public AddressInputDialog(Control parent)
	{
		Width = 600;
		Height = 180;
		Padding = 20;
		WindowStyle = WindowStyle.None;
		MovableByWindowBackground = false;
		Maximizable = false;
		Minimizable = false;

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
		LostFocus += ExitWithNoResult;
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

	private enum UriResult { Valid, File, Invalid, MissingHttp, InProgress, Duplicate };
	private UriResult GetResult()
	{
		try
		{
			var text = AddressInput.Text;
			var model = new SharedModel(text);
			if (Model.SharedModels.Any(m => m.Equals(model)))
				return UriResult.Duplicate;

			if ((!text.Contains("http://") || !text.Contains("https://")) && text.Contains("."))
			{
				return UriResult.MissingHttp;
			}

			var uri = new Uri(text);
			if (uri.IsFile) return UriResult.File;

			return UriResult.Valid;
		}
		catch
		{
			if (!AddressInput.Text.Contains(".")) return UriResult.InProgress;
		}
		return UriResult.Invalid;
	}

	private void ValidateText(object sender, EventArgs e)
	{
		var result = GetResult();
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

			_ => "URL is incomplete"
		};
		DoneButton.Enabled = result == UriResult.Valid;

		Invalidate(true);
	}

	private void ExitWithResult(object sender, EventArgs e)
	{
		if (!DoneButton.Enabled) return;
		Close(AddressInput.Text);
	}

	private void ExitWithNoResult(object sender, EventArgs e)
	{
		Close(string.Empty);
	}

	private void HandleKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Keys.Enter && DoneButton.Enabled)
			Close(AddressInput.Text);
		else if (e.Key == Keys.Escape)
			Close(string.Empty);
	}
}
