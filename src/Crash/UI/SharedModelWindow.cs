using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Crash.Properties;

using Eto.Drawing;
using Eto.Forms;

using Rhino.UI;

using static Crash.UI.SharedModelViewModel;

namespace Crash.UI
{

	[Guid("37943c4b-5c30-471c-a5b0-c1bdaafa628d")]
	public partial class SharedModelWindow : Panel
	{

		internal SharedModelViewModel Model;

		public EventHandler<EventArgs> Clicked;

		void InitializeComponent()
		{
			Padding = 0;

			Model = new();

			GridView view = new GridView()
			{
				DataStore = new ObservableCollection<SharedModel>(Model.SharedModels),
				ShowHeader = false,
				RowHeight = 25,
			};

			var openCell = new CustomCell();
			openCell.CreateCell = (args) =>
			{
				;

				int row = args.Row;
				var val = (args.Grid as GridView).DataStore.ToArray()[row];
				var model = val as SharedModel;

				return new Button()
				{
					Text = "Open",
					Size = new Size(12, 20),
					ToolTip = "Open the Shared Model",
					Command = new Command(Clicked) { DataContext = model }
				};
			};

			view.Columns.Add(new GridColumn()
			{
				Width = 60,
				Resizable = false,
				DataCell = openCell,
			});
			view.Columns.Add(new GridColumn()
			{
				Width = 240,
				Resizable = false,
				Editable = false,
				DataCell = new TextBoxCell { Binding = Binding.Property<SharedModel, string>(s => s.ModelAddress) }
			});
			var dividerCell = new DrawableCell();
			dividerCell.Paint += (sender, args) =>
			{
				args.Graphics.DrawLine(new Color(125, 125, 125), new PointF(5, 4), new PointF(5, 16));
			};
			view.Columns.Add(new GridColumn()
			{
				Width = 10,
				Resizable = false,
				DataCell = dividerCell,
			});
			view.Columns.Add(new GridColumn()
			{
				Width = 30,
				Resizable = false,
				DataCell = new ImageViewCell { Binding = Binding.Property<SharedModel, Image>(s => s.UserIcon) },
			});
			view.Columns.Add(new GridColumn()
			{
				Width = 30,
				Resizable = false,
				DataCell = new TextBoxCell { Binding = Binding.Property<SharedModel, string>(s => s.UserCount) }
			});
			view.Columns.Add(new GridColumn()
			{
				Width = 30,
				Resizable = false,
				DataCell = new ImageViewCell { Binding = Binding.Property<SharedModel, Image>(s => s.Signal) }
			});

			Content = new StackLayout
			{
				Padding = 0,
				Spacing = 0,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Top,
				Items =
				{
					view,
				},
			};

			Model.OnLoaded += (sender, args) =>
			{
				view.Invalidate(true);
				Content.Invalidate(true);
			};

		}

		public SharedModelWindow()
		{
			InitializeComponent();
			Clicked += (sender, args) =>
			{
				var model = (sender as Command)?.DataContext as SharedModel;
				var parent = this.Parent as Eto.Forms.Dialog<SharedModel>;
				parent.Close(model);
			};
		}
	}

}
