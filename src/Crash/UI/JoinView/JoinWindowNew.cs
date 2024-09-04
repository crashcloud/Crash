using Crash.Handlers.Data;

using Eto.Drawing;
using Eto.Forms;

using Rhino.UI;

namespace Crash.UI.JoinView
{
	internal class JoinWindowNew : Dialog<SharedModel>
	{

		private Control ModelList { get; set; }
		private Control AddModelBar { get; set; }

		private JoinViewModel Model => DataContext as JoinViewModel;

		public JoinWindowNew()
		{
			Size = new Size(600, 300);
			DataContext = new JoinViewModel();
			InitLayout();
			InitBindings();
			this.UseRhinoStyle();
		}

		private void InitLayout()
		{
			ModelList = InitModelList();
			AddModelBar = InitModelBar();

			var layout = new DynamicLayout()
			{
				Spacing = new Size(0, 0),
				Padding = 0,
			};
			layout.BeginVertical();
			layout.Add(ModelList, true, true);
			layout.AddRow(AddModelBar);
			layout.EndVertical();

			Content = layout;
		}

		private Control InitModelBar()
		{
			var textInput = new TextBox()
			{
				AllowDrop = false,
				TextAlignment = TextAlignment.Center,
				PlaceholderText = "Enter model address",
			};
			textInput.TextChanged += (s, e) =>
			{
				Model.TemporaryModel ??= new SharedModel();
				Model.TemporaryModel.ModelAddress = textInput.Text;
			};
			textInput.KeyDown += (s, e) =>
			{
				if (e.Key != Keys.Enter) return;
				if (Model?.TemporaryModel is null) return;
				if (!Model.AddSharedModel(Model.TemporaryModel)) return;
				Model.TemporaryModel = new SharedModel();
			};

			var layout = new DynamicLayout()
			{
				Height = 26,
			};
			layout.BeginHorizontal();
			var addButton = GetButton("Add ↗", new Size(100, layout.Height));
			addButton.MouseDown += (s, e) =>
			{
				if (Model?.TemporaryModel is null) return;
				if (!Model.AddSharedModel(Model.TemporaryModel)) return;
				Model.TemporaryModel = new SharedModel();
			};
			layout.Add(addButton);
			layout.Add(textInput, true, false);
			var helpButton = GetButton("Help", new Size(100, layout.Height));
			helpButton.MouseDown += (s, e) =>
			{
				var discourseUrl = "http://crsh.cloud/docs/";
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(discourseUrl) { UseShellExecute = true });
			};
			layout.Add(helpButton);
			layout.EndHorizontal();

			return layout;
		}

		private void InitBindings()
		{

		}

		private Control InitModelList()
		{
			int height = 60;
			var layout = new GridView()
			{
				Height = -1,
				ShowHeader = false,
				DataStore = Model.SharedModels,
				RowHeight = height,
				AllowColumnReordering = false,
				AllowMultipleSelection = false,
				AllowDrop = false,
				AllowEmptySelection = false,
			};

			var buttonCell = new CustomCell();
			buttonCell.CreateCell = (args) =>
			{
				var button = GetButton("Join", new Size(100, height));
				button.MouseDown += (s, e) =>
				{
					if (args.Item is not SharedModel sharedModel) return;
					Close(sharedModel);
				};
				return button;
			};

			layout.Columns.Add(new GridColumn()
			{
				HeaderText = "Join Button",
				DataCell = buttonCell,
				Width = 100,
				AutoSize = false,
			});
			layout.Columns.Add(new GridColumn()
			{
				HeaderText = "Model Address",
				DataCell = new TextBoxCell()
				{
					TextAlignment = TextAlignment.Center,
					Binding = Binding.Property<SharedModel, string>(r => r.ModelAddress)
				},
				Width = 400,
				AutoSize = false,
			});
			layout.Columns.Add(new GridColumn()
			{
				HeaderText = "Model Image",
				DataCell = GetModelData(),
				Width = 100,
				AutoSize = false,
			});

			var scrollable = new Scrollable()
			{
				Border = BorderType.None,
				ExpandContentHeight = true,
				ExpandContentWidth = false,
				Width = -1,
			};

			scrollable.Content = layout;

			return layout;
		}

		private Cell GetModelData()
		{
			var cell = new CustomCell();
			cell.CreateCell = (args) =>
			{
				var layout = new Drawable();
				layout.Size = new Size(100, -1);

				layout.Paint += (s, e) =>
				{
					if (layout.DataContext is not SharedModel sharedModel) return;
					if (sharedModel.Thumbnail is null) return;

					float x = (100 - layout.Size.Width) / 2;
					float y = -(100 - layout.Size.Height) / 2;
					e.Graphics.DrawImage(sharedModel.Thumbnail, x, y, 100, 100);
				};

				return layout;
			};

			return cell;
		}

		private Control GetModelRow(string url)
		{
			int height = 26;
			var textInput = new TextBox()
			{
				AllowDrop = false,
				TextAlignment = TextAlignment.Center,
				PlaceholderText = "Enter model address",
				Text = "url",
				Height = height,
			};
			var layout = new DynamicLayout()
			{
				Height = height,
			};
			layout.BeginHorizontal();
			layout.Add(GetButton("Join ->", new Size(100, height)));
			layout.Add(textInput, true, false);
			layout.EndHorizontal();

			return layout;
		}

		private Drawable GetButton(string text, Size size)
		{
			var button = new Drawable();
			button.Size = size;
			button.Paint += (s, e) =>
			{
				var rect = new RectangleF(0, 0, 100, button.Size.Height);
				e.Graphics.FillRectangle(Palette.Blue, rect);

				var font = new Font(SystemFont.Default, 12);
				var size = e.Graphics.MeasureString(font, text);
				var textRect = new RectangleF((button.Size.Width - size.Width) / 2, (button.Size.Height - size.Height) / 2, size.Width, size.Height);
				e.Graphics.DrawText(font, new SolidBrush(Palette.White), textRect, text);

			};

			return button;
		}

	}
}
