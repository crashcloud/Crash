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
			if (crashArgs.Args is not CrashTransformEventArgs transformArgs)
			{
				crashArgs.Doc.DocumentIsBusy = false;
				return false;
			}

			if (transformArgs.ObjectsWillBeCopied)
			{
				var create = new GeometryCreateAction();
				var newChanges = new List<Change>(transformArgs.Objects.Count());
				foreach (var crashObject in transformArgs.Objects)
				{
					var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashArgs.Doc);
					var rhinoObject = rhinoDoc.Objects.FindId(crashObject.RhinoId);
					rhinoObject.Geometry.UserDictionary.Clear();

					var geometry = rhinoObject.Geometry.Duplicate();
					geometry.Transform(transformArgs.Transform.ToRhino());

					var changeId = Guid.NewGuid();
					var createArgs = new CreateRecieveArgs(ChangeAction.Add | ChangeAction.Temporary,
					                                       new CrashObjectEventArgs(geometry,
							                                        crashObject.RhinoId, changeId),
					                                       crashArgs.Doc);

					create.TryConvert(sender, createArgs, out var changesOut);
					newChanges.AddRange(changesOut);
				}

				changes = newChanges;
			}
			else
			{
				var user = crashArgs.Doc.Users.CurrentUser.Name;
				var transform = transformArgs.Transform;

				changes = getTransforms(transform, user, transformArgs.Objects);
			}

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
			var transformChanges = new List<Change>(crashObjects.Count());
			foreach (var crashObject in crashObjects)
			{
				if (crashObject.ChangeId == Guid.Empty)
				{
					continue;
				}

				var transformChange = TransformChange.CreateChange(crashObject.ChangeId, userName, transform);
				transformChanges.Add(transformChange);
			}

			return transformChanges;
		}
	}
}
