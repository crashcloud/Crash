using Eto.Drawing;
using Eto.Forms;

using Crash.Resources;

using Item = Crash.UI.RightClickMenuItem;

namespace Crash.UI
{

	internal record struct RightClickMenuItem(string Text, string IconKey, Action Command)
	{
		internal bool Hover { get; set; }
	}

	internal class RightClickMenu : Drawable
	{
		private const int RowHeight = 48;
		private const int IconSize = 24;
		private const int Radius = 4;
		private const int Inset = 6;
		private RectangleF InsetBounds => new RectangleF(Inset, Inset, Width - (Inset * 2), Height - (Inset * 2));
		private IGraphicsPath InsetPath => GraphicsPath.GetRoundRect(InsetBounds, Radius);
		private IGraphicsPath FullPath => GraphicsPath.GetRoundRect(new RectangleF(0f, 0f, Width, Height), Radius);

		private List<Item> Items { get; } = new();

		public RightClickMenu(List<Item> items)
		{
			Width = (RowHeight * 4) + (Inset * 2);
			Height = (items.Count * RowHeight) + (Inset * 2);
			Items.AddRange(items);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (Items is null) return;

			var font = SystemFonts.Default(18f);
			var brush = new SolidBrush(Palette.White);

			e.Graphics.FillPath(Palette.Shadow, FullPath);
			e.Graphics.FillPath(ParentWindow.BackgroundColor, InsetPath);
			e.Graphics.TranslateTransform(Inset, Inset);

			var textBounds = new RectangleF(IconSize + (Inset * 3f), 12f, InsetBounds.Width, RowHeight);

			for (int i = 0; i < Items.Count; i++)
			{
				var menuItem = Items[i];
				var menuBounds = new RectangleF(0f, 0f, InsetBounds.Width, RowHeight);
				if (menuItem.Hover)
					e.Graphics.FillRectangle(Palette.Shadow, menuBounds);

				var image = CrashIcons.Icon(menuItem.IconKey, IconSize);
				var imagePoint = new PointF(Inset, (RowHeight - IconSize) / 2f);
				e.Graphics.DrawImage(image, imagePoint);

				e.Graphics.DrawText(font, brush, textBounds, menuItem.Text);
				e.Graphics.TranslateTransform(0f, RowHeight);
			}

			base.OnPaint(e);
		}

		private bool TryGetItemAtLocation(PointF location, out Item item)
		{
			item = default;
			for (int i = 0; i < Items.Count; i++)
			{
				item = Items[i];
				var bounds = new RectangleF(Inset, (RowHeight * i) + Inset, InsetBounds.Width, RowHeight);
				if (bounds.Contains(location))
					return true;
			}

			return false;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			TryGetItemAtLocation(e.Location, out var item);
			item.Command?.Invoke();

			base.OnMouseUp(e);
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
