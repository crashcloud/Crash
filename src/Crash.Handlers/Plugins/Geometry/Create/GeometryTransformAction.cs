using Crash.Common.Changes;
using Crash.Common.Events;
using Crash.Events;
using Crash.Geometry;
using Crash.Handlers.InternalEvents;

namespace Crash.Handlers.Plugins.Geometry.Create
{
	/// <summary>Handles Transform Changes</summary>
	internal sealed class GeometryTransformAction : IChangeCreateAction
	{
		
		public ChangeAction Action => ChangeAction.Transform;

		
		public bool CanConvert(object sender, CreateRecieveArgs crashArgs)
		{
			return crashArgs.Args is CrashTransformEventArgs;
		}


		public bool TryConvert(object sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes)
		{
			changes = Array.Empty<Change>();
			if (crashArgs.Args is not CrashTransformEventArgs cargs)
			{
				crashArgs.Doc.DocumentIsBusy = false;
				return false;
			}

			var user = crashArgs.Doc.Users.CurrentUser.Name;
			var transform = cargs.Transform;

			changes = getTransforms(transform, user, cargs.Objects);
			crashArgs.Doc.Queue.AddAction(new IdleAction(ResetBusy, new IdleArgs(crashArgs.Doc, null)));

			return true;
		}

		private void ResetBusy(IdleArgs args)
		{
			args.Doc.DocumentIsBusy = false;
		}

		private IEnumerable<Change> getTransforms(CTransform transform, string userName,
			IEnumerable<CrashObject> crashObjects)
		{
			foreach (var crashObject in crashObjects)
			{
				if (crashObject.ChangeId == Guid.Empty)
				{
					continue;
				}

				var transChange = TransformChange.CreateChange(crashObject.ChangeId, userName, transform);
				yield return transChange;
			}
		}
	}
}
