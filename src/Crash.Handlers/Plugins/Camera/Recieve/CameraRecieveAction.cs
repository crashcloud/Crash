using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;

using Rhino;

namespace Crash.Handlers.Plugins.Camera.Recieve
{
	/// <summary>Handles receiving a camera from the Server</summary>
	internal sealed class CameraRecieveAction : IChangeRecieveAction
	{

		public bool CanRecieve(ChangeAction action) => action.HasFlag(ChangeAction.Add);


		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			var cameraArgs = new IdleArgs(crashDoc, recievedChange);
			var cameraAction = new IdleAction(AddToDocument, cameraArgs);
			await crashDoc.Queue.AddActionAsync(cameraAction);
		}

		private void AddToDocument(IdleArgs args)
		{
			var convertedChange = CameraChange.CreateFrom(args.Change);
			args.Doc.Cameras.TryAddCamera(convertedChange);
			args.Doc.Users.Add(args.Change.Owner);

			if (args.Doc.Users.Get(args.Change.Owner).Camera == CameraState.Follow)
			{
				FollowCamera(convertedChange);
			}
		}

		private void FollowCamera(CameraChange change)
		{
			var activeView = RhinoDoc.ActiveDoc.Views.ActiveView;

			var cameraTarget = change.Camera.Target.ToRhino();
			var cameraLocation = change.Camera.Location.ToRhino();

			activeView.ActiveViewport.SetCameraLocations(cameraTarget, cameraLocation);
		}
	}
}
