﻿using Crash.Changes.Extensions;
using Crash.Common.Document;
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
			crashDoc.CacheTable.SomeoneIsDone = true;
			try
			{
				// Done Range
				if (string.IsNullOrEmpty(recievedChange.Owner))
				{
					if (!crashDoc.CacheTable.TryGetValue(recievedChange.Id, out Change doneChange))
					{
						return;
					}

					ReleaseChange(crashDoc, doneChange);
				}
				// Done
				else
				{
					foreach (var change in crashDoc.CacheTable.GetChanges())
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
				EventHandler? _event = null;
				_event = (sender, args) =>
				{
					crashDoc.CacheTable.SomeoneIsDone = false;
					crashDoc.Queue.OnCompletedQueue -= _event;
				};

				// TODO : What about things in the existing queue etc?
				crashDoc.Queue.OnCompletedQueue += _event;
			}
		}

		private async Task ReleaseChange(CrashDoc crashDoc, IChange change)
		{
			if (!crashDoc.CacheTable.TryGetValue(change.Id,
				    out GeometryChange geomChange))
			{
				return;
			}

			geomChange.RemoveAction(ChangeAction.Temporary);

			var add = new GeometryAddRecieveAction();
			await add.OnRecieveAsync(crashDoc, geomChange);
			crashDoc.CacheTable.RemoveChange(change.Id);
		}
	}
}
