using Crash.Changes.Extensions;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Handlers.Changes;
using Crash.Handlers.Plugins.Geometry.Recieve;

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
			crashDoc.TemporaryChangeTable.SomeoneIsDone = true;
			try
			{
				// Done Range
				if (string.IsNullOrEmpty(recievedChange.Owner))
				{
					if (!crashDoc.TemporaryChangeTable.TryGetValue(recievedChange.Id, out Change doneChange))
					{
						return;
					}

					ReleaseChange(crashDoc, doneChange);
				}
				// Done
				else
				{
					foreach (var change in crashDoc.TemporaryChangeTable.GetChanges())
					{
						ReleaseChange(crashDoc, change);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
			finally
			{
				// TODO: This seems like it is doomed to fail!
				// We need to specify which packets dont need reporting to the server
				EventHandler<CrashEventArgs>? _event = null;
				_event = (sender, args) =>
				         {
					         crashDoc.TemporaryChangeTable.SomeoneIsDone = false;
					         crashDoc.Queue.OnCompletedQueue -= _event;
				         };

				// TODO : What about things in the existing queue etc?
				crashDoc.Queue.OnCompletedQueue += _event;
			}
		}

		private async Task ReleaseChange(CrashDoc crashDoc, IChange change)
		{
			if (!crashDoc.TemporaryChangeTable.TryGetValue(change.Id,
			                                               out GeometryChange geomChange))
			{
				return;
			}

			geomChange.RemoveAction(ChangeAction.Temporary);

			var add = new GeometryAddRecieveAction();
			await add.OnRecieveAsync(crashDoc, geomChange);
			crashDoc.TemporaryChangeTable.RemoveChange(change.Id);
		}
	}
}
