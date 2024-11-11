using Crash.Common.Document;
using Crash.Resources;

using Eto.Drawing;

using Rhino.UI;

namespace Crash.UI.UsersView
{

	// TODO : This is messy
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
		public System.Drawing.Color Colour { get; private set; }

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

		public Bitmap Image => Camera switch
		{
			CameraState.Follow => CrashIcons.Icon("camera-follow", 24),
			CameraState.Visible => CrashIcons.Icon("camera", 24),

			_ => CrashIcons.Icon("camera-off", 24)
		};

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
