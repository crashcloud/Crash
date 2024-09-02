using System.Runtime.InteropServices;

using Eto.Drawing;
using Eto.Forms;

namespace Crash.UI.JoinModel
{
	[Guid("37943c4b-5c30-471c-a5b0-c1bdaafa628d")]
	public partial class JoinWindow
	{
		protected const string DefaultGridStyle = "DefaultGrid";
		protected const string AddButtonStyle = "AddButton";

		protected GridView ActiveModels;

		protected StackLayout NewModel;

		private TextBox inputTextBox { get; } = new()
		{
			AutoSelectMode = AutoSelectMode.OnFocus,
			TextAlignment = TextAlignment.Center,
			PlaceholderText = "Enter a new Model Address",
			ShowBorder = false,
			ToolTip = "Enter a new Model Address to Join",
			Width = WindowWidth - (OpenCellWidth * 2),
			Height = RowHeight
		};

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

		private static int ActiveModelsHeight => WindowHeight -
												 (RowHeight +
													DividerHeight);

		protected SharedModel CurrentSelection { get; set; }

		public void InitializeComponent()
		{
			ActiveModels = CreateModelGrid();
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
				Items =
							{
								new Scrollable
								{
									Content = ActiveModels,
									Padding = 0,
									Size = new Size(WindowWidth, 150),
									ExpandContentHeight = true
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

		private void InitStyles()
		{
			Styles.Add<GridView>(DefaultGridStyle, grid =>
													 {
														 // Allows
														 grid.AllowDrop = false;
														 grid.AllowEmptySelection = false;
														 grid.AllowColumnReordering = false;
														 grid.AllowMultipleSelection = false;
														 grid.ShowHeader = false;
														 grid.RowHeight = RowHeight;

														 // Styling
														 grid.Border = BorderType.Line;
														 grid.GridLines = GridLines.Horizontal;
													 });
		}

		private GridView CreateModelGrid()
		{
			var gridView = new GridView
			{
				Style = DefaultGridStyle,

				// Help
				ToolTip = "Choose a model to join",
				Size = new Size(WindowWidth - 40, -1),
				DataStore = Model.SharedModels
			};
			gridView.ContextMenu = CreateContextMenu();

			gridView.Columns.Add(CreateOpenCell());
			gridView.Columns.Add(CreateTextCell());
			gridView.Columns.Add(CreateDividerCell());
			gridView.Columns.Add(CreateUserIconCell());
			gridView.Columns.Add(CreateCountCell());
			gridView.Columns.Add(CreateSignalCell());

			return gridView;
		}

		private ContextMenu? CreateContextMenu()
		{
			var menu = new ContextMenu();
			menu.Items.AddRange(new[] { MenuCommand("Remove", RemoveModel), MenuCommand("Join", JoinModel) });

			return menu;
		}

		private MenuItem MenuCommand(string label, EventHandler<EventArgs> eventHandler)
		{
			var button = new ButtonMenuItem
			{
				Text = label,
				ToolTip = $"{label} Item",
				Command = new Command(eventHandler)
			};

			return button;
		}

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
										Text = "Add",
										ToolTip = "Enter a new Shared Model Address",
										Command = new Command(AddNewModel) { CommandParameter = inputTextBox },
										Width = OpenCellWidth,
										Height = RowHeight
									},
									inputTextBox,
									new Button
									{
									Text = "Help",
									ToolTip = "Confused about Crash? Click here!",
									Command = new Command(OpenHelp),
									Width = OpenCellWidth,
									Height = RowHeight
									}
								},
				Size = new Size(-1, RowHeight),
				VerticalContentAlignment = VerticalAlignment.Top,
				HorizontalContentAlignment = HorizontalAlignment.Stretch
			};

			return addView;
		}

		private void OpenHelp(object sender, EventArgs args)
		{
			var discourseUrl = "http://crsh.cloud/docs/";
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(discourseUrl) { UseShellExecute = true });
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
				VerticalAlignment = VerticalAlignment.Center
			};

			return new GridColumn { Width = TextCellWidth, Resizable = false, Editable = true, DataCell = addressCell };
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
				DataCell = new ImageViewCell { Binding = Binding.Property<SharedModel, Image>(s => null) }
			};
		}

		private GridColumn CreateCountCell()
		{
			return new GridColumn
			{
				Width = CountCellWidth,
				Resizable = false,
				DataCell = new TextBoxCell
				{
					Binding = Binding.Property<SharedModel, string>(s => "") //s => s.UserCount)
				}
			};
		}

		private GridColumn CreateSignalCell()
		{
			return new GridColumn
			{
				Width = SignalCellWidth,
				Resizable = false,
				DataCell = new ImageViewCell { Binding = Binding.Property<SharedModel, Image>(s => null) }
			};
		}
	}
}
