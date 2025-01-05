using System.ComponentModel;
using System.Runtime.InteropServices;

using Crash.Common.App;
using Crash.Common.Document;
using Crash.Handlers;
using Crash.Resources;

using Eto.Drawing;
using Eto.Forms;

using Rhino.Runtime;

using Rhino.UI;

using Color = System.Drawing.Color;

namespace Crash.UI.UsersView
{
	internal sealed class UsersForm : Form, ICrashInstance
	{
		private CrashDoc _crashDoc { get; }
		private UsersViewModel Model => DataContext as UsersViewModel;

		private UsersForm(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
			var model = new UsersViewModel(crashDoc);
			model.OnInvalidate += (sender, args) =>
									   {
										   Invalidate(true);
									   };

			DataContext = model;
			CreateForm();
		}

		protected override void OnClosed(EventArgs e)
		{
			CrashInstances.RemoveInstance(_crashDoc, typeof(UsersForm));
			base.OnClosed(e);
		}

		protected override void OnShown(EventArgs e)
		{
			if (!CrashInstances.TryGetInstance<UsersForm>(_crashDoc, out var usersForm))
				CrashInstances.TrySetInstance(_crashDoc, this);

			base.OnShown(e);
		}

		internal static void ShowForm(CrashDoc crashDoc)
		{
			if (crashDoc is null) return;
			if (!CrashInstances.TryGetInstance<UsersForm>(crashDoc, out var usersForm))
			{
				usersForm = new UsersForm(crashDoc);
				usersForm.SavePosition();
				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);
				CrashInstances.TrySetInstance(crashDoc, usersForm);
				usersForm.Show(rhinoDoc);
				usersForm.RestorePosition();
			}

			usersForm.BringToFront();
		}

		internal static void CloseActiveForm(CrashDoc crashDoc)
		{
			if (!CrashInstances.TryGetInstance<UsersForm>(crashDoc, out var usersForm)) return;

			try
			{
				usersForm.Close();
				CrashInstances.RemoveInstance(crashDoc, typeof(UsersForm));
			}
			catch { }
		}

		internal static void ReDraw(CrashDoc crashDoc)
		{
			if (crashDoc is null) return;
			try
			{
				if (!CrashInstances.TryGetInstance(crashDoc, out UsersForm form)) return;

				form?.Invalidate(true);

				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);
				rhinoDoc?.Views.Redraw();
			}
			catch
			{
				// ignored
			}
		}

		private void CreateForm()
		{
			// TOOD : Loading .ico as png causes issue on windows
			// Icon = new Icon(1f, CrashIcons.Icon("logo", 16));
			Title = "Collaborators";
			Padding = HostUtils.RunningOnOSX ? 6 : 0;
			Topmost = false;
			AutoSize = false;
			Resizable = true;
			Maximizable = false;
			Minimizable = false;
			WindowStyle = WindowStyle.Default;
			ShowInTaskbar = false;

			var _rhinoDoc = CrashDocRegistry.GetRelatedDocument(_crashDoc);
			if (_rhinoDoc is not null)
				Owner = RhinoEtoApp.MainWindowForDocument(_rhinoDoc);

			SetSizeAndLocation();

#if NET7_0
			this.UseRhinoStyle();
#else
			// Rhino 7 Styling
#endif

			var gridView = new GridView
			{
				AllowMultipleSelection = false,
				AllowEmptySelection = true,
				DataStore = Model.Users,
				ShowHeader = false,
				Border = BorderType.None,
				RowHeight = 24,
				GridLines = GridLines.None,
				BackgroundColor = Colors.Transparent,
				CanDeleteItem = (_) => false,
				AllowColumnReordering = false,
				AllowDrop = false,
				Columns =
						{
							CreateCameraColumn(),
							CreateVisibleColumn(),
							CreateColourColumn(),
							CreateUsersColumn(),
						}
			};

			gridView.CellClick += Model.CycleCameraSetting;

			Content = gridView;
			/* Unsure why this was.
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				Content = new Scrollable { Content = gridView, ScrollSize = new Size(0, 0) };
			*/


			// We don't want focus by default on the Users View, we want it on Rhino so the user can click and pan.
			this.Shown += (sender, args) =>
						  {
							  RhinoApp.InvokeOnUiThread(() => Owner?.Focus());
						  };
		}

		private void SetSizeAndLocation()
		{
			MinimumSize = new Size(200, 40);
			Size = new Size(RecentModelDialog.PreviewWidth, 140);
			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(_crashDoc);
			if (rhinoDoc is null) return;

			try
			{
				Point point = new Point(0, 0);
				foreach (var view in rhinoDoc.Views)
				{
					if (view is null) continue;
					var rect = view.ScreenRectangle;
					if (point.X == 0 || point.X < rect.Right)
						point = new Point(rect.Right, point.Y);

					if (point.Y == 0 || point.Y > rect.Top)
						point = new Point(point.X, rect.Top);
				}

				int padding = 5;

				// Should only really work when there is no previous location
				Location = new Point(point.X - padding - Size.Width, point.Y + padding);
			}
			catch { }
		}

		private static GridColumn CreateUsersColumn()
		{
			return new GridColumn
			{
				DataCell = new TextBoxCell
				{
					Binding = Binding.Property<UserObject, string>(u => u.Name)
				},
				AutoSize = true,
				Editable = false,
				HeaderText = "Name",
				Resizable = false,
				Expand = true,
			};
		}

		private static GridColumn CreateColourColumn()
		{
			var cell = new DrawableCell();
			cell.Paint += DrawColourCircle;
			var colourColumn = new GridColumn
			{
				DataCell = cell,
				AutoSize = false,
				Editable = false,
				HeaderText = "",
				Resizable = false,
				Width = 24,
			};

			return colourColumn;
		}

		private GridColumn CreateVisibleColumn()
		{
			return new GridColumn
			{
				DataCell = new CheckBoxCell { Binding = Binding.Property<UserObject, bool?>(uo => uo.Visible) },
				AutoSize = false,
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
					Binding = Binding.Property<UserObject, Image>(u => u.Image)
				},
				AutoSize = false,
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
			CrashInstances.RemoveInstance(_crashDoc, typeof(UsersForm));
			base.OnClosing(e);
		}
	}
}
