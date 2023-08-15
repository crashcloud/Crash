using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Eto.Forms;
using Eto.Drawing;
using System.Runtime.InteropServices;
using static Crash.UI.SharedModelViewModel;

namespace Crash.UI
{

	[Guid("37943c4b-5c30-471c-a5b0-c1bdaafa628d")]
	public partial class JoinWindow : Form
	{

		private int Width = 400;
		private int Height = 200;
		private int Padding = 4;

		internal IEnumerable<object> Users { get; set; } = Array.Empty<object>();

		GridColumn OpenCell { get; set; }
		GridColumn TextCell { get; set; }
		GridColumn DividerCell { get; set; }
		GridColumn UserIconCell { get; set; }
		GridColumn UserCountCell { get; set; }
		GridColumn SignalCell { get; set; }

		public void InitializeComponent()
		{
			OpenCell = CreateOpenCell();
			TextCell = CreateTextCell();
			DividerCell = CreateDividerCell();
			UserIconCell = CreateUserIconCell();
			UserCountCell = CreateCountCell();
			SignalCell = CreateSignalCell();

			GridView gridView = new GridView()
			{
				Size = new Size(Width, Height),
				// CanDeleteItem = true, // TODO
				DataStore = Users,
				ShowHeader = false,
				RowHeight = 25,
				AllowColumnReordering = false,
				AllowMultipleSelection = false,
				Border = BorderType.None,
				GridLines = GridLines.Horizontal,
				AllowDrop = false,
				AllowEmptySelection = false,
			};

			gridView.Columns.Add(OpenCell);
			gridView.Columns.Add(TextCell);
			gridView.Columns.Add(DividerCell);
			gridView.Columns.Add(UserIconCell);
			gridView.Columns.Add(UserCountCell);
			gridView.Columns.Add(SignalCell);

			Content = new StackLayout
			{
				Padding = 0,
				Spacing = 0,
				Size = new Size(Width, Height),
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
				BackgroundColor = new Color(1,1,1),
				Enabled = true,
				Items =
				{
					gridView
				},
			};
		}

		private GridColumn CreateOpenCell()
		{
			return new GridColumn()
			{
				Width = 60,
				Resizable = false,
				DataCell = CreateOpenButton()
			};
		}

		private GridColumn CreateTextCell()
		{
			return new GridColumn()
			{
				Width = 200,
				Resizable = false,
				// Editable = editable,
				HeaderTextAlignment = TextAlignment.Left,
				DataCell = new TextBoxCell
				{
					Binding = Binding.Property<SharedModel, string>(s => s.ModelAddress)
				}
			};
		}

		private GridColumn CreateDividerCell()
		{
			var dividerCell = new DrawableCell();
			dividerCell.Paint += (sender, args) =>
			{
				args.Graphics.DrawLine(new Color(125, 125, 125), new PointF(5, 4), new PointF(5, 16));
			};

			return new GridColumn()
			{
				Width = 10,
				Resizable = false,
				DataCell = dividerCell,
			};
		}

		private GridColumn CreateUserIconCell()
		{
			return new GridColumn()
			{
				Width = 30,
				Resizable = false,
				DataCell = new ImageViewCell
				{
					Binding = Binding.Property<SharedModel, Image>(s => s.UserIcon)
				}
			};
		}

		private GridColumn CreateCountCell()
		{
			return new GridColumn()
			{
				Width = 30,
				Resizable = false,
				DataCell = new TextBoxCell
				{
					Binding = Binding.Property<SharedModel, string>(s => s.UserCount)
				}
			};
		}

		private GridColumn CreateSignalCell()
		{
			return new GridColumn()
			{
				Width = 30,
				Resizable = false,
				DataCell = new ImageViewCell
				{
					Binding = Binding.Property<SharedModel, Image>(s => s.Signal)
				}
			};
		}

	}

}
