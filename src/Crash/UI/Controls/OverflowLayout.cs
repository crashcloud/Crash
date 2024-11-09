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
		private List<DynamicLayout> CachedRows { get; set; }

		private Func<TItem, Control> ControlFactory { get; }

		public OverflowLayout(int controlWidth, ObservableCollection<TItem> sharedModels, Func<TItem, Control> controlFactory)
		{
			ControlFactory = controlFactory;
			ControlWidth = controlWidth;
			DataStore = sharedModels;
			CachedRows = new();
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

		internal float RealWidth => Width < 0 ? GetPreferredSize().Width : Width;
		internal int HorizontalControlCount => (int)(RealWidth / (ControlWidth));
		internal int VerticalControlCount => (int)Math.Ceiling(DataControls.Count / (float)HorizontalControlCount);

		public void InitLayout()
		{
			return;
			int max = Math.Max(VerticalControlCount, CachedRows.Count);
			for (int i = 0; i < max; i++)
			{
				if (i >= VerticalControlCount)
				{
					var row = GetRow(i);
					row.Clear();
					continue;
				}
				else
				{
					var row = GetRow(i);
					FillRow(row, i);
				}
				Invalidate(true);
			}
		}

		internal void FillRow(DynamicLayout row, int i)
		{
			row.Clear();
			row.BeginHorizontal();

			for (int j = 0; j < HorizontalControlCount; j++)
			{
				var index = i * HorizontalControlCount + j;
				if (index >= DataControls.Count)
					break;

				var control = DataControls[index];
				row.Add(control, false, false);
				row.Height = control.Height;
			}

			row.AddSpace(true, true);
			row.EndHorizontal();
		}

		internal DynamicLayout GetRow(int index)
		{
			if (index < CachedRows.Count)
			{
				var row = CachedRows[index];
				row.Clear();
				return row;
			}
			else
			{
				var row = new DynamicLayout()
				{
					Padding = new Padding(0, 4),
					Width = -1,
					Spacing = new Size(8, 0),
				};

				Add(row, false, false);
				return row;
			}
		}

		private void CreateControls()
		{
			if (DataControls?.Count > 0) return;

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
