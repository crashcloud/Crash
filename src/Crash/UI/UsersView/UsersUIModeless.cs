using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

using Crash.Properties;

using Eto.Forms;

using Rhino.UI;

using Pen = Eto.Drawing.Pen;
using RectangleF = Eto.Drawing.RectangleF;
using Size = Eto.Drawing.Size;

namespace Crash.UI.UsersView
{
	internal sealed class UsersForm : Form
	{
		private readonly UsersViewModel ViewModel;
		private GridView m_grid;

		internal UsersForm()
		{
			Owner = RhinoEtoApp.MainWindow;
			ViewModel = new UsersViewModel();
			CreateForm();
			Icon = Icons.crashlogo.ToEto();
			BackgroundColor = Color.White.ToEto();
			Padding = 0;
			MinimumSize = new Size(120, 40);
			// Size = new Size(240, 80); // Required to make UI smaller on windows, but prevents resizing later
		}

		internal static UsersForm? ActiveForm { get; set; }

		internal static void ShowForm()
		{
			if (ActiveForm is not null)
			{
				return;
			}

			var form = new UsersForm();
			form.Closed += OnFormClosed;
			form.Show();
			form.BringToFront();

			ActiveForm = form;
		}

		internal static void ToggleFormVisibility()
		{
			if (ActiveForm is null)
			{
				ShowForm();
			}
			else
			{
				ActiveForm = null;
			}
		}

		internal static void CloseActiveForm()
		{
			ActiveForm?.Close();
			ActiveForm = null;
		}

		internal static void ReDraw()
		{
			if (ActiveForm is null)
			{
				ActiveForm = new UsersForm();
			}

			try
			{
				ActiveForm.m_grid.Invalidate(true);
				ActiveForm.Invalidate(true);
			}
			catch { }
		}

		/// <summary>
		///     FormClosed EventHandler
		/// </summary>
		private static void OnFormClosed(object sender, EventArgs e)
		{
			ActiveForm?.Dispose();
			ActiveForm = null;
		}

		private void ReDrawEvent(object sender, EventArgs e)
		{
			ReDraw();
		}

		private void CreateForm()
		{
			Maximizable = false;
			Minimizable = false;
			Padding = 0;
			Resizable = false;
			ShowInTaskbar = false;
			Title = "Collaborators";
			WindowStyle = WindowStyle.Default;
			MinimumSize = new Size(20, 50);

			m_grid = new GridView
			         {
				         AllowMultipleSelection = false,
				         DataStore = ViewModel.Users,
				         ShowHeader = false,
				         Border = BorderType.None,
				         AllowEmptySelection = true,
				         RowHeight = 24
			         };

			ViewModel.View = m_grid;
			m_grid.CellClick += ViewModel.CycleCameraSetting;
			m_grid.DataContextChanged += M_grid_DataContextChanged;

			// TODO : Implement Sorting
			// m_grid.ColumnHeaderClick += M_grid_ColumnHeaderClick;

			// Camera
			var ivc = new ImageViewCell();

			m_grid.Columns.Add(new GridColumn
			                   {
				                   DataCell = new ImageViewCell { Binding = ViewModel.ImageCellBinding },
				                   Editable = false,
				                   HeaderText = "",
				                   Resizable = false,
				                   Sortable = false,
				                   Width = 30
			                   });

			// Visible
			m_grid.Columns.Add(new GridColumn
			                   {
				                   DataCell = new CheckBoxCell { Binding = ViewModel.VisibleCellBinding },
				                   AutoSize = true,
				                   Editable = true,
				                   HeaderText = "",
				                   Resizable = false,
				                   Sortable = false,
				                   Width = 24
			                   });

			var cell = new DrawableCell();
			cell.Paint += Cell_Paint;

			// Colours
			m_grid.Columns.Add(new GridColumn
			                   {
				                   DataCell = cell,
				                   AutoSize = true,
				                   Editable = false,
				                   HeaderText = "",
				                   Resizable = false,
				                   Sortable = false,
				                   Width = 24
			                   });

			// User
			m_grid.Columns.Add(new GridColumn
			                   {
				                   DataCell = new TextBoxCell { Binding = ViewModel.TextCellBinding },
				                   AutoSize = true,
				                   MinWidth = 120,
				                   Editable = false,
				                   HeaderText = "Name",
				                   Resizable = false,
				                   Sortable = true
			                   });

			var user_layout = new TableLayout
			                  {
				                  // Padding = new Padding(5, 10, 5, 5),
				                  // Spacing = new Size(5, 5),
				                  Rows = { new TableRow(null, m_grid, null) }
			                  };

			Content = new TableLayout
			          {
				          // Padding = new Padding(5),
				          // Spacing = new Size(5, 5),
				          Rows = { new TableRow(user_layout) }
			          };
		}

		private void M_grid_DataContextChanged(object sender, EventArgs e)
		{
			;
		}

		private void Cell_Paint(object sender, CellPaintEventArgs e)
		{
			if (e.Item is not UsersViewModel.UserObject user)
			{
				return;
			}

			var bottomOffset = 4;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				bottomOffset = 2;
			}

			e.Graphics.FillEllipse(user.Colour.ToEto(), new RectangleF(bottomOffset, bottomOffset, 16, 16));
			e.Graphics.DrawEllipse(new Pen(Color.Black.ToEto()), new RectangleF(bottomOffset, bottomOffset, 16, 16));
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			this.SavePosition();
			base.OnClosing(e);
		}
	}
}
