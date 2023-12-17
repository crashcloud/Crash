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
		private readonly CrashDoc _crashDoc;
		private readonly UsersViewModel _viewModel;

		private UsersForm(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
			_viewModel = new UsersViewModel(crashDoc);
			CreateForm();

			RhinoDoc.ActiveDocumentChanged += (_, _) => { Close(); };
		}

		private static UsersForm? ActiveForm { get; set; }

		internal static void ShowForm(CrashDoc crashDoc)
		{
			if (ActiveForm is not null)
			{
				return;
			}

			var form = new UsersForm(crashDoc);
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
			try
			{
				ActiveForm?.Invalidate(true);
			}
			catch
			{
				// ignored
			}
		}

		private void CreateForm()
		{
			Icon = Icons.crashlogo.ToEto();
			Size = new Size(200, -1);
			Title = "Collaborators";
			Owner = RhinoEtoApp.MainWindow;
			Padding = 0;
			Topmost = false;
			AutoSize = true;
			Resizable = false;
			Maximizable = false;
			Minimizable = false;
			WindowStyle = WindowStyle.Default;
			ShowInTaskbar = false;

#if NET7_0
			this.UseRhinoStyle();
#endif

			var gridView = new GridView
			               {
				               AllowMultipleSelection = false,
				               AllowEmptySelection = true,
				               DataStore = _viewModel.Users,
				               ShowHeader = false,
				               Border = BorderType.None,
				               RowHeight = 24,
				               Columns =
				               {
					               CreateCameraColumn(),
					               CreateVisibleColumn(),
					               CreateColourColumn(),
					               CreateUsersColumn()
				               }
			               };

			gridView.CellClick += _viewModel.CycleCameraSetting;
			Content = gridView;
		}

		private static GridColumn CreateUsersColumn()
		{
			return new GridColumn
			       {
				       DataCell = new TextBoxCell
				                  {
					                  Binding =
						                  Binding.Property<UserObject, string>(u => u.Name)
				                  },
				       AutoSize = true,
				       MinWidth = 120,
				       Editable = false,
				       HeaderText = "Name",
				       Resizable = false
			       };
		}

		private static GridColumn CreateColourColumn()
		{
			var cell = new DrawableCell();
			cell.Paint += DrawColourCircle;
			var colourColumn = new GridColumn
			                   {
				                   DataCell = cell,
				                   AutoSize = true,
				                   Editable = false,
				                   HeaderText = "",
				                   Resizable = false,
				                   Width = 24
			                   };

			return colourColumn;
		}

		private GridColumn CreateVisibleColumn()
		{
			return new GridColumn
			       {
				       DataCell = new CheckBoxCell { Binding = Binding.Property<UserObject, bool?>(uo => uo.Visible) },
				       AutoSize = true,
				       Editable = true,
				       HeaderText = "",
				       Resizable = false,
				       Width = 24
			       };
		}

		private static GridColumn CreateCameraColumn()
		{
			return new GridColumn
			       {
				       DataCell = new ImageViewCell
				                  {
					                  Binding =
						                  Binding.Property<UserObject, Image>(u =>
							                                                      UsersViewModel.GetCameraImage(u))
				                  },
				       Editable = false,
				       HeaderText = "",
				       Resizable = false,
				       Width = 30
			       };
		}

		private static void DrawColourCircle(object? sender, CellPaintEventArgs e)
		{
			if (e.Item is not UserObject user)
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
