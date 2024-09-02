using Eto.Drawing;
using Eto.Forms;

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
		}

		private void InitLayout()
		{
			ModelList = InitModelList();
			AddModelBar = InitModelBar();

			var layout = new DynamicLayout();
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
			var layout = new DynamicLayout()
			{
				Height = 26,
			};
			layout.BeginHorizontal();
			layout.Add(GetButton("Add ->", new Size(100, layout.Height)));
			layout.Add(textInput, true, false);
			layout.Add(GetButton("Helo ->", new Size(100, layout.Height)));
			layout.EndHorizontal();

			return layout;
		}

		private void InitBindings()
		{

		}

		private Control InitModelList()
		{
			var layout = new GridView()
			{
				Height = -1,
				ShowHeader = false,
				DataStore = Model.SharedModels,
			};

			var buttonCell = new CustomCell();
			buttonCell.CreateCell = (args) =>
			{
				var button = GetButton("Join", new Size(100, 26));
				button.MouseUp += (s, e) =>
				{
					// JoinModel(row);
				};
				return button;
			};

			layout.Columns.Add(new GridColumn()
			{
				HeaderText = "Join Button",
				DataCell = buttonCell,
			});
			layout.Columns.Add(new GridColumn()
			{
				HeaderText = "Model Address",
				DataCell = new TextBoxCell() { Binding = Binding.Property<SharedModel, string>(r => r.ModelAddress) },
			});
			layout.Columns.Add(new GridColumn()
			{
				HeaderText = "Model Stats",
				DataCell = GetModelData()
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
				layout.Size = new Size(100, 26);

				layout.Paint += (s, e) =>
				{
					if (layout.DataContext is not SharedModel sharedModel) return;

					e.Graphics.DrawImage(sharedModel.Thumbnail, new PointF(0, 0), new SizeF(100, 26));
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
				var rect = new RectangleF(0, 0, button.Size.Width, button.Size.Height);
				e.Graphics.FillRectangle(Colors.Blue, rect);

				var font = new Font(SystemFont.Default, 12);
				var size = e.Graphics.MeasureString(font, text);
				var textRect = new RectangleF((button.Size.Width - size.Width) / 2, (button.Size.Height - size.Height) / 2, size.Width, size.Height);
				e.Graphics.DrawText(font, new SolidBrush(Colors.White), textRect, text);

			};

			return button;
		}


	}
}
