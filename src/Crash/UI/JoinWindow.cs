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

			// ActiveForm = this;
		}

		private IEnumerable<object> GetUsers()
		{
			yield return new SharedModel()
			{
				ModelAddress = "http://localhost:8080",
				Users = new string[] { "Callum" }
			};
		}

		private Cell CreateOpenButton()
		{
			var openCell = new CustomCell();
			openCell.CreateCell = CreateOpenButtonContents;
			openCell.Paint += OpenCell_Paint;

			return openCell;
		}

		private void OpenCell_Paint(object sender, CellPaintEventArgs e)
		{
			;

		}

		private Control CreateOpenButtonContents(CellEventArgs args)
		{
			int row = args.Row;
			var val = (args.Grid as GridView).DataStore.ToArray()[row];
			var model = val as SharedModel;

			var button = new Button()
			{
				Text = "+",
				ToolTip = "Add a new Shared Model",
				// Command = new Command(AddNewModel) { DataContext = model },
				Enabled = true // model.Loaded == true,
			};

			return button;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			JoinWindow window = sender as JoinWindow;
			// Get Selected or whatever
		}

	}
}
