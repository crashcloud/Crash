using Eto.Drawing;
using Eto.Forms;

using Crash.Resources;

using Crash.UI.RecentView;
using Rhino.Runtime;

namespace Crash.UI
{

	internal class RightClickMenu : Drawable
	{
		private const int RowHeight = 48;
		private const int IconSize = 24;
		private const int Radius = 4;
		private const int Inset = 6;
		private RectangleF InsetBounds => new RectangleF(Inset, Inset, Width - (Inset * 2), Height - (Inset * 2));
		private IGraphicsPath InsetPath => GraphicsPath.GetRoundRect(InsetBounds, Radius);
		private IGraphicsPath FullPath => GraphicsPath.GetRoundRect(new RectangleF(0f, 0f, Width, Height), Radius);

		public List<CrashCommands.CrashCommand> Items { get; } = new();

		public RightClickMenu(List<CrashCommands.CrashCommand> commands)
		{
			AddItems(commands);
			Height = (RowHeight * commands.Count) + (Inset * 2);
		}

		public void AddItem(CrashCommands.CrashCommand command)
		{
			Items.Add(command);

			Width = (RowHeight * 4) + (Inset * 2);
			Height = (Items.Count * RowHeight) + (Inset * 2);
		}

		public void AddItems(List<CrashCommands.CrashCommand> commands)
		{
			foreach (var command in commands)
				AddItem(command);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (Items is null) return;

			var fontSize = 18f;
			var font = SystemFonts.Default(fontSize);

			e.Graphics.FillPath(Palette.Shadow, FullPath);
			e.Graphics.FillPath(ParentWindow.BackgroundColor, InsetPath);
			e.Graphics.TranslateTransform(Inset, Inset);

			float fontOffset = HostUtils.RunningOnOSX ? 12f : 4f;
			var textBounds = new RectangleF(RowHeight + (Inset * 3f), fontOffset, InsetBounds.Width, RowHeight);

			var menuBounds = new RectangleF(0f, 0f, InsetBounds.Width, RowHeight);

			var imageBounds = new RectangleF(Inset, Inset, RowHeight, RowHeight);
			if (HostUtils.RunningOnOSX)
			{
				imageBounds = new RectangleF(0f, 0f, RowHeight, RowHeight);
				imageBounds.Inset(Inset);
			}

			for (int i = 0; i < Items.Count; i++)
			{
				var command = Items[i];
				var colour = GetColor(command);

				if (command.Hover)
					e.Graphics.FillRectangle(Palette.Shadow, menuBounds);

				var image = command.GetIcon(256, colour);
				e.Graphics.DrawImage(image, new RectangleF(0, 0, image.Width, image.Height), imageBounds);

				e.Graphics.DrawText(font, new SolidBrush(colour), textBounds, command.MenuText);

				e.Graphics.TranslateTransform(0f, RowHeight);
			}

			base.OnPaint(e);
		}

		private static Color GetColor(CrashCommands.CrashCommand command)
		{
			if (command.ColourOverride != Colors.Transparent)
				return command.ColourOverride;

			// var colour = command.Enabled ? Palette.TextColour : Palette.DisabledTextColour;
			return Palette.TextColour;
		}

		private bool TryGetItemAtLocation(PointF location, out CrashCommands.CrashCommand command)
		{
			command = default;
			for (int i = 0; i < Items.Count; i++)
			{
				command = Items[i];
				var bounds = new RectangleF(Inset, (RowHeight * i) + Inset, InsetBounds.Width, RowHeight);
				if (bounds.Contains(location))
					return true;
			}

			return false;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Buttons == MouseButtons.Primary)
			{
				e.Handled = true;
				TryGetItemAtLocation(e.Location, out var command);
				Visible = false;
				Invalidate();
				command?.Execute();
			}

			base.OnMouseDown(e);
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				var item = Items[i];
				item.Hover = false;
				Items[i] = item;
			}

			base.OnMouseLeave(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				var item = Items[i];
				item.Hover = false;
				Items[i] = item;
			}

			base.OnLostFocus(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				var item = Items[i];
				item.Hover = false;
				Items[i] = item;
			}

			if (TryGetItemAtLocation(e.Location, out var chosenItem))
			{
				int index = Items.IndexOf(chosenItem);
				if (index >= 0)
				{
					chosenItem.Hover = true;
					Items[index] = chosenItem;
				}
			}

			Invalidate(false);
			base.OnMouseMove(e);
		}

	}

}
