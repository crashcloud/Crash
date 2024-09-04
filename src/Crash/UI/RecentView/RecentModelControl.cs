using System.Text.RegularExpressions;

using Crash.Handlers.Data;
using Crash.UI.JoinView;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI;

internal class RecentModelControl : Drawable
{

	private RecentModelViewModel ViewModel => DataContext as RecentModelViewModel;
	private CrashRecentView HostView { get; }

	private int Frame { get; set; } = 0;
	private UITimer FrameTimer { get; }

	public RecentModelControl(CrashRecentView crashRecentView, SharedModel model)
	{
		HostView = crashRecentView;
		DataContext = new RecentModelViewModel(model);
		Size = new Eto.Drawing.Size(240, 135);

		ViewModel.PropertyChanged += (s, e) => Invalidate(true);

		HostView.Model.ListenToProperty(nameof(JoinViewModel.TemporaryModel), () =>
		{
			HostView.Invalidate(true);
		});

		HostView.Shown += async (s, e) =>
		{
			await ViewModel.AttemptToConnect();
		};

		FrameTimer = new UITimer
		{
			Interval = 0.1,
		};
		FrameTimer.Elapsed += (s, _) =>
		{
			Frame++;
			Invalidate();
		};
		FrameTimer.Start();

		ToolTip = "Double Click to Join. Right Click for Options.";
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (e.Buttons == MouseButtons.Alternate && !ViewModel.State.HasFlag(ModelRenderState.Add))
		{
			if (!ViewModel.State.HasFlag(ModelRenderState.RightClick))
			{
				ViewModel.State = ViewModel.State |= ModelRenderState.RightClick;
				Invalidate();
			}
			else
			{
				ViewModel.State = ViewModel.State &= ~ModelRenderState.RightClick;
				Invalidate();
			}
		}

		if (e.Buttons == MouseButtons.Primary && ViewModel.State == ModelRenderState.Loaded)
		{
			// Join
			// HostView.Close(ViewModel.Model);

			// Remove
			// ...

			// Refresh
			// ...
		}

		if (e.Buttons == MouseButtons.Primary && ViewModel.State == ModelRenderState.Add)
		{
			HostView.Model.TemporaryModel = new SharedModel();
			HostView.ModelInputBar.Invalidate();
		}

		base.OnMouseUp(e);
	}

	private bool MouseOver { get; set; }
	protected override void OnMouseEnter(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		Invalidate(true);
		MouseOver = true;
	}
	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		Invalidate(true);
		MouseOver = false;
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		if (e.Buttons == MouseButtons.Primary &&
			ViewModel.State == ModelRenderState.Loaded)
		{
			HostView.Close(ViewModel.Model);
		}

		base.OnMouseDoubleClick(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SaveTransform();

		if (!MouseOver)
		{
			e.Graphics.TranslateTransform(12f, 6f);
			e.Graphics.ScaleTransform(0.9f, 0.9f);
		}

		if (ViewModel.State.HasFlag(ModelRenderState.Add))
		{
			RenderAdd(e);
		}
		else if (ViewModel.State.HasFlag(ModelRenderState.Loading))
		{
			RenderLoading(e);
		}
		else if (ViewModel.State.HasFlag(ModelRenderState.Loaded))
		{
			RenderLoaded(e);
		}
		else if (ViewModel.State.HasFlag(ModelRenderState.FailedToLoad))
		{
			RenderFailedToLoad(e);
		}

		e.Graphics.RestoreTransform();
		if (ViewModel.State.HasFlag(ModelRenderState.RightClick))
		{
			RenderRightClick(e);
		}

		base.OnPaint(e);
	}

	private void RenderRightClick(PaintEventArgs e)
	{
		// var overlay = Color.FromArgb(40, 40, 40, 120);
		var overlay = Palette.GetHashedTexture(6, 0.75f);
		e.Graphics.FillRectangle(overlay, e.ClipRectangle);

		float inset = 20f;
		var rect = new RectangleF(inset, inset, Size.Width - (inset * 2), Size.Height - (inset * 2));
		e.Graphics.FillRectangle(Palette.White, rect);

		string[] options = new[] { "Refresh", "Delete", "Join" };
		for (int i = 0; i < 3; i++)
		{
			float y = rect.Height / 3f * i;
			e.Graphics.DrawLine(Color.FromArgb(40, 40, 40, 40), rect.Left + 4f, rect.Top + y, rect.Right - 4f, rect.Top + y);

			var color = Palette.Black;
			if (i == 2 && !ViewModel.State.HasFlag(ModelRenderState.Loaded))
			{
				// Draw faded out
				color = Palette.Gray;
			}

			e.Graphics.DrawText(SystemFonts.Default(14f), new SolidBrush(color), new RectangleF(rect.Left + 4f, rect.Top + y + 6f, rect.Width - 8f, rect.Height / 3f), options[i], alignment: FormattedTextAlignment.Center);
		}
	}

	private void RenderFailedToLoad(PaintEventArgs e)
	{
		e.Graphics.FillRectangle(Palette.Red, e.ClipRectangle);
		e.Graphics.SaveTransform();
		e.Graphics.TranslateTransform(e.ClipRectangle.Center);

		var crossRect = RectangleF.FromCenter(new PointF(0, 0), new SizeF(30f, 8f));
		var crossRect90 = RectangleF.FromCenter(new PointF(0, 0), new SizeF(8f, 30f));

		e.Graphics.RotateTransform(45f);

		e.Graphics.FillRectangle(Palette.White, crossRect);
		e.Graphics.FillRectangle(Palette.White, crossRect90);

		e.Graphics.RestoreTransform();
		RenderAddressBar(e);
	}

	private void RenderLoaded(PaintEventArgs e)
	{
		e.Graphics.FillRectangle(Palette.Green, e.ClipRectangle);
		if (ViewModel.Model is null) return;
		if (ViewModel.Model.Thumbnail is null) return;

		e.Graphics.DrawImage(ViewModel.Model.Thumbnail, 0, -((ViewModel.Model.Thumbnail.Height - (e.ClipRectangle.Height - 28f)) / 2f));
		RenderAddressBar(e);
	}


	private void RenderLoading(PaintEventArgs e)
	{
		e.Graphics.FillRectangle(Palette.Blue, e.ClipRectangle);

		var circleRect = RectangleF.FromCenter(e.ClipRectangle.Center, new SizeF(30f, 30f));
		circleRect.Y -= 14f;

		int startArc = 0 + (Frame * 4);
		int endArc = 120 + (Frame * 6);

		if (startArc > 360)
			startArc = 0;

		if (endArc > 360)
			endArc = 120;

		e.Graphics.DrawArc(new Pen(Palette.White, 4f), circleRect, startArc, endArc);

		RenderAddressBar(e);
	}

	private void RenderAddressBar(PaintEventArgs e)
	{
		var rect = e.ClipRectangle;
		var bar = new RectangleF(rect.Left, rect.Bottom - 30f, rect.Width, 30f);
		e.Graphics.FillRectangle(Palette.White, bar);

		var font = SystemFonts.Default(20f);
		var brush = new SolidBrush(Palette.Black);
		// e.Graphics.DrawText(font, brush, bar, ViewModel.Model.ModelAddress, alignment: FormattedTextAlignment.Center);
		RenderAddress(e, ViewModel.Model.ModelAddress);
	}

	private const string httpRegex = "(http|https)://";
	private const string portRegex = ":[0-9]+";
	private const string pathRegex = "/[^/]+";
	internal static readonly string[] separator = new[] { "." };

	//private const string extensionRegex = "\\.[a-zA-Z]+";

	public static void RenderAddress(PaintEventArgs args, string text)
	{
		if (string.IsNullOrEmpty(text)) return;

		var httpText = Regex.Match(text, httpRegex).Value;
		// var pathText = Regex.Match(text, pathRegex).Value;
		var portText = Regex.Match(text, portRegex).Value;
		var wwwText = Regex.Match(text, "www\\.").Value;

		var urlBits = text;
		if (!string.IsNullOrEmpty(httpText))
			urlBits = text.Replace(httpText, string.Empty);

		// if (!string.IsNullOrEmpty(pathText))
		//	urlBits = urlBits.Replace(pathText, string.Empty);

		if (!string.IsNullOrEmpty(portText))
			urlBits = urlBits.Replace(portText, string.Empty);

		if (!string.IsNullOrEmpty(wwwText))
			urlBits = urlBits.Replace(wwwText, string.Empty);

		if (string.IsNullOrEmpty(urlBits)) return;

		var parts = urlBits.Split(separator, StringSplitOptions.RemoveEmptyEntries)
		.Where(p => !string.IsNullOrEmpty(p))
		.Select(p => $".{p}")
		.ToList();
		parts[0] = parts[0].Substring(1);

		if (!string.IsNullOrEmpty(httpText))
			parts.Insert(0, httpText);

		if (!string.IsNullOrEmpty(portText))
			parts.Add(portText);

		// if (!string.IsNullOrEmpty(pathText))
		// 	parts.Add(pathText);

		foreach (var part in parts)
		{
			args.Graphics.TranslateTransform(4f, 0);

			var font = SystemFonts.Default(14f);
			var brush = new SolidBrush(Palette.Black);

			var size = args.Graphics.MeasureString(font, part);

			var pillSize = new RectangleF(4f, args.ClipRectangle.Height - 26f, size.Width + 12f, 22f);

			var roundedRect = GraphicsPath.GetRoundRect(pillSize, 6f);
			args.Graphics.FillPath(Palette.Yellow, roundedRect);
			args.Graphics.DrawText(font, brush, pillSize, part, alignment: FormattedTextAlignment.Center);

			args.Graphics.TranslateTransform(pillSize.Width, 0);
		}
	}

	private void RenderAdd(PaintEventArgs e)
	{
		float inset = 4f;
		float radius = 6f;
		var rect = new RectangleF(inset, inset, Size.Width - (inset * 2), Size.Height - (inset * 2));
		var path = GraphicsPath.GetRoundRect(rect, radius);

		e.Graphics.FillPath(Palette.GetHashedTexture(6, 0.1f), path);

		var pen = new Pen(Color.FromArgb(19, 27, 35), 4f);
		pen.DashStyle = new DashStyle(4, 4);
		e.Graphics.DrawPath(pen, path);

		float centerBox = 40f;
		var plusBox = RectangleF.FromCenter(rect.Center, new SizeF(centerBox, centerBox));
		var centerBoxPath = GraphicsPath.GetRoundRect(plusBox, radius);

		var color = Palette.Purple;

		e.Graphics.FillPath(color, centerBoxPath);

		var plusPathVert = RectangleF.FromCenter(rect.Center, new SizeF(20f, 5f));
		var plusPathHoz = RectangleF.FromCenter(rect.Center, new SizeF(5f, 20f));

		e.Graphics.FillRectangle(Palette.White, plusPathVert);
		e.Graphics.FillRectangle(Palette.White, plusPathHoz);
	}

}
