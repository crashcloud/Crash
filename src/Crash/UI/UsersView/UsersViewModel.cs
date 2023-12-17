using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

using Crash.Common.Document;
using Crash.Common.Tables;
using Crash.Properties;

using Eto.Forms;

using Rhino.UI;

using Image = Eto.Drawing.Image;

namespace Crash.UI.UsersView
{
	internal sealed class UsersViewModel : INotifyPropertyChanged
	{
		private readonly CrashDoc _crashDoc;
		internal GridView _view;

		internal UsersViewModel(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
			SetUsers();

			PropertyChanged += ViewModel_PropertyChanged;
			UserTable.OnUserRemoved += UserTable_OnUserChanged;
			UserTable.OnUserAdded += UserTable_OnUserChanged;
		}

		internal ObservableCollection<UserObject> Users { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		private void SetUsers()
		{
			Users = new ObservableCollection<UserObject>(_crashDoc.Users.Select(u => new UserObject(u)));
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
			foreach (var user in Users)
			{
				_crashDoc.Users.Update(user.CUser);
			}

			SetUsers();

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
							_crashDoc.Users.Update(currUser.CUser);
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

		private static CameraState CycleState(CameraState state)
		{
			var stateCount = (int)state;
			stateCount++;

			if (stateCount >= Enum.GetValues(typeof(CameraState)).Length)
			{
				return CameraState.None;
			}

			return (CameraState)stateCount;
		}

		internal sealed class UserObject
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
			private static readonly Dictionary<CameraState, Image> s_cameras;

			static UserUIExtensions()
			{
				s_cameras = new Dictionary<CameraState, Image>
				            {
					            {
						            CameraState.None,
						            (Palette.DarkMode ? Icons.CameraNone_Light : Icons.CameraNone_Dark).ToEto()
					            },
					            {
						            CameraState.Visible,
						            (Palette.DarkMode ? Icons.CameraVisible_Light : Icons.CameraVisible_Dark).ToEto()
					            },
					            {
						            CameraState.Follow,
						            (Palette.DarkMode ? Icons.CameraFollow_Light : Icons.CameraFollow_Dark).ToEto()
					            }
				            };
			}

			internal static Image GetCameraImage(UserObject user)
			{
				return s_cameras[user?.Camera ?? CameraState.None];
			}
		}
	}
}
