using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Tables;
using Crash.Handlers.Changes;
using Crash.Handlers.Plugins.Geometry.Recieve;

using Rhino;

namespace Crash.Handlers.Plugins.Initializers.Recieve
{
	/// <summary>Handles 'Done' calls from the Server</summary>
	internal class DoneRecieve : IChangeRecieveAction
	{
		public bool CanRecieve(IChange change)
		{
			return change.Action.HasFlag(ChangeAction.Release);
		}

		public async Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange)
		{
			crashDoc.DocumentIsBusy = true;
			try
			{
				if (!crashDoc.Tables.TryGet<TemporaryChangeTable>(out var tempTable)) return;
				// Done Range
				if (string.IsNullOrEmpty(recievedChange.Owner))
				{
					if (!tempTable.TryGetChangeOfType(recievedChange.Id, out IChange doneChange))
					{
						return;
					}

					await ReleaseChange(crashDoc, doneChange);
				}
				// Done
				else
				{
					foreach (var change in tempTable.GetChanges())
					{
						if (string.Equals(change.Owner, recievedChange.Owner,
										  StringComparison.InvariantCultureIgnoreCase))
						{
							await ReleaseChange(crashDoc, change);
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		private async Task ReleaseChange(CrashDoc crashDoc, IChange change)
		{
			if (!crashDoc.Tables.TryGet<TemporaryChangeTable>(out var tempTable)) return;
			if (!tempTable.TryGetChangeOfType(change.Id, out GeometryChange geomChange)) return;
			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);
			if (rhinoDoc is null) return;

			tempTable.RemoveChange(change.Id);

			geomChange.RemoveAction(ChangeAction.Temporary);

			var add = new GeometryAddRecieveAction();
			await add.OnRecieveAsync(crashDoc, geomChange);

			rhinoDoc.ClearUndoRecords(true);
			(crashDoc.Dispatcher as EventDispatcher).ClearUndoRedoQueue();

		}
	}
}
