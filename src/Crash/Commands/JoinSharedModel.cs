using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Handlers;
using Crash.Handlers.InternalEvents;
using Crash.UI.JoinModel;
using Crash.UI.UsersView;

using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;

using Environment = System.Environment;

namespace Crash.Commands
{
	/// <summary>Command to Open a Shared Model</summary>
	[CommandStyle(Style.ScriptRunner)]
	public sealed class JoinSharedModel : AsyncCommand
	{
		private CrashDoc? _crashDoc;

		private string? _lastUrl = $"{CrashClient.DefaultURL}:{CrashClient.DefaultPort}";
		private RhinoDoc _rhinoDoc;

		/// <summary>Default Constructor</summary>
		public JoinSharedModel()
		{
			Instance = this;
		}


		public static JoinSharedModel Instance { get; private set; }


		public override string EnglishName => "JoinSharedModel";


		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc crashDoc, RunMode mode)
		{
			var dialog = new JoinWindow();
			var chosenModel = await dialog.ShowModalAsync(RhinoEtoApp.MainWindow);

			_crashDoc = null;
			_rhinoDoc = doc;

			if (!await CommandUtils.CheckAlreadyConnectedAsync(crashDoc))
			{
				return Result.Cancel;
			}

			await CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);

			var name = Environment.UserName;
			if (mode == RunMode.Interactive)
			{
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

			var settings = new ObjectEnumeratorSettings
			{
				IncludePhantoms = false,
				IncludeGrips = false,
				DeletedObjects = false,
				HiddenObjects = true,
				IncludeLights = false
			};
			var currentObjects = _rhinoDoc.Objects.GetObjectList(settings);

			_crashDoc.Queue.OnCompletedQueue += QueueOnOnCompleted;
			if (await CommandUtils.StartLocalClient(_crashDoc, _lastUrl))
			{
				LoadingUtils.SetState(LoadingUtils.LoadingState.ConnectingToServer);

				InteractivePipe.Active.Enabled = true;

				// Sends pre-existing Geometry
				foreach (var rhinoObject in currentObjects)
				{
					await _crashDoc.Dispatcher.NotifyServerAsync(ChangeAction.Add | ChangeAction.Temporary,
																 this,
																 new CrashObjectEventArgs(_crashDoc, rhinoObject));
				}

				return;
			}
			else if (_crashDoc?.LocalClient is not null)
			{
				_crashDoc.Queue.OnCompletedQueue -= QueueOnOnCompleted;
				await _crashDoc.LocalClient.StopAsync();

				LoadingUtils.Close();
			}

			_crashDoc.Queue.OnCompletedQueue -= QueueOnOnCompleted;
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
