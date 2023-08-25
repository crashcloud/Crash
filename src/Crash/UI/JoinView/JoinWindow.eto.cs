using System.Runtime.InteropServices;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI.JoinModel
{

	[Guid("37943c4b-5c30-471c-a5b0-c1bdaafa628d")]
	public partial class JoinWindow
	{

		private static int WindowWidth = 400;
		private static int WindowHeight = 200;
		private static int ActiveModelsHeight => WindowHeight -
			(RowHeight +
			DividerHeight);

		private static int RowHeight = 25;
		private static int DividerHeight = 5;

		private static int OpenCellWidth = 60;
		private static int DividerCellWidth = 10;
		private static int UserIconCellWidth = 30;
		private static int CountCellWidth = 30;
		private static int SignalCellWidth = 30;
		private static int TextCellWidth = 180;

		private static int OSX_Padd = 10;

		protected GridView ActiveModels;
		protected GridView NewModel;

		public void InitializeComponent()
		{
			ActiveModels = CreateExistingGrid();
			NewModel = CreateAddGrid();

			Content = new StackLayout
			{
				Padding = 0,
				Spacing = 0,
				Size = new Size(WindowWidth, WindowHeight + (IsOSX ? OSX_Padd : 0)),
				MinimumSize = new Size(WindowWidth, WindowHeight),
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch,
				BackgroundColor = Palette.White,
				Enabled = true,
				Items =
				{
					ActiveModels,
					DrawHorizonalCell(),
					NewModel
				},
			};
		}

		private Drawable DrawHorizonalCell()
		{
			var drawable = new Drawable()
			{
				Height = DividerHeight,
				Width = WindowWidth,
			};
			drawable.Paint += (sender, e) =>
			{
				var half = (float)DividerHeight / 2;
				var start = new PointF(DividerHeight, half);
				var end = new PointF(WindowWidth - DividerHeight, half);
				e.Graphics.DrawLine(Palette.SubtleGrey, start, end);
			};

			return drawable;
		}

		private GridView CreateDefaultGrid(bool isEditable = false)
		{
			GridView gridView = new GridView()
			{
				// Allows
				AllowDrop = false,
				AllowEmptySelection = false,
				AllowColumnReordering = false,
				AllowMultipleSelection = false,

				ShowHeader = false,

				RowHeight = RowHeight,

				// Styling
				Border = BorderType.None,
				GridLines = GridLines.None,
				BackgroundColor = Palette.White,
				ContextMenu = CreateContextMenu(isEditable),

				// Help
				ToolTip = "Choose a Model to Join",
			};

			gridView.Columns.Add(isEditable ? CreateAddCell() : CreateOpenCell());
			gridView.Columns.Add(CreateTextCell(isEditable));
			gridView.Columns.Add(CreateDividerCell());
			gridView.Columns.Add(CreateUserIconCell());
			gridView.Columns.Add(CreateCountCell(isEditable));
			gridView.Columns.Add(CreateSignalCell());

			return gridView;
		}

		private ContextMenu? CreateContextMenu(bool isEditable = false)
		{
			if (isEditable)
				return null;

			ContextMenu menu = new ContextMenu();
			menu.Items.AddRange(new MenuItem[]
			{
				MenuCommand("Remove", RemoveModel),
				MenuCommand("Refresh", RefreshModels),
				MenuCommand("Join", JoinModel),
			});

			return menu;
		}

		private MenuItem MenuCommand(string label, EventHandler<EventArgs> eventHandler)
		{
			ButtonMenuItem button = new ButtonMenuItem()
			{
				Text = label,
				ToolTip = $"{label} Item",
				Command = new Command(eventHandler),
			};

			return button;
		}

		private GridView CreateAddGrid()
		{
			GridView addView = CreateDefaultGrid(true);

			if (IsOSX)
				addView.Size = new Size(WindowWidth, RowHeight + OSX_Padd);
			else
				addView.Size = new Size(WindowWidth, -1);

			addView.DataStore = Model.AddModels;

			return addView;
		}

		private GridView CreateExistingGrid()
		{
			GridView existingView = CreateDefaultGrid();
			existingView.Size = new Size(WindowWidth, ActiveModelsHeight);
			existingView.DataStore = Model.SharedModels;

			return existingView;
		}

		private GridColumn CreateAddCell()
		{
			return new GridColumn()
			{
				Width = OpenCellWidth,
				Resizable = false,
				DataCell = new CustomCell
				{
					CreateCell = CreateAddButtonContents,
				}
			};
		}

		private GridColumn CreateOpenCell()
		{
			return new GridColumn()
			{
				Width = OpenCellWidth,
				Resizable = false,
				DataCell = new CustomCell
				{
					CreateCell = CreateOpenButtonContents,
				}
			};
		}

		private GridColumn CreateTextCell(bool isEditable = false)
		{
			var addressCell = new TextBoxCell
			{
				Binding = Binding.Property<SharedModel, string>(s => s.ModelAddress),
				AutoSelectMode = isEditable ? AutoSelectMode.Always : AutoSelectMode.OnFocus,
				TextAlignment = TextAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
			};

			return new GridColumn()
			{
				Width = TextCellWidth,
				Resizable = false,
				AutoSize = false,
				Editable = isEditable,
				HeaderTextAlignment = TextAlignment.Left,
				DataCell = addressCell
			};
		}

		private GridColumn CreateDividerCell()
		{
			var dividerCell = new DrawableCell();
			dividerCell.Paint += (sender, args) =>
			{
				args.Graphics.DrawLine(Palette.SubtleGrey,
										new PointF(DividerCellWidth / 2, 5),
										new PointF(DividerCellWidth / 2, RowHeight - 5));
			};

			return new GridColumn()
			{
				Width = DividerCellWidth,
				Resizable = false,
				DataCell = dividerCell,
			};
		}

		private GridColumn CreateUserIconCell()
		{
			return new GridColumn()
			{
				Width = UserIconCellWidth,
				Resizable = false,
				DataCell = new ImageViewCell
				{
					Binding = Binding.Property<SharedModel, Image>(s => s.UserIcon)
				}
			};
		}

		private GridColumn CreateCountCell(bool isEditable = false)
		{
			return new GridColumn()
			{
				Width = CountCellWidth,
				Resizable = false,
				DataCell = new TextBoxCell
				{
					Binding = Binding.Property<SharedModel, string>(s => isEditable ? "" : s.UserCount)
				}
			};
		}

		private GridColumn CreateSignalCell()
		{
			return new GridColumn()
			{
				Width = SignalCellWidth,
				Resizable = false,
				DataCell = new ImageViewCell
				{
					Binding = Binding.Property<SharedModel, Image>(s => s.Signal)
				}
			};
		}

	}

}
