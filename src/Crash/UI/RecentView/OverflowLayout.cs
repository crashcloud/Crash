using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI
{

	internal class OverflowLayout<TItem> : PixelLayout
	{
		public int ControlWidth { get; set; } = -1;
		public int ControlHeight { get; set; } = -1;
		public int Padding => RecentModelDialog.PreviewPadding;

		public ObservableCollection<TItem> DataStore { get; set; }
		public List<DynamicLayout> CachedRows { get; set; }
		private Func<TItem, Control> ControlFactory { get; }

		private RightClickMenu RightClickMenu { get; }

		public OverflowLayout(ObservableCollection<TItem> sharedModels, Func<TItem, Control> controlFactory)
		{
			ControlFactory = (i) =>
			{
				var control = controlFactory(i);
				SubscribeObjectToEvents(control);
				return control;
			};
			DataStore = sharedModels;
			CachedRows = new();
			RightClickMenu = new RightClickMenu(new() {
				new ("Join", "join.png", () => {}),
				new ("Remove", "close.png", () => {}),
				new ("Reload", "reload.png",  () => {}),
			});
			Width = Width;
			Height = Width;

			InitLayout();
			InitBindings();
			SubscribeObjectToEvents(this);
		}

		// TODO : Use Parent to get the width
		private void InitLayout()
		{
			if (Controls is not IList<Control> controls) return;
			if (controls?.Count > 0) return;

			if (DataStore.Count > controls.Count)
			{
				for (int i = controls.Count; i < DataStore.Count; i++)
				{
					var control = ControlFactory(DataStore[i]);
					ControlWidth = control.Width;
					ControlHeight = control.Height;
					Add(control, 0, 0);
				}

				return;
			}

			controls.Clear();
			foreach (var item in DataStore)
			{
				var control = ControlFactory(item);
				ControlWidth = control.Width;
				ControlHeight = control.Height;
				Add(control, 0, 0);
			}

			RepositionLayout();
		}

		private void InitBindings()
		{
			Shown += (s, e) =>
			{
				RepositionLayout();

				ParentWindow.SizeChanged += (s, e) =>
				{
					Width = ParentWindow.Width;
					RepositionLayout();
				};
				DataStore.CollectionChanged += (s, e) =>
				{
					// TODO : Implement
				};
			};
		}

		internal float RealWidth => ParentWindow?.Width ?? this.GetPreferredSize().Width;
		internal int HorizontalControlCount => (int)((RealWidth - Padding) / (ControlWidth + Padding));
		internal int VerticalControlCount => (int)Math.Ceiling(Controls.Count() / (float)HorizontalControlCount);

		public void RepositionLayout()
		{
			// TODO : Use Parent to get the width
			if (Controls is not IList<Control> controls) return;
			if (controls.Count == 0) return;

			for (int i = 0; i < HorizontalControlCount; i++)
			{
				for (int j = 0; j < VerticalControlCount; j++)
				{
					var index = i + j * HorizontalControlCount;
					if (index >= controls.Count) break;

					var control = controls[index];
					int x = (i * (ControlWidth + Padding)) + Padding;
					int y = (j * (ControlHeight + Padding)) + Padding;
					var point = new Point(x, y);
					SetLocation(control, point);
				}
			}

			// TODO : Sizing is flimsy
			if (Parent is not Scrollable scrollable) return;
			if (ParentWindow is null) return;
			scrollable.Height = ParentWindow.Height - 80;
			Height = (VerticalControlCount * (ControlHeight + Padding)) + Padding;

			Invalidate(true);
		}

		private void SubscribeObjectToEvents(Control control)
		{
			control.MouseDown += (s, e) =>
			{
				if (e.Buttons == MouseButtons.Alternate)
					ShowRightClick(e);
				else
					HideRightClick();
			};

			control.KeyDown += (s, e) =>
			{
				if (e.Key == Keys.Escape)
					HideRightClick();
			};
		}

		private void HideRightClick()
		{
			Remove(RightClickMenu);
		}

		private void ShowRightClick(MouseEventArgs e)
		{
			var point = e.Location;
			Add(RightClickMenu, (int)e.Location.X, (int)e.Location.Y);
		}
	}
}
