using Crash.Common.Changes;
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
				return false;
			}

			var user = crashArgs.Doc.Users.CurrentUser.Name;
			var transform = cargs.Transform;

			changes = getTransforms(transform, user, cargs.Objects);

			return true;
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
