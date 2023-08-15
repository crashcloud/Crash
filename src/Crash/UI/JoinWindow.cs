using System;

using Eto.Forms;
using Eto.Drawing;

using Rhino.UI;
using Crash.Properties;
using static Crash.UI.SharedModelViewModel;

namespace Crash.UI
{
	public partial class JoinWindow : Form, IDisposable
	{

		// internal JoinViewModel ViewModel { get; set; }

		internal static JoinWindow ActiveForm;

		public JoinWindow()
		{
			Title = "Join Shared Model";
			Resizable = false;
			Minimizable = false;
			AutoSize = true;
			Padding = 0;
			Icon = Icons.crashlogo.ToEto();
			ShowActivated = true;

			Users = GetUsers();
			InitializeComponent();
			CreateBindings();

			// ActiveForm = this;
		}

		private IEnumerable<object> GetUsers()
		{
			yield break;

			yield return new SharedModel()
			{
				ModelAddress = "http://localhost:8080",
				Loaded = true
			};
		}

		private void CreateBindings()
		{
			OpenCell.DataCell = CreateOpenButton();
			TextCell.DataCell = new TextBoxCell
			{
				Binding = Binding.Property<SharedModel, string>(s => s.ModelAddress)
			};

			UserIconCell.DataCell = new ImageViewCell
			{
				Binding = Binding.Property<SharedModel, Image>(s => s.UserIcon)
			};

			UserCountCell.DataCell = new TextBoxCell
			{
				Binding = Binding.Property<SharedModel, string>(s => s.UserCount)
			};

			SignalCell.DataCell = new ImageViewCell
			{
				Binding = Binding.Property<SharedModel, Image>(s => s.Signal)
			};
		}

		private Cell CreateOpenButton()
		{
			Func<CellEventArgs, Control> addNewSharedFunc = (args) =>
			{
				int row = args.Row;
				var val = (args.Grid as GridView).DataStore.ToArray()[row];
				// var model = val as SharedModel;

				var button = new Button()
				{

					Text = "+",
					ToolTip = "Add a new Shared Model",
					// Command = new Command(AddNewModel) { DataContext = model },
					Enabled = true // model.Loaded == true,
				};

				return button;
			};

			var openCell = new CustomCell();
			openCell.CreateCell = addNewSharedFunc;

			return openCell;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			JoinWindow window = sender as JoinWindow;
			// Get Selected or whatever
		}

	}
}
