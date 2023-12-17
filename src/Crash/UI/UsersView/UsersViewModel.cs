using System.Collections.ObjectModel;

using Crash.Common.Document;
using Crash.Common.Tables;
using Crash.Handlers;
using Crash.Properties;

using Eto.Drawing;
using Eto.Forms;

using Rhino.UI;

namespace Crash.UI.UsersView
{
	internal sealed class UsersViewModel : BaseViewModel
	{
		private static readonly Dictionary<CameraState, Image> s_cameras = new()
		                                                                   {
			                                                                   {
				                                                                   CameraState.None, (Palette.DarkMode
							                                                                   ? Icons
								                                                                   .CameraNone_Light
							                                                                   : Icons
								                                                                   .CameraNone_Dark)
				                                                                   .ToEto()
			                                                                   },
			                                                                   {
				                                                                   CameraState.Visible,
				                                                                   (Palette.DarkMode
						                                                                   ? Icons
							                                                                   .CameraVisible_Light
						                                                                   : Icons
							                                                                   .CameraVisible_Dark)
				                                                                   .ToEto()
			                                                                   },
			                                                                   {
				                                                                   CameraState.Follow, (Palette.DarkMode
							                                                                   ? Icons
								                                                                   .CameraFollow_Light
							                                                                   : Icons
								                                                                   .CameraFollow_Dark)
				                                                                   .ToEto()
			                                                                   }
		                                                                   };

		private readonly CrashDoc _crashDoc;

		internal UsersViewModel(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
			var userObjects = _crashDoc.Users.Select(u =>
			                                         {
				                                         var user = new UserObject(_crashDoc, u);
				                                         user.OnPropertyChanged += RedrawView;
				                                         return user;
			                                         });
			Users = new ObservableCollection<UserObject>(userObjects);

			UserTable.OnUserRemoved += UserRemoved;
			UserTable.OnUserAdded += AddUsers;
		}

		internal ObservableCollection<UserObject> Users { get; set; }

		private void RedrawView(object? sender, UserObject e)
		{
			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(_crashDoc);
			rhinoDoc?.Views.Redraw();
		}

		private void AddUsers(object? sender, UserEventArgs e)
		{
			Application.Instance.Invoke(() =>
			                            {
				                            Users.Add(new UserObject(_crashDoc, e.User));
				                            UsersForm.ReDraw();
			                            });
		}

		private void UserRemoved(object? sender, UserEventArgs e)
		{
			Application.Instance.Invoke(() =>
			                            {
				                            Users.Remove(new UserObject(_crashDoc, e.User));
				                            UsersForm.ReDraw();
			                            });
		}

		internal void CycleCameraSetting(object? sender, GridCellMouseEventArgs e)
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
					foreach (var currUser in Users)
					{
						if (CameraState.Follow != currUser.Camera)
						{
							continue;
						}

						currUser.Camera = CameraState.Visible;
					}
				}

				user.Camera = state;
				var rhinoDoc = CrashDocRegistry.GetRelatedDocument(_crashDoc);
				rhinoDoc?.Views.Redraw();
			}
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

		internal static Image GetCameraImage(UserObject user)
		{
			return s_cameras[user?.Camera ?? CameraState.None];
		}
	}
}
