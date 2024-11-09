using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Handlers.Data;
using Crash.Handlers;
using Crash.Handlers.Changes;
using Crash.Handlers.InternalEvents;
using Crash.UI.JoinView;
using Crash.UI.UsersView;

using Eto.Forms;

using Rhino.Commands;
using Rhino.Display;
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

		public override string EnglishName => EnglishCommandName;

		public const string EnglishCommandName = "JoinSharedModel";

		public JoinSharedModel() : base(true)
		{
		}

		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc crashDoc, RunMode mode)
		{
			_crashDoc = null;
			_rhinoDoc = doc;

			if (crashDoc?.LocalClient?.IsConnected == true)
			{
				CommandUtils.AlertUser(crashDoc, $"You are already connected to a model ({crashDoc.LocalClient.Url}). Please disconnect first.", mode == RunMode.Scripted);
				return Result.Cancel;
			}

			var name = Environment.UserName;
			SharedModel chosenModel = null;
			if (mode == RunMode.Interactive)
			{
				var dialog = new RecentModelDialog();
				chosenModel = await dialog.ShowModalAsync(RhinoEtoApp.MainWindowForDocument(doc));
				if (chosenModel is null) return Result.Cancel;

				if (string.IsNullOrEmpty(chosenModel?.ModelAddress)) return Result.Cancel;

				_lastUrl = chosenModel?.ModelAddress;
			}
			else
			{
				if (!CommandUtils.GetUserName(out name))
				{
					CommandUtils.AlertUser(crashDoc, "Invalid Name Input. Avoid empty values", true);
					return Result.Cancel;
				}

				if (!_GetServerURL(ref _lastUrl))
				{
					CommandUtils.AlertUser(crashDoc, $"{_lastUrl} is an invalid URL.", true);
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
			LoadingUtils.Start(_crashDoc);

			var settings = new ObjectEnumeratorSettings
			{
				IncludePhantoms = false,
				IncludeGrips = false,
				DeletedObjects = false,
				HiddenObjects = true,
				IncludeLights = false
			};
			var currentObjects = _rhinoDoc.Objects.GetObjectList(settings);

			LoadingUtils.SetState(_crashDoc, LoadingUtils.LoadingState.CheckingServer);
			_crashDoc.Queue.OnCompletedQueue += QueueOnOnCompleted;

			if (await CommandUtils.StartLocalClient(_crashDoc, _lastUrl))
			{
				LoadingUtils.SetState(_crashDoc, LoadingUtils.LoadingState.ConnectingToServer);

				var pipe = InteractivePipe.GetActive(_crashDoc);
				pipe.Enabled = true;

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

			LoadingUtils.Close(_crashDoc);
		}

		private void QueueOnOnCompleted(object? sender, CrashEventArgs e)
		{
			LoadingUtils.Close(_crashDoc);
			e.CrashDoc.Queue.OnCompletedQueue -= QueueOnOnCompleted;
			UsersForm.ShowForm(e.CrashDoc);

			StatusBar.HideProgressMeter();
			StatusBar.ClearMessagePane();
			_rhinoDoc.Views.Redraw();
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
