using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Common.Tables;
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

			if (!args.Doc.Tables.TryGet<RealisedChangeTable>(out var realisedTable)) return;

			args.Doc.DocumentIsBusy = true;
			try
			{
				var rhinoId = Guid.Empty;
				if (geomChange.Geometry is null)
				{
					realisedTable.RestoreChange(args.Change.Id);
					realisedTable.TryGetRhinoId(args.Change.Id, out rhinoId);
				}
				else
				{
					rhinoId = rhinoDoc.Objects.Add(geomChange.Geometry);
				}

				var rhinoObject = rhinoDoc.Objects.FindId(rhinoId);
				realisedTable.AddPair(args.Change.Id, rhinoObject.Id);
				if (args.Change.HasFlag(ChangeAction.Locked))
				{
					if (string.Equals(args.Change.Owner, args.Doc.Users.CurrentUser.Name,
									  StringComparison.InvariantCultureIgnoreCase))
					{
						rhinoDoc.Objects.Select(rhinoId, true, true);
					}
					else
					{
						rhinoDoc.Objects.Lock(rhinoObject, true);
					}
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

			if (!args.Doc.Tables.TryGet<RealisedChangeTable>(out var realisedTable)) return;
			if (!args.Doc.Tables.TryGet<TemporaryChangeTable>(out var tempTable)) return;

			// Undo Delete Realised
			if (realisedTable.IsDeleted(geomChange.Id))
			{
				realisedTable.PurgeChange(geomChange.Id);
			}

			// Undo Delete Temporary
			if (tempTable.IsDeleted(geomChange.Id))
			{
				// Restore!
				tempTable.RestoreChange(geomChange.Id);

				return;
			}

			// Add Temporary
			tempTable.UpdateChange(geomChange);
		}
	}
}
