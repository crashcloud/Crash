using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI
{

	internal class OverflowLayout<TItem> : DynamicLayout
	{
		public int ControlWidth { get; set; }

		public ObservableCollection<TItem> DataStore { get; set; } = new ObservableCollection<TItem>();
		public ObservableCollection<Control> DataControls { get; set; } = new ObservableCollection<Control>();
		private Func<TItem, Control> ControlFactory { get; }

		public OverflowLayout(int controlWidth, ObservableCollection<TItem> sharedModels, Func<TItem, Control> controlFactory)
		{
			ControlFactory = controlFactory;
			ControlWidth = controlWidth;
			DataStore = sharedModels;
			Width = Width;
			Height = Width;
			Spacing = new Size(16, 16);
			MinimumSize = new Size(800, 500);

			CreateControls();
			InitBindings();
			InitLayout();
		}

		private void InitBindings()
		{
			SizeChanged += (s, e) =>
			{
				InitLayout();
			};
			DataStore.CollectionChanged += (s, e) =>
			{
				CreateControls();
			};
		}

		public void InitLayout()
		{
			return;
			Clear();
			var width = this.GetPreferredSize().Width;
			int HorizontalControlCount = (int)(width / (this.ControlWidth)); // + Spacing.Value.Width));

			BeginVertical();
			Add(new Button() { Text = "Test" }, false, false);

			for (int i = 0; i < DataStore.Count; i++)
			{
				int controlWidth = 0;
				var stackLayout = new DynamicLayout()
				{
					// VerticalContentAlignment = VerticalAlignment.Center,
					// Spacing = Spacing.Value.Width,
					Padding = new Padding(0, 8),
					Width = this.Width,
					Height = 120,
					BackgroundColor = Colors.Green,
					MinimumSize = new Size(this.Width, 20),
				};

				stackLayout.BeginHorizontal();
				stackLayout.Add(new Button() { Text = "Test" }, false, false);
				while (controlWidth < width)
				{
					if (i >= DataStore.Count)
					{
						i = int.MaxValue / 2;
						controlWidth = int.MaxValue / 2;
						break;
					}
					var control = ControlFactory(DataStore[i]);
					stackLayout.Add(control, true, false);
					stackLayout.Height = control.Height;
					controlWidth += control.Width;
					i++;
				}
				stackLayout.AddSpace(true, true);
				stackLayout.EndHorizontal();

				Add(stackLayout, false, false);
			}

			AddSpace(true, true);

			EndVertical();
			Invalidate();
		}

		private void CreateControls()
		{
			if (DataStore.Count == DataControls.Count) return;

			if (DataStore.Count > DataControls.Count)
			{
				for (int i = DataControls.Count; i < DataStore.Count; i++)
				{
					var control = ControlFactory(DataStore[i]);
					DataControls.Add(control);
				}

				return;
			}

			DataControls.Clear();
			foreach (var item in DataStore)
			{
				var control = ControlFactory(item);
				DataControls.Add(control);
			}
		}
	}
}
