using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Crash.Common.Document;
using Crash.Common.Tables;
using Crash.Handlers;
using Crash.Properties;

using Eto.Drawing;
using Eto.Forms;

using Rhino.UI;

using Color = System.Drawing.Color;

namespace Crash.UI.UsersView
{
	internal sealed class UsersViewModel : INotifyPropertyChanged
	{
		internal readonly IndirectBinding<Image> ImageCellBinding;

		internal readonly IndirectBinding<string> TextCellBinding;

		internal readonly IndirectBinding<bool?> VisibleCellBinding;

		internal GridView View;

		internal UsersViewModel()
		{
			RhinoDoc.ActiveDocumentChanged += (sender, args) => UsersForm.ReDraw();
			RhinoDoc.ActiveDocumentChanged += (sender, args) => UsersForm.ReDraw();

			ImageCellBinding = Binding.Property<UserObject, Image>(u => UserUIExtensions.GetCameraImage(u));
			// ImageCellBinding.Changed += ImageCellBinding_Changed;

			VisibleCellBinding = Binding.Property<UserObject, bool?>(u => u.Visible);
			VisibleCellBinding.Changed += VisibleCellBinding_Changed;
			;

			TextCellBinding = Binding.Property<UserObject, string>(u => u.Name);
			// TextCellBinding.Changed += TextCellBinding_Changed;

			SetUsers();

			PropertyChanged += ViewModel_PropertyChanged;
			UserTable.OnUserRemoved += UserTable_OnUserChanged;
			UserTable.OnUserAdded += UserTable_OnUserChanged;
		}

		internal ObservableCollection<UserObject> Users { get; set; }


		public event PropertyChangedEventHandler PropertyChanged;

		private void SetUsers()
		{
			if (CrashDocRegistry.ActiveDoc?.Users is IEnumerable<User> users)
			{
				Users = new ObservableCollection<UserObject>(users.Select(u => new UserObject(u)));
			}
			else
			{
				Users = new ObservableCollection<UserObject>();
			}
		}

		private void UserTable_OnUserChanged(object sender, UserEventArgs e)
		{
			Application.Instance.Invoke(() =>
			{
				try
				{
					SetUsers();
					UsersForm.ReDraw();
				}
				catch { }
			});
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(Users))
			{
				return;
			}

			UpdateCrashUserTable();
		}

		private void UpdateCrashUserTable()
		{
			var userTable = CrashDocRegistry.ActiveDoc.Users;
			foreach (var user in Users)
			{
				userTable.Update(user.CUser);
			}

			RhinoDoc.ActiveDoc.Views.Redraw();
			UsersForm.ReDraw();
		}

		private void VisibleCellBinding_Changed(object sender, BindingChangedEventArgs e)
		{
			NotifyPropertyChanged(nameof(Users));
		}

		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		internal void CycleCameraSetting(object sender, GridCellMouseEventArgs e)
		{
			var row = e.Row;
			var col = e.Column;

			if (row < 0 || col != 0)
			{
				return;
			}

			if (!e.Buttons.HasFlag(MouseButtons.Primary))
			{
				return;
			}

			if (e.Item is UserObject user)
			{
				var state = CycleState(user.Camera);

				if (state == CameraState.Follow)
				{
					for (var i = 0; i < Users.Count; i++)
					{
						var currUser = Users[i];
						if (CameraState.Follow == currUser.Camera)
						{
							currUser.Camera = CameraState.Visible;
							CrashDocRegistry.ActiveDoc.Users.Update(currUser.CUser);
						}
					}

					user.Camera = state;
					UpdateCrashUserTable();
				}

				user.Camera = state;
				UpdateCrashUserTable();
			}

			NotifyPropertyChanged(nameof(Users));
		}

		private CameraState CycleState(CameraState state)
		{
			var stateCount = (int)state;
			stateCount++;

			if (stateCount >= Enum.GetValues(typeof(CameraState)).Length)
			{
				return CameraState.None;
			}

			return (CameraState)stateCount;
		}

		public sealed class UserObject
		{
			internal UserObject(User user)
			{
				Name = user.Name;
				Colour = user.Color;
				Camera = user.Camera;
				Visible = user.Visible;
			}

			public string Name { get; }
			public Color Colour { get; private set; }
			public CameraState Camera { get; set; }
			public bool Visible { get; set; }

			internal User CUser => new(Name) { Camera = Camera, Visible = Visible };

			public override string ToString()
			{
				return Name;
			}
		}


		internal static class UserUIExtensions
		{
			private static readonly Dictionary<CameraState, Image> cameras;

			static UserUIExtensions()
			{
				cameras = new Dictionary<CameraState, Image>
						  {
							  { CameraState.None, Icons.CameraNone.ToEto() },
							  { CameraState.Visible, Icons.CameraVisible.ToEto() },
							  { CameraState.Follow, Icons.CameraFollow.ToEto() }
						  };
			}

			internal static Image GetCameraImage(UserObject user)
			{
				return cameras[user.Camera];
			}
		}
	}
}
