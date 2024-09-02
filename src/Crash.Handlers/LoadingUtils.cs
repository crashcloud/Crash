using Crash.Common.App;
using Crash.Common.Document;

using Rhino.UI;

namespace Crash.Commands
{

	public static class LoadingUtils
	{

		internal class CrashStatusBar : ICrashInstance
		{
			public bool Enabled { get; set; }

			public int _progress { get; set; } = 0;
			public int Progress
			{
				get => _progress;
				set
				{
					_progress = value;
					if (Enabled)
						StatusBar.UpdateProgressMeter(_progress, true);
				}
			}


		}

		public enum LoadingState
		{
			None = -1,
			CheckingServer = 0,
			ConnectingToServer = 25,
			LoadingChanges = 50,
			Done = 100,
		}


		public static void Start(CrashDoc crashDoc)
		{
			Close(crashDoc);

			var statusBar = new CrashStatusBar();

			statusBar.Progress = 0;
			statusBar.Enabled = true;
			CrashInstances.TrySetInstance(crashDoc, statusBar);

			StatusBar.ShowProgressMeter(0, 100, "Loading Crash", true, true);
			SetState(crashDoc, LoadingState.CheckingServer);
		}

		public static void SetState(CrashDoc crashDoc, LoadingState state, bool slowlyUpdate = true)
		{
			if (state == LoadingState.None)
			{
				Close(crashDoc);
				return;
			}

			if (!CrashInstances.TryGetInstance(crashDoc, out CrashStatusBar statusBar)) return;

			statusBar.Progress = (int)state;
			LoadingState nextState = LoadingState.None;

			if (state == LoadingState.CheckingServer)
			{
				nextState = LoadingState.ConnectingToServer;
			}
			else if (state == LoadingState.ConnectingToServer)
			{
				nextState = LoadingState.LoadingChanges;
			}
			else if (state == LoadingState.LoadingChanges)
			{
				nextState = LoadingState.Done;
			}

#pragma warning disable CS4014, VSTHRD110 // Because this call is not awaited, execution of the current method continues before the call is completed
			if (slowlyUpdate)
				SlowlyUpdate(statusBar, (int)nextState);
#pragma warning restore CS4014, VSTHRD110 // Because this call is not awaited, execution of the current method continues before the call is completed

		}

		private static async Task SlowlyUpdate(CrashStatusBar statusBar, int end)
		{
			await Task.Run(async () =>
			{
				while (statusBar.Enabled && statusBar.Progress < end)
				{
					await Task.Delay(600);
					statusBar.Progress++;
				}
			});
		}

		public static void Close(CrashDoc crashDoc)
		{
			if (!CrashInstances.TryGetInstance(crashDoc, out CrashStatusBar statusBar)) return;
			statusBar.Enabled = false;
			StatusBar.HideProgressMeter();
			StatusBar.ClearMessagePane();
		}

	}

}
