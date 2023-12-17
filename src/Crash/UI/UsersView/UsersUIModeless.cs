using System.ComponentModel;
using System.Runtime.InteropServices;

using Crash.Common.Document;
using Crash.Properties;

using Eto.Drawing;
using Eto.Forms;

using Rhino.UI;

using Color = System.Drawing.Color;

namespace Crash.UI.UsersView
{
	internal sealed class UsersForm : Form
	{
		private readonly UsersViewModel _viewModel;
		private GridView _mGrid;

		private UsersForm(CrashDoc crashDoc)
		{
			Owner = RhinoEtoApp.MainWindow;
			RhinoDoc.ActiveDocumentChanged += (sender, args) => { Close(); };

#if NET7_0
			this.UseRhinoStyle();
#endif

			_viewModel = new UsersViewModel(crashDoc);
			CreateForm();
			Icon = Icons.crashlogo.ToEto();
			Padding = 0;
			// MinimumSize = new Size(100, -1);
			Size = new Size(200, -1);
		}

		private static UsersForm? ActiveForm { get; set; }

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

		internal static void CloseActiveForm()
		{
			ActiveForm?.Close();
			ActiveForm = null;
		}

		internal static void ReDraw()
		{
			ActiveForm ??= new UsersForm();

			try
			{
				ActiveForm._mGrid.Invalidate(true);
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

		private void CreateForm()
		{
			Maximizable = false;
			Minimizable = false;
			Padding = 0;
			Resizable = false;
			AutoSize = true;
			ShowInTaskbar = false;
			Title = "Collaborators";
			WindowStyle = WindowStyle.Default;
			Size = new Size(120, -1);

			var rowHeight = 24;

			_mGrid = new GridView
			         {
				         AllowMultipleSelection = false,
				         DataStore = _viewModel.Users,
				         ShowHeader = false,
				         Border = BorderType.None,
				         AllowEmptySelection = true,
				         RowHeight = rowHeight
			         };

			_viewModel._view = _mGrid;
			_mGrid.CellClick += _viewModel.CycleCameraSetting;

			// Camera
			_mGrid.Columns.Add(new GridColumn
			                   {
				                   DataCell = new ImageViewCell
				                              {
					                              Binding =
						                              Binding.Property<UsersViewModel.UserObject, Image>(u =>
							                              UsersViewModel.UserUIExtensions
							                                            .GetCameraImage(u));
				                              },
				                   Editable = false,
				                   HeaderText = "",
				                   Resizable = false,
				                   Width = rowHeight + 6
			                   });

			// Visible
			_mGrid.Columns.Add(new GridColumn
			                   {
				                   DataCell = new CheckBoxCell
				                              {
					                              Binding =
						                              Binding.Property<UsersViewModel.UserObject, bool?>(u => u.Visible)
				                              },
				                   AutoSize = true,
				                   Editable = true,
				                   HeaderText = "",
				                   Resizable = false,
				                   Width = rowHeight
			                   });

			var cell = new DrawableCell();
			cell.Paint += DrawColourCircle;

			// Colours
			_mGrid.Columns.Add(new GridColumn
			                   {
				                   DataCell = cell,
				                   AutoSize = true,
				                   Editable = false,
				                   HeaderText = "",
				                   Resizable = false,
				                   Width = rowHeight
			                   });

			// User
			_mGrid.Columns.Add(new GridColumn
			                   {
				                   DataCell = new TextBoxCell
				                              {
					                              Binding =
						                              Binding.Property<UsersViewModel.UserObject, string>(u => u.Name)
				                              },
				                   AutoSize = true,
				                   MinWidth = 120,
				                   Editable = false,
				                   HeaderText = "Name",
				                   Resizable = false
			                   });

			var userLayout = new TableLayout { Rows = { new TableRow(null, _mGrid, null) } };
			Content = new TableLayout { Rows = { new TableRow(userLayout) } };
		}

		private static void DrawColourCircle(object sender, CellPaintEventArgs e)
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
