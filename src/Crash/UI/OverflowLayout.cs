using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI
{

	internal class OverflowLayout<TItem> : DynamicLayout
	{
		private int ControlWidth { get; }

		public ObservableCollection<TItem> DataStore { get; set; } = new ObservableCollection<TItem>();
		private Func<TItem, Control> ControlFactory { get; }

		public OverflowLayout(int controlWidth, Func<TItem, Control> controlFactory)
		{
			ControlFactory = controlFactory;
			ControlWidth = controlWidth;
			BackgroundColor = Colors.Orange;
		}

		override protected void OnShown(EventArgs e)
		{
			InitLayout();
			InitBindings();
			var t = this;
			base.OnPreLoad(e);
		}

		private void InitBindings()
		{
			DataStore.CollectionChanged += (s, e) =>
			{
				InitLayout();
			};
		}

		private void InitLayout()
		{
			Clear();
			var width = this.GetPreferredSize().Width;
			int HorizontalControlCount = (int)(width / (this.ControlWidth + Spacing.Value.Width));

			BeginVertical();

			for (int i = 0; i < DataStore.Count; i++)
			{
				int controlWidth = 0;
				var stackLayout = new StackLayout()
				{
					VerticalContentAlignment = VerticalAlignment.Center,
					Spacing = this.Spacing.Value.Width,
					HorizontalContentAlignment = HorizontalAlignment.Left,
					Orientation = Orientation.Horizontal,
					Padding = new Padding(0, 8),
					Width = this.Width,
				};

				while (controlWidth < width)
				{
					if (i >= DataStore.Count)
					{
						// Done
						i = int.MaxValue / 2;
						controlWidth = int.MaxValue / 2;
						break;
					}
					var control = ControlFactory(DataStore[i]);
					stackLayout.Items.Add(control);
					stackLayout.Height = control.Height;
					controlWidth += control.Width + stackLayout.Spacing;
					i++;
				}

				Add(stackLayout, true, false);
			}

			AddSpace(true, true);

			EndVertical(); ;
		}

	}
}
