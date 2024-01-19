using Rhino.UI;

namespace Crash.Commands
{

	internal static class LoadingUtils
	{
		internal enum LoadingState
		{
			None = -1,
			CheckingServer = 0,
			ConnectingToServer = 25,
			LoadingChanges = 50,
			Done = 100,
		}

		private static bool Enabled { get; set; }

		private static int _progress { get; set; } = 0;
		private static int Progress
		{
			get => _progress;
			set
			{
				_progress = value;
				if (Enabled)
					StatusBar.UpdateProgressMeter(_progress, true);
			}
		}

		internal static void Start()
		{
			Close();
			
			Progress = 0;
			Enabled = true;

			StatusBar.ShowProgressMeter(0, 100, "Loading Crash", true, true);
			SetState(LoadingState.CheckingServer);
		}

		internal static void SetState(LoadingState state)
		{
			if (state == LoadingState.None)
			{
				Close();
				return;
			}

			Progress = (int)state;
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

			SlowlyUpdate((int)nextState);
		}

		private static async Task SlowlyUpdate(int end)
		{
			await Task.Run(async () =>
			{
				while (Enabled && Progress < end)
				{
					await Task.Delay(600);
					Progress++;
				}
			});
		}

		internal static void Close()
		{
			Enabled = false;
			StatusBar.HideProgressMeter();
			StatusBar.ClearMessagePane();
		}

	}

}
