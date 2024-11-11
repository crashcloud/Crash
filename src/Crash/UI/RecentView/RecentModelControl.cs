using System.Text.RegularExpressions;

using Crash.Handlers.Data;
using Crash.Resources;
using Crash.UI.JoinView;
using Crash.UI.RecentView;
using Crash.UI.RecentView.Layers;

using Eto.Drawing;
using Eto.Forms;

using Rhino.Runtime;

namespace Crash.UI;

internal class RecentModelControl : Drawable
{

	internal RecentModelViewModel ViewModel => DataContext as RecentModelViewModel;
	private RecentModelDialog HostView { get; }
	private bool Disabled
	{
		get
		{
			var parent = (Parent as OverflowLayout<ISharedModel>);
			var menu = parent?.RightClickMenu;
			return menu?.Visible ?? false;
		}
	}

	private int Frame { get; set; } = 0;
	private UITimer FrameTimer { get; }

	private RectangleF MaximumRectangle => new RectangleF(0, 0, Width, Height);

	public RecentModelControl(RecentModelDialog crashRecentView, ISharedModel model)
	{
		HostView = crashRecentView;
		DataContext = new RecentModelViewModel(model);
		Width = RecentModelDialog.PreviewWidth;
		Height = RecentModelDialog.PreviewHeight;

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

		if (e.Buttons == MouseButtons.Primary)
		{
			if (ViewModel.Model is AddModel)
				HostView.CommandsInstance.Add?.Execute();
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

		base.OnMouseDown(e);
	}

	protected override void OnMouseEnter(MouseEventArgs e)
	{
		if (Disabled)
		{
			base.OnMouseEnter(e);
			return;
		}
		ViewModel.State |= ModelRenderState.MouseOver;
		e.Handled = true;
		base.OnMouseEnter(e);
		Invalidate(true);
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		if (Disabled)
		{
			base.OnMouseLeave(e);
			return;
		}
		e.Handled = true;
		ViewModel.State &= ~ModelRenderState.MouseOver;
		Invalidate(true);
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		e.Handled = true;
		if (e.Buttons == MouseButtons.Primary)
		{
			if (ViewModel.State.HasFlag(ModelRenderState.Loaded))
				HostView.Close(ViewModel.Model);
		}

		base.OnMouseDoubleClick(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		RenderBase(e);

		if (ViewModel.State.HasFlag(ModelRenderState.Add))
		{
			RenderAdd(e);
		}
		else if (ViewModel.State.HasFlag(ModelRenderState.Sandbox))
		{
			RenderSandbox(e);
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
		}

		base.OnPaint(e);
	}

	#region Base

	private const float Inset = 10f;
	private const float Radius = 6f;
	private IGraphicsPath BoundingPath => GraphicsPath.GetRoundRect(MaximumRectangle, Radius);
	private RectangleF PreviewRectNoAddress => new RectangleF(Inset, Inset, MaximumRectangle.Width - (Inset * 2f), MaximumRectangle.Height - (Inset * 2f));
	private RectangleF PreviewRect => new RectangleF(Inset, Inset, MaximumRectangle.Width - (Inset * 2f), MaximumRectangle.Height - (Inset * 5f));
	private IGraphicsPath PreviewPath => GraphicsPath.GetRoundRect(PreviewRect, Radius);

	private RectangleF StatusRect => RectangleF.FromCenter(PreviewRect.Center, new SizeF(40f, 40f));
	private IGraphicsPath StatusPath => GraphicsPath.GetRoundRect(StatusRect, Radius);

	private record struct PushIcon(Bitmap Image, Color Colour);

	private void RenderBase(PaintEventArgs e)
	{
		float inset = 8f;
		float radius = 6f;
		var rect = MaximumRectangle;
		var state = ViewModel.State;

		if (state.HasFlag(ModelRenderState.MouseOver))
		{
			// Highlight
			e.Graphics.FillPath(Palette.Shadow, BoundingPath);

			// Show a slight shadow
			e.Graphics.TranslateTransform(1f, 2f);
			e.Graphics.FillPath(Palette.Shadow, PreviewPath);
			e.Graphics.TranslateTransform(-1f, -2f);
		}

		e.Graphics.FillPath(Palette.DarkGray, PreviewPath);

		if (state.HasFlag(ModelRenderState.Loading))
			e.Graphics.FillPath(Palette.Blue, PreviewPath);

		else if (state.HasFlag(ModelRenderState.Add))
			e.Graphics.FillPath(Palette.Pink, PreviewPath);

		else if (state.HasFlag(ModelRenderState.Sandbox))
			e.Graphics.FillPath(Palette.Purple, PreviewPath);
	}

	#endregion

	private void RenderSandbox(PaintEventArgs e)
	{
		var box = RectangleF.FromCenter(PreviewRect.Center, new SizeF(30f, 30f));
		e.Graphics.FillRectangle(Palette.White, box);

		RenderAddressBar(e);
	}

	private void RenderPlus(PaintEventArgs e, Color color, float roationInDegrees = 0)
	{
		e.Graphics.SaveTransform();
		e.Graphics.TranslateTransform(PreviewRect.Center);

		var crossRect = RectangleF.FromCenter(new PointF(0, 0), new SizeF(30f, 8f));
		var crossRect90 = RectangleF.FromCenter(new PointF(0, 0), new SizeF(8f, 30f));

		e.Graphics.RotateTransform(roationInDegrees);

		e.Graphics.FillRectangle(color, crossRect);
		e.Graphics.FillRectangle(color, crossRect90);

		e.Graphics.RestoreTransform();
	}

	private void RenderFailedToLoad(PaintEventArgs e)
	{
		RenderPlus(e, Palette.Red, 45f);
		RenderAddressBar(e);
	}

	private void RenderLoaded(PaintEventArgs e)
	{
		if (ViewModel.Model is not SharedModel sharedModel) return;
		if (sharedModel.Thumbnail is null) return;

		var textureBrush = new TextureBrush(sharedModel.Thumbnail, 1f);
		var yHeight = -((sharedModel.Thumbnail.Height - (MaximumRectangle.Height - 28f)) / 2f);
		var matrix = Matrix.FromTranslation(0, yHeight);
		textureBrush.Transform = matrix;

		e.Graphics.FillPath(textureBrush, PreviewPath);
		RenderAddressBar(e);
	}

	private void RenderLoading(PaintEventArgs e)
	{
		int startArc = 0 + (Frame * 4);
		int endArc = 120 + (Frame * 6);

		if (startArc > 360)
			startArc = 0;

		if (endArc > 360)
			endArc = 120;

		// TODO : Draw a cicle and move the line pattern instead
		e.Graphics.DrawArc(new Pen(Palette.White, 4f), StatusRect, startArc, endArc);

		RenderAddressBar(e);
	}

	private void RenderAddressBar(PaintEventArgs e)
	{
		var address = ViewModel?.Model?.ModelAddress;
		if (string.IsNullOrEmpty(address)) return;
		var textContainer = new RectangleF(PreviewRect.Left, PreviewRect.Bottom + 10f, PreviewRect.Width, MaximumRectangle.Bottom - PreviewRect.Bottom);

		var font = SystemFonts.Default(16f);
		var brush = new SolidBrush(Palette.TextColour);

		var fullSize = e.Graphics.MeasureString(font, address);
		e.Graphics.DrawText(font, brush, textContainer, address, alignment: FormattedTextAlignment.Left, wrap: FormattedTextWrapMode.Word, trimming: FormattedTextTrimming.CharacterEllipsis);
	}

	private void RenderAdd(PaintEventArgs e)
	{
		RenderPlus(e, Palette.White, 0f);
		RenderAddressBar(e);
	}

	public override string ToString() => $"{this.ViewModel.Model.ModelAddress}";

}
