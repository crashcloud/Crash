using System.Collections.ObjectModel;

using Crash.Common.Document;
using Crash.Common.Tables;

using Eto.Forms;

namespace Crash.UI.UsersView
{
	internal sealed class UsersViewModel : BaseViewModel
	{
		private readonly CrashDoc _crashDoc;

		internal EventHandler OnInvalidate;

		internal UsersViewModel(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
			var userObjects = _crashDoc.Users.Where(u => !string.IsNullOrEmpty(u.Name)).Select(u =>
						 {
							 var user = new UserObject(_crashDoc, u);
							 user.OnPropertyChanged += (sender, o) => UsersForm.ReDraw(crashDoc);
							 return user;
						 });
			Users = new ObservableCollection<UserObject> { UserObject.CreateForCurrentUser(_crashDoc) };

			foreach (var userObject in userObjects)
				Users.Add(userObject);

			crashDoc.Users.OnUserRemoved += UserRemoved;
			crashDoc.Users.OnUserAdded += AddUsers;
		}

		internal ObservableCollection<UserObject> Users { get; set; }

		private void AddUsers(object? sender, UserEventArgs e)
		{
			if (string.IsNullOrEmpty(e.User.Name))
			{
				return;
			}

			Application.Instance.Invoke(() =>
										{
											Users.Add(new UserObject(_crashDoc, e.User));
											UsersForm.ReDraw(_crashDoc);
										});
		}

		private void UserRemoved(object? sender, UserEventArgs e)
		{
			Application.Instance.Invoke(() =>
										{
											Users.Remove(new UserObject(_crashDoc, e.User));
											UsersForm.ReDraw(_crashDoc);
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

			if (e.Item is not UserObject user)
			{
				return;
			}

			if (user.IsCurrentUser) return;

			var state = CycleState(user.Camera);
			if (state == CameraState.Follow)
			{
				foreach (var currUser in Users)
				{
					if (currUser.Camera == CameraState.Follow)
					{
						currUser.Camera = CameraState.Visible;
					}
				}
			}

			user.Camera = state;
			var index = Users.IndexOf(user);
			Users.RemoveAt(index);
			Users.Insert(index, user);
			UsersForm.ReDraw(_crashDoc);
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
	}
}
