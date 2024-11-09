using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Crash.UI.JoinView;
using Crash.UI.RecentView.Layers;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI.RecentView
{

	internal class AddressInputBar : Drawable
	{
		private JoinViewModel Model { get; }

		private RecentModelDialog ParentControl => ParentWindow as RecentModelDialog;

		public AddressInputBar(JoinViewModel model)
		{
			Model = model;
			CanFocus = true;
			AllowDrop = false;

			Model.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName != nameof(JoinViewModel.TemporaryModel)) return;
				Invalidate();
			};
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.Clear(Colors.Transparent);
			// Draw nothing until we need to add!
			// Temporary?
			if (Model.TemporaryModel is null) return;

			Width = Parent.Width;
			Height = Parent.Height;

			var brush = new SolidBrush(Color.FromArgb(160, 160, 160, 160));
			e.Graphics.FillRectangle(Palette.GetHashedTexture(6, 0.75f), 0, 0, Width, Height);

			var bar = RectangleF.FromCenter(Parent.Bounds.Center, new SizeF(600, 80));
			bar.Y -= 60f;
			var b = new SolidBrush(Palette.White);
			e.Graphics.FillRectangle(b, bar);

			// var font = SystemFonts.Default(24f);
			// e.Graphics.DrawText(font, new SolidBrush(Palette.Black), bar, Model.TemporaryModel.ModelAddress, alignment: FormattedTextAlignment.Center);
			PillLayer.RenderAddress(e, Model.TemporaryModel.ModelAddress, bar, true);

			base.OnPaint(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (Model.TemporaryModel is null) return;
			Invalidate(true);

			base.OnMouseDown(e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
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
				Invalidate(true);
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
					// ParentControl.AddModelsToGallery(new[] { Model.TemporaryModel });
				}
				Model.TemporaryModel = null;
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

			Invalidate(true);
			base.OnKeyDown(e);
		}

	}
}
