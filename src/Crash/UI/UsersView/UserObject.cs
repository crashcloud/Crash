using System.Drawing;

using Crash.Common.Document;
using Crash.Properties;

using Rhino.UI;

using Image = Eto.Drawing.Image;

namespace Crash.UI.UsersView
{
	internal sealed class UserObject
	{
		private CrashDoc _crashDoc { get; set; }
		private CameraState _cameraState { get; set; }
		private bool _visible { get; set; }

		public bool IsCurrentUser { get; private set; } = false;

		internal EventHandler<UserObject> OnPropertyChanged;

		internal UserObject(CrashDoc crashDoc, User user)
		{
			_crashDoc = crashDoc;

			Name = user.Name;
			Colour = user.Color;

			_cameraState = user.Camera;
			_visible = user.Visible;
		}

		private UserObject() { }

		internal static UserObject CreateForCurrentUser(CrashDoc crashDoc)
		{
			var user = crashDoc.Users.CurrentUser;

			return new UserObject()
			{
				_crashDoc = crashDoc,
				Name = $"{user.Name} (You)",
				Colour = user.Color,
				_cameraState = CameraState.None,
				_visible = true,
				IsCurrentUser = true,
			};
		}

		public string Name { get; private set; }
		public Color Colour { get; private set; }

		public CameraState Camera
		{
			get => _cameraState;
			set
			{
				if (IsCurrentUser) return;
				_cameraState = value;
				_crashDoc.Users.Update(Convert(this));
				OnPropertyChanged?.Invoke(null, this);
			}
		}

		public Image Image => ((Camera, Palette.DarkMode) switch
		{
			(CameraState.Follow, true) => Icons.CameraFollow_Light,
			(CameraState.Follow, false) => Icons.CameraFollow_Dark,
			(CameraState.Visible, true) => Icons.CameraVisible_Light,
			(CameraState.Visible, false) => Icons.CameraVisible_Dark,
			(CameraState.None, true) => Icons.CameraNone_Light,
			(CameraState.None, false) => Icons.CameraNone_Dark,

			_ => Icons.CameraNone_Dark
		}).ToEto();

		public bool Visible
		{
			get => _visible;

			set
			{
				if (IsCurrentUser) return;
				_visible = value;
				_crashDoc.Users.Update(Convert(this));
				Camera = value ? CameraState.Visible : CameraState.None;
				OnPropertyChanged?.Invoke(null, this);
			}
		}

		private static User Convert(UserObject userObject)
		{
			return new User(userObject.Name) { Camera = userObject.Camera, Visible = userObject.Visible };
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
