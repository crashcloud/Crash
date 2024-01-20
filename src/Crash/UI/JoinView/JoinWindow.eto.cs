using System.Runtime.InteropServices;

using Eto;
using Eto.Drawing;
using Eto.Forms;

using Rhino.UI;

namespace Crash.UI.JoinModel
{
	[Guid("37943c4b-5c30-471c-a5b0-c1bdaafa628d")]
	public partial class JoinWindow
	{
		private static int WindowWidth => 500;
		private static int WindowHeight => 260;

		private static int RowHeight => 25;
		private static int DividerHeight => 5;

		private static int OpenCellWidth => 60;
		private static int DividerCellWidth => 10;
		private static int UserIconCellWidth => 30;
		private static int CountCellWidth => 30;
		private static int SignalCellWidth => 30;
		private static int TextCellWidth => 280;

		private static int OSX_Padd => 10;

		protected GridView ActiveModels;
		protected StackLayout NewModel;

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

			NewModel = CreateAddLayout();
			Padding = 0;

			Content = new StackLayout
			          {
				          Padding = 0,
				          Spacing = 0,
				          HorizontalContentAlignment = HorizontalAlignment.Stretch,
				          VerticalContentAlignment = VerticalAlignment.Top,
				          Enabled = true,
				          Items = {	
								new Scrollable()
								{
									Content = ActiveModels,
									Padding = 0,
									Size = new Size(WindowWidth, 150),
									ScrollSize = new Size(1, 1),
									ExpandContentHeight = true,
								},
								DrawHorizonalCell(),
								NewModel
							}
			          };
		}

		private Drawable DrawHorizonalCell()
		{
			var drawable = new Drawable { Height = DividerHeight, Width = WindowWidth - 6 };
			drawable.Padding = 0;
			drawable.Paint += (sender, e) =>
			                  {
				                  var half = (float)DividerHeight / 2;
				                  var start = new PointF(0, half);
				                  var end = new PointF(WindowWidth - DividerHeight, half);
				                  e.Graphics.DrawLine(Palette.SubtleGrey, start, end);
			                  };

			return drawable;
		}

		protected const string DefaultGridStyle = "DefaultGrid";
		protected const string AddButtonStyle = "AddButton";
		private void InitStyles()
		{
			this.Styles.Add<GridView>(DefaultGridStyle, (grid) =>
			{

				// Allows
				grid.AllowDrop = false;
				grid.AllowEmptySelection = false;
				grid.AllowColumnReordering = false;
				grid.AllowMultipleSelection = false;
				grid.ShowHeader = false;
				grid.RowHeight = RowHeight;
				grid.Border = BorderType.None;

				// Styling
				grid.Border = BorderType.None;
				grid.GridLines = GridLines.Horizontal;
			});

			this.Styles.Add<Button>(AddButtonStyle, (button) =>
			{

			});
		}

		private GridView CreateModelGrid()
		{
			var gridView = new GridView
			               {
								Style = DefaultGridStyle,

				               // Help
				               ToolTip = "Choose a model to join",
			               };
			gridView.ContextMenu = CreateContextMenu();

			gridView.Columns.Add(CreateOpenCell());
			gridView.Columns.Add(CreateTextCell(false));
			gridView.Columns.Add(CreateDividerCell());
			gridView.Columns.Add(CreateUserIconCell());
			gridView.Columns.Add(CreateCountCell(false));
			gridView.Columns.Add(CreateSignalCell());

			return gridView;
		}

		private ContextMenu? CreateContextMenu()
		{
			var menu = new ContextMenu();
			menu.Items.AddRange(new[]
			                    {
				                    MenuCommand("Remove", RemoveModel),
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

		private TextBox inputTextBox = new TextBox
		{
			AutoSelectMode = AutoSelectMode.OnFocus,
			TextAlignment = TextAlignment.Center,
			PlaceholderText = "Enter a new Model Address",
			ShowBorder = false,
			ToolTip = "Enter a new Model Address to Join",
			Width = TextCellWidth,
		};

		private StackLayout CreateAddLayout()
		{
			var addView = new StackLayout
			{
				// Style = DefaultGridStyle,
				Padding = 0,
				Orientation = Orientation.Horizontal,
				Items =
				{
					new Button
					{
						Text = "+",
						ToolTip = "Add a new Shared Model",
						Command = new Command(AddNewModel) { CommandParameter = inputTextBox },
						Width = OpenCellWidth
					},
					inputTextBox,
				},
				Size = new Size(-1, 26),
				VerticalContentAlignment = VerticalAlignment.Top,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
			};

			return addView;
		}

		private GridView CreateExistingGrid()
		{
			var existingView = CreateModelGrid();
			existingView.DataStore = Model.SharedModels;

			return existingView;
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
				       Editable = true,
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
