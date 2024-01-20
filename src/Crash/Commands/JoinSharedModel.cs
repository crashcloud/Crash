using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Handlers;
using Crash.UI.JoinModel;
using Crash.UI.UsersView;

using Rhino.Commands;
using Rhino.UI;

namespace Crash.Commands
{
	/// <summary>Command to Open a Shared Model</summary>
	[CommandStyle(Style.ScriptRunner)]
	public sealed class JoinSharedModel : AsyncCommand
	{
		private CrashDoc? _crashDoc;

		private string? _lastUrl = $"{CrashClient.DefaultURL}:{CrashClient.DefaultPort}";

		/// <summary>Default Constructor</summary>
		public JoinSharedModel()
		{
			Instance = this;
		}


		public static JoinSharedModel Instance { get; private set; }


		public override string EnglishName => "JoinSharedModel";


		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc crashDoc, RunMode mode)
		{
			_crashDoc = null;

			if (!await CommandUtils.CheckAlreadyConnectedAsync(crashDoc))
			{
				return Result.Cancel;
			}

			await CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);

			var name = Environment.UserName;
			if (mode == RunMode.Interactive)
			{
				var dialog = new JoinWindow();
				var chosenModel = await dialog.ShowModalAsync(RhinoEtoApp.MainWindow);
				dialog.Dispose();

				if (string.IsNullOrEmpty(chosenModel?.ModelAddress))
				{
					await CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);
					return Result.Cancel;
				}

				_lastUrl = chosenModel?.ModelAddress;
			}
			else
			{
				if (!CommandUtils.GetUserName(out name))
				{
					RhinoApp.WriteLine("Invalid Name Input. Avoid empty values");
					return Result.Cancel;
				}

				if (!_GetServerURL(ref _lastUrl))
				{
					RhinoApp.WriteLine("Invalid URL Input. ");
					return Result.Nothing;
				}
			}

			_crashDoc ??= CrashDocRegistry.CreateAndRegisterDocument(doc);

			_CreateCurrentUser(_crashDoc, name);

			await StartServer();

			return Result.Success;
		}

		private async Task StartServer()
		{
			LoadingUtils.Start();

			if (await CommandUtils.StartLocalClient(_crashDoc, _lastUrl))
			{
				LoadingUtils.SetState(LoadingUtils.LoadingState.ConnectingToServer);

				InteractivePipe.Active.Enabled = true;
				_crashDoc.Queue.OnCompletedQueue += QueueOnOnCompleted;
			}
			else if (_crashDoc?.LocalClient is not null)
			{
				await _crashDoc.LocalClient.StopAsync();

				StatusBar.HideProgressMeter();
				StatusBar.ClearMessagePane();
			}
		}

		private void QueueOnOnCompleted(object? sender, CrashEventArgs e)
		{
			LoadingUtils.Close();
			e.CrashDoc.Queue.OnCompletedQueue -= QueueOnOnCompleted;
			UsersForm.CloseActiveForm();
			UsersForm.ShowForm(e.CrashDoc);

			StatusBar.HideProgressMeter();
			StatusBar.ClearMessagePane();
		}

		private bool _GetServerURL(ref string url)
		{
			return SelectionUtils.GetValidString("Server URL", ref url);
		}

		private void _CreateCurrentUser(CrashDoc crashDoc, string name)
		{
			var user = new User(name);
			crashDoc.Users.CurrentUser = user;
		}
	}
}
