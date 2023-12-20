using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Events;
using Crash.Handlers.Changes;

using Rhino;

namespace Crash.Handlers.Plugins.Geometry.Recieve
{
	/// <summary>Handles recieving of Geometry</summary>
	internal sealed class GeometryAddRecieveAction : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			if (change is null)
			{
				return false;
			}

			if (!change.Action.HasFlag(ChangeAction.Add))
			{
				return false;
			}

			if (change.Action.HasFlag(ChangeAction.Remove))
			{
				return false;
			}

			return true;
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			await OnRecieveAsync(crashDoc, GeometryChange.CreateFrom(recievedChange));
		}

		/// <summary>Handles recieved Geometry Changes</summary>
		public async Task OnRecieveAsync(CrashDoc crashDoc, GeometryChange geomChange)
		{
			if (crashDoc is null || geomChange is null)
			{
				return;
			}

			var changeArgs = new IdleArgs(crashDoc, geomChange);
			IdleAction resultingAction = null;

			if (!geomChange.HasFlag(ChangeAction.Temporary) ||
			    geomChange.Owner?.Equals(crashDoc.Users.CurrentUser.Name,
			                             StringComparison.InvariantCultureIgnoreCase) == true)
			{
				resultingAction = new IdleAction(AddToDocument, changeArgs);
			}
			else
			{
				resultingAction = new IdleAction(AddToCache, changeArgs);
			}

			crashDoc.Queue.AddAction(resultingAction);
		}

		private void AddToDocument(IdleArgs args)
		{
			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(args.Doc);
			if (args.Change is not GeometryChange geomChange)
			{
				return;
			}

			args.Doc.DocumentIsBusy = true;
			try
			{
				var rhinoId = Guid.Empty;
				if (geomChange.Geometry is null)
				{
					args.Doc.RealisedChangeTable.RestoreChange(args.Change.Id);
					args.Doc.RealisedChangeTable.TryGetRhinoId(args.Change.Id, out rhinoId);
				}
				else
				{
					rhinoId = rhinoDoc.Objects.Add(geomChange.Geometry);
				}

				var rhinoObject = rhinoDoc.Objects.FindId(rhinoId);
				if (args.Change.HasFlag(ChangeAction.Locked))
				{
					rhinoDoc.Objects.Select(rhinoId, true, true);
				}
			}
			catch (Exception ex)
			{
				RhinoApp.WriteLine(ex.Message);
			}
			finally
			{
				args.Doc.DocumentIsBusy = false;
			}
		}

		private void AddToCache(IdleArgs args)
		{
			if (args.Change is not GeometryChange geomChange)
			{
				return;
			}

			// Undo Delete Realised
			if (args.Doc.RealisedChangeTable.IsDeleted(geomChange.Id))
			{
				args.Doc.RealisedChangeTable.PurgeChange(geomChange.Id);
				args.Doc.TemporaryChangeTable.UpdateChange(geomChange);

				return;
			}

			// Undo Delete Temporary
			if (args.Doc.TemporaryChangeTable.IsDeleted(geomChange.Id))
			{
				// Restore!
				args.Doc.TemporaryChangeTable.RestoreChange(geomChange.Id);

				return;
			}

			// Add Temporary
			args.Doc.TemporaryChangeTable.UpdateChange(geomChange);
		}
	}
}
