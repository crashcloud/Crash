using System.Drawing;

using Crash.Common.Document;

namespace Crash.UI.UsersView
{
	internal sealed class UserObject
	{
		private readonly CrashDoc _crashDoc;
		private CameraState _cameraState;
		private bool _visible;

		internal EventHandler<UserObject> OnPropertyChanged;

		internal UserObject(CrashDoc crashDoc, User user)
		{
			_crashDoc = crashDoc;

			Name = user.Name;
			Colour = user.Color;

			_cameraState = user.Camera;
			_visible = user.Visible;
		}

		public string Name { get; }
		public Color Colour { get; }

		public CameraState Camera
		{
			get => _cameraState;
			set
			{
				_cameraState = value;
				_crashDoc.Users.Update(Convert(this));
				OnPropertyChanged?.Invoke(null, this);
			}
		}

		public bool Visible
		{
			get => _visible;

			set
			{
				_visible = value;
				_crashDoc.Users.Update(Convert(this));
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
