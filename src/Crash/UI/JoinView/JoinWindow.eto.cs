using System.Runtime.InteropServices;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI.JoinModel
{
	[Guid("37943c4b-5c30-471c-a5b0-c1bdaafa628d")]
	public partial class JoinWindow
	{
		private static readonly int WindowWidth = IsOSX ? 400 : 350;
		private static readonly int WindowHeight = 200;

		private static readonly int RowHeight = 25;
		private static readonly int DividerHeight = 5;

		private static readonly int OpenCellWidth = 60;
		private static readonly int DividerCellWidth = 10;
		private static readonly int UserIconCellWidth = 30;
		private static readonly int CountCellWidth = 30;
		private static readonly int SignalCellWidth = 30;
		private static readonly int TextCellWidth = 180;

		private static readonly int OSX_Padd = 10;

		protected GridView ActiveModels;
		protected GridView NewModel;

		private static int ActiveModelsHeight => WindowHeight -
		                                         (RowHeight +
		                                          DividerHeight);

		protected SharedModel CurrentSelection { get; set; }

		public void InitializeComponent()
		{
			ActiveModels = CreateExistingGrid();
			ActiveModels.SelectionChanged += (sender, args) =>
			                                 {
				                                 if (ActiveModels.SelectedItem is SharedModel model)
				                                 {
					                                 CurrentSelection = model;
				                                 }
			                                 };

			NewModel = CreateAddGrid();
			Padding = 0;
			// Size = new Size(-1, WindowHeight + (IsOSX ? OSX_Padd : 0));
			MinimumSize = new Size(WindowWidth, WindowHeight);

			Content = new StackLayout
			          {
				          Padding = 0,
				          Spacing = 0,
				          Size = new Size(WindowWidth, WindowHeight + (IsOSX ? OSX_Padd : 0)),
				          MinimumSize = new Size(WindowWidth, WindowHeight),
				          HorizontalContentAlignment = HorizontalAlignment.Stretch,
				          VerticalContentAlignment = VerticalAlignment.Stretch,
				          Enabled = true,
				          Items = {
								new Scrollable()
								{
									Content = ActiveModels,
								},
								DrawHorizonalCell(),
								NewModel
							}
			          };
		}

		private Drawable DrawHorizonalCell()
		{
			var drawable = new Drawable { Height = DividerHeight, Width = WindowWidth };
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
			var gridView = new GridView
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
				               GridLines = GridLines.Horizontal,
				               ContextMenu = CreateContextMenu(isEditable),

				               // Help
				               ToolTip = isEditable ? "Enter a model address to join" : "Choose a model to join",
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
			{
				return null;
			}

			var menu = new ContextMenu();
			menu.Items.AddRange(new[]
			                    {
				                    MenuCommand("Remove", RemoveModel), MenuCommand("Refresh", RefreshModels),
				                    MenuCommand("Join", JoinModel)
			                    });

			return menu;
		}

		private MenuItem MenuCommand(string label, EventHandler<EventArgs> eventHandler)
		{
			var button = new ButtonMenuItem
			             {
				             Text = label, ToolTip = $"{label} Item", Command = new Command(eventHandler)
			             };

			return button;
		}

		private GridView CreateAddGrid()
		{
			var addView = CreateDefaultGrid(true);

			if (IsOSX)
			{
				addView.Size = new Size(WindowWidth, RowHeight + OSX_Padd);
			}
			else
			{
				addView.Size = new Size(WindowWidth, -1);
			}

			addView.DataStore = Model.AddModels;

			return addView;
		}

		private GridView CreateExistingGrid()
		{
			var existingView = CreateDefaultGrid();
			existingView.Size = new Size(WindowWidth, ActiveModelsHeight);
			existingView.DataStore = Model.SharedModels;

			return existingView;
		}

		private GridColumn CreateAddCell()
		{
			var customCell = new CustomCell { CreateCell = CreateAddButtonContents };

			return new GridColumn
			       {
				       Width = OpenCellWidth,
				       Resizable = false,
				       DataCell = customCell,
			};
		}

		private GridColumn CreateOpenCell()
		{
			return new GridColumn
			       {
				       Width = OpenCellWidth,
				       Resizable = false,
				       DataCell = new CustomCell { CreateCell = CreateOpenButtonContents }
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

			return new GridColumn
			       {
				       Width = TextCellWidth,
				       Resizable = false,
				       AutoSize = false,
				       Editable = isEditable,
				       HeaderTextAlignment = TextAlignment.Left,
				       DataCell = addressCell,
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

			return new GridColumn { Width = DividerCellWidth, Resizable = false, DataCell = dividerCell };
		}

		private GridColumn CreateUserIconCell()
		{
			return new GridColumn
			       {
				       Width = UserIconCellWidth,
				       Resizable = false,
				       DataCell = new ImageViewCell { Binding = Binding.Property<SharedModel, Image>(s => s.UserIcon) }
			       };
		}

		private GridColumn CreateCountCell(bool isEditable = false)
		{
			return new GridColumn
			       {
				       Width = CountCellWidth,
				       Resizable = false,
				       DataCell = new TextBoxCell
				                  {
					                  Binding =
						                  Binding.Property<SharedModel, string>(s => isEditable ? "" : s.UserCount)
				                  }
			       };
		}

		private GridColumn CreateSignalCell()
		{
			return new GridColumn
			       {
				       Width = SignalCellWidth,
				       Resizable = false,
				       DataCell = new ImageViewCell { Binding = Binding.Property<SharedModel, Image>(s => s.Signal) }
			       };
		}
	}
}
