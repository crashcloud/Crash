using Crash.Changes.Extensions;
using Crash.Common.Document;
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
				// Done Range
				if (string.IsNullOrEmpty(recievedChange.Owner))
				{
					if (!crashDoc.TemporaryChangeTable.TryGetChangeOfType(recievedChange.Id, out IChange doneChange))
					{
						return;
					}

					await ReleaseChange(crashDoc, doneChange);
				}
				// Done
				else
				{
					foreach (var change in crashDoc.TemporaryChangeTable.GetChanges())
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
			if (!crashDoc.TemporaryChangeTable.TryGetChangeOfType(change.Id,
			                                                      out GeometryChange geomChange))
			{
				return;
			}

			crashDoc.TemporaryChangeTable.RemoveChange(change.Id);

			geomChange.RemoveAction(ChangeAction.Temporary);

			var add = new GeometryAddRecieveAction();
			await add.OnRecieveAsync(crashDoc, geomChange);

			RhinoDoc.ActiveDoc.ClearUndoRecords(true);
			(crashDoc.Dispatcher as EventDispatcher).ClearUndoRedoQueue();

		}
	}
}
