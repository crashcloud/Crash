using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Handlers;
using Crash.Handlers.Changes;
using Crash.Handlers.InternalEvents;
using Crash.UI.JoinModel;
using Crash.UI.UsersView;

using Eto.Forms;

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
			_crashDoc = null;
			_rhinoDoc = doc;

			if (crashDoc?.LocalClient?.IsConnected == true)
			{
				CommandUtils.AlertUser("You are already connected to a model. Please disconnect first.", mode == RunMode.Scripted);
				return Result.Cancel;
			}

			var name = Environment.UserName;
			if (mode == RunMode.Interactive)
			{
				var dialog = new JoinWindow();
				var chosenModel = await dialog.ShowModalAsync(RhinoEtoApp.MainWindow);

				if (chosenModel is null) return Result.Cancel;

				if (string.IsNullOrEmpty(chosenModel?.ModelAddress))
				{
					return Result.Cancel;
				}

				_lastUrl = chosenModel?.ModelAddress;
			}
			else
			{
				if (!CommandUtils.GetUserName(out name))
				{
					CommandUtils.AlertUser("Invalid Name Input. Avoid empty values", true);
					return Result.Cancel;
				}

				if (!_GetServerURL(ref _lastUrl))
				{
					CommandUtils.AlertUser("Invalid URL Input.", true);
					return Result.Nothing;
				}
			}

			await CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);
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
				List<Change> changes = new List<Change>(currentObjects.Count());
				foreach (var rhinoObject in currentObjects)
				{
					if (rhinoObject?.Geometry is null) continue;
					var change = GeometryChange.CreateNew(rhinoObject.Geometry, _crashDoc.Users.CurrentUser.Name);
					changes.Add(new Change(change));
				}

				await _crashDoc.Dispatcher.NotifyServerAsync(changes);

				UsersForm.ShowForm(_crashDoc);

				return;
			}

			_crashDoc.Queue.OnCompletedQueue -= QueueOnOnCompleted;
			await _crashDoc.LocalClient.StopAsync();

			LoadingUtils.Close();
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
