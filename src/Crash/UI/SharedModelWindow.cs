using System.Runtime.InteropServices;

using Eto.Drawing;
using Eto.Forms;

using static Crash.UI.SharedModelViewModel;

namespace Crash.UI
{
	/*
	[Guid("37943c4b-5c30-471c-a5b0-c1bdaafa628d")]
	public partial class SharedModelWindow : Form
	{

		internal SharedModelViewModel viewModel;
		internal Eto.Forms.Dialog<SharedModel> Dialog;

		public EventHandler<EventArgs> Clicked;
		public EventHandler<EventArgs> AddNewModel;

		private GridView sharedModelsView;
		private GridView newModelView;

		private int sharedModelHeight
		{
			get
			{
				try
				{
					return (viewModel.SharedModels.Count + 1) * 25;
				}
				catch
				{
					return 0;
				}
			}
		}

		private void CreateContextMenu(GridView gridView)
		{
			ContextMenu = new ContextMenu();

			var nb = new ButtonMenuItem();

			MenuItem[] menuItems = new MenuItem[]
			{
				new ButtonMenuItem()
				{
					Text = "Open Model",
					Command = new Command((sender, args) =>
					{
						;

					}),
					ToolTip = "Open a Shared Model (Open button must be enabled)"
				},
				new ButtonMenuItem()
				{
					Text = "Remove Model",
					Command = new Command((sender, args) =>
					{
						var sharedModel = gridView.SelectedItem as SharedModel;
						this.viewModel.SharedModels.Remove(sharedModel);
						gridView.ReloadData(gridView.SelectedRow);
					}),
					ToolTip = "Removes a model from the list (Does not actually delete it from the server)"
				},
				new ButtonMenuItem()
				{
					Text = "Refresh Model",
					Command = new Command((sender, args) =>
					{
						var sharedModel = gridView.SelectedItem as SharedModel;
						sharedModel.LoadModel();
						gridView.Invalidate(true);
					}),
					ToolTip = "Tests the connection to the model again"
				},
			};
			ContextMenu.Items.AddRange(menuItems);
			// ContextMenu.DataContext = new ObservableCollection<SharedModel>(viewModel.SharedModels);

			gridView.ContextMenu = ContextMenu;
		}

		private void ResizeView(GridView gridView)
		{
			gridView.Height = sharedModelHeight;
			// Dialog.Size = new Size(Dialog.Width, sharedModelHeight + 70);

			// Dialog.Invalidate(true);
			// gridView.Invalidate(true);
		}

		void InitializeComponent()
		{
			Padding = 0;

			sharedModelsView = CreateGridView();
			sharedModelsView.DataStore = viewModel.SharedModels;
			ResizeView(sharedModelsView);
			CreateContextMenu(sharedModelsView);

			Func<CellEventArgs, Control> openSharedFunc = (args) =>
			{
				int row = args.Row;
				var val = (args.Grid as GridView).DataStore.ToArray()[row];
				var model = val as SharedModel;

				var button = new Button()
				{
					Text = "Open",
					ToolTip = "Open the Shared Model",
					Command = new Command(Clicked) { DataContext = model },
					Enabled = model.Loaded == true,
				};

				return button;
			};

			CreateColumns(sharedModelsView, openSharedFunc, false);


			newModelView = CreateGridView();
			newModelView.DataStore = new viewModel.AddModels;
			newModelView.Height = sharedModelsView.RowHeight;
			newModelView.KeyDown += (sender, args) =>
			{
				// Get Cell
				// addNewSharedFunc()
			};
			// newModelView.GetCellAt(new PointF(6, 0))

			Func<CellEventArgs, Control> addNewSharedFunc = (args) =>
			{
				int row = args.Row;
				var val = (args.Grid as GridView).DataStore.ToArray()[row];
				var model = val as SharedModel;

				var button = new Button()
				{
					Text = "+",
					ToolTip = "Add a new Shared Model",
					Command = new Command(AddNewModel) { DataContext = model },
					Enabled = model.Loaded == true,
				};

				return button;
			};

			CreateColumns(newModelView, addNewSharedFunc, true);

			Content = new StackLayout
			{
				Padding = new Padding(4, 4),
				Spacing = 0,
				Width = 360,
				Size = new Size(360, -1),
				MinimumSize = new Size(240, 60),
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Top,
				Items =
				{
					sharedModelsView,
					newModelView,
				},
			};

			viewModel.OnLoaded += (sender, args) =>
			{
				sharedModelsView.Invalidate(true);
				Content.Invalidate(true);
			};

		}

		const string BUTTON_STYLE = "button_style";
		internal SharedModelWindow(SharedModelViewModel viewModel, Eto.Forms.Dialog<SharedModel> dialog)
		{
			this.viewModel = viewModel;
			// this.Dialog = dialog;

			InitializeComponent();
			Clicked += (sender, args) =>
			{
				var model = (sender as Command)?.DataContext as SharedModel;
				var parent = this.Parent as Eto.Forms.Dialog<SharedModel>;
				parent.Close(model);
			};

			AddNewModel += (sender, args) =>
			{
				viewModel.AddSharedModel(new SharedModel
				{
					Loaded = false,
					ModelAddress = viewModel.AddModels.First().ModelAddress,
					ViewModel = viewModel
				});

				viewModel.AddModels.Clear();
				viewModel.AddModels.Add(new SharedModel() { Loaded = true, ModelAddress = "" });

				ResizeView(sharedModelsView);
				// Dialog.Invalidate(true);
				// sharedModelsView.Invalidate(true);
			};

			// this.KeyDown += (sender, args) => { };
			this.UnLoad += (sender, args) =>
			{
				viewModel.SaveSharedModels(null, null);
			};
		}

		private GridView CreateGridView() => new GridView()
		{
			ShowHeader = false,
			RowHeight = 25,
			AllowColumnReordering = false,
			AllowMultipleSelection = false,
			Border = BorderType.None,
			GridLines = GridLines.Horizontal,
			// ToolTip = "Open a Shared Model"
			AllowDrop = false,
			AllowEmptySelection = false,
			// Size = new Size(-1, -1),
			Height = sharedModelHeight
		};

		private void CreateColumns(GridView gridView, Func<CellEventArgs, Control> createCell, bool editable)
		{
			gridView.ShowHeader = false;
			gridView.RowHeight = 25;

			CreateButton(gridView, createCell);

			CreateAddressTextBox(gridView, editable);

			var dividerCell = new DrawableCell();
			dividerCell.Paint += (sender, args) =>
			{
				args.Graphics.DrawLine(new Color(125, 125, 125), new PointF(5, 4), new PointF(5, 16));
			};
			gridView.Columns.Add(new GridColumn()
			{
				Width = 10,
				Resizable = false,
				DataCell = dividerCell,
			});
			gridView.Columns.Add(new GridColumn()
			{
				Width = 30,
				Resizable = false,
				DataCell = new ImageViewCell { Binding = Binding.Property<SharedModel, Image>(s => s.UserIcon) },
			});
			gridView.Columns.Add(new GridColumn()
			{
				Width = 30,
				Resizable = false,
				DataCell = new TextBoxCell { Binding = Binding.Property<SharedModel, string>(s => s.UserCount) }
			});
			gridView.Columns.Add(new GridColumn()
			{
				Width = 30,
				Resizable = false,
				DataCell = new ImageViewCell { Binding = Binding.Property<SharedModel, Image>(s => s.Signal) },
			});
		}

		private void CreateButton(GridView gridView, Func<CellEventArgs, Control> createCell)
		{
			var openCell = new CustomCell();
			openCell.CreateCell = createCell;
			gridView.Columns.Add(new GridColumn()
			{
				Width = 60,
				Resizable = false,
				DataCell = openCell,
			});
		}

		private void CreateAddressTextBox(GridView gridView, bool editable = false)
		{
			var binding = Binding.Property<SharedModel, string>(s => s.ModelAddress);
			binding.Changed += (sender, args) =>
			{
				if (!editable) return;
				// addModel.Loaded = !string.IsNullOrEmpty(args.Value as string);
			};

			gridView.Columns.Add(new GridColumn()
			{
				Width = 240,
				Resizable = false,
				Editable = editable,
				HeaderTextAlignment = TextAlignment.Left,
				// CellToolTipBinding,
				DataCell = new TextBoxCell { Binding = binding },
			});
		}

	}
	*/
}
