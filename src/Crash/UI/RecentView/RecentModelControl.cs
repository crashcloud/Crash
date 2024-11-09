using System.Text.RegularExpressions;

using Crash.Handlers.Data;
using Crash.UI.JoinView;
using Crash.UI.RecentView.Layers;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI;

internal class RecentModelControl : Drawable
{

	private RecentModelViewModel ViewModel => DataContext as RecentModelViewModel;
	private RecentModelDialog HostView { get; }

	private int Frame { get; set; } = 0;
	private UITimer FrameTimer { get; }

	public RecentModelControl(RecentModelDialog crashRecentView, SharedModel model)
	{
		HostView = crashRecentView;
		DataContext = new RecentModelViewModel(model);
		Width = 240;
		Height = 135;
		BackgroundColor = Colors.Crimson;

		ViewModel.PropertyChanged += (s, e) => Invalidate(true);

		HostView.Model.ListenToProperty(nameof(JoinViewModel.TemporaryModel), () =>
		{
			HostView.Invalidate(true);
		});

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
		HostView.Shown += async (s, e) =>
		{
			try
			{
				await ViewModel.AttemptToConnect();
			}
			catch (Exception ex)
			{
				;
			}
		};
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates

		if (ViewModel.State != ModelRenderState.Add)
		{
			var previousState = ViewModel.State;
			FrameTimer = new UITimer
			{
				Interval = 0.1,
			};
			FrameTimer.Elapsed += (s, _) =>
			{
				if (previousState == ViewModel.State)
				{
					if (ViewModel.State == ModelRenderState.FailedToLoad) return;
					if (ViewModel.State == ModelRenderState.Loaded) return;
				}
				else
				{
					previousState = ViewModel.State;
				}

				Frame++;
				Invalidate();
			};
			FrameTimer.Start();
		}

		ToolTip = "Double Click to Join. Right Click for Options.";
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		e.Handled = true;
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

	protected override void OnMouseDown(MouseEventArgs e)
	{
		e.Handled = true;
		if (HostView.Model.TemporaryModel is not null) return;
		if (e.Buttons == MouseButtons.Primary)
		{
			if (!ViewModel.State.HasFlag(ModelRenderState.Selected))
			{
				ViewModel.State = ViewModel.State |= ModelRenderState.Selected;
			}
			else
			{
				ViewModel.State = ViewModel.State &= ~ModelRenderState.Selected;
			}

			Invalidate(true);
		}

		base.OnMouseUp(e);
	}

	private bool MouseOver { get; set; }
	protected override void OnMouseEnter(MouseEventArgs e)
	{
		e.Handled = true;
		base.OnMouseMove(e);
		Invalidate(true);
		MouseOver = true;
	}
	protected override void OnMouseLeave(MouseEventArgs e)
	{
		e.Handled = true;
		base.OnMouseLeave(e);
		Invalidate(true);
		MouseOver = false;
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		e.Handled = true;
		if (e.Buttons == MouseButtons.Primary &&
			ViewModel.State == ModelRenderState.Loaded)
		{
			HostView.Close(ViewModel.Model);
		}

		base.OnMouseDoubleClick(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (ViewModel.State.HasFlag(ModelRenderState.Add))
		{
			RenderAdd(e);
		}
		else
		{
			if (ViewModel.State.HasFlag(ModelRenderState.Loading))
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

			if (ViewModel.State.HasFlag(ModelRenderState.Selected) && !ViewModel.State.HasFlag(ModelRenderState.Add))
			{
				e.Graphics.DrawRectangle(new Pen(Palette.Yellow, 4f), Bounds);
			}

			if (ViewModel.State.HasFlag(ModelRenderState.RightClick))
			{
				RenderRightClick(e);
			}
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

		// TODO : Draw a cicle and move the line pattern instead
		e.Graphics.DrawArc(new Pen(Palette.White, 4f), circleRect, startArc, endArc);

		RenderAddressBar(e);
	}

	private void RenderAddressBar(PaintEventArgs e)
	{
		var rect = e.ClipRectangle;
		var bar = new RectangleF(rect.Left, rect.Bottom - 30f, rect.Width, 30f);
		e.Graphics.FillRectangle(Palette.White, bar);

		PillLayer.RenderAddress(e, ViewModel.Model.ModelAddress, bar);
	}

	private void RenderAdd(PaintEventArgs e)
	{
		float inset = 4f;
		float radius = 6f;
		var rect = new RectangleF(inset, inset, Size.Width - (inset * 2), Size.Height - (inset * 2));
		var path = GraphicsPath.GetRoundRect(rect, radius);

		e.Graphics.FillPath(Palette.GetHashedTexture(6, 0.1f), path);
		e.Graphics.DrawPath(Palette.GetDashedPen(Color.FromArgb(19, 27, 35)), path);

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

	public override string ToString() => $"{this.ViewModel.Model.ModelAddress}";

}
