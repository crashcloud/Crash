using Crash.Common.Communications;
using Crash.Common.Document;
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
		private CrashDoc? crashDoc;

		private string LastURL = $"{CrashClient.DefaultURL}:{CrashServer.DefaultPort}";

		private RhinoDoc rhinoDoc;

		/// <summary>Default Constructor</summary>
		public JoinSharedModel()
		{
			Instance = this;
		}


		public static JoinSharedModel Instance { get; private set; }


		public override string EnglishName => "JoinSharedModel";


		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc CrashDoc, RunMode mode)
		{
			rhinoDoc = doc;
			CrashDoc = crashDoc;

			CommandUtils.CheckAlreadyConnected(CrashDoc);

			var name = Environment.UserName;

			if (mode == RunMode.Interactive)
			{
				var dialog = new JoinWindow();
				var chosenModel = dialog.ShowModal(RhinoEtoApp.MainWindow);

				if (string.IsNullOrEmpty(chosenModel?.ModelAddress))
				{
					RhinoApp.WriteLine("Invalid URL Input");
					return Result.Cancel;
				}

				LastURL = chosenModel?.ModelAddress;
			}
			else
			{
				if (!CommandUtils.GetUserName(out name))
				{
					RhinoApp.WriteLine("Invalid Name Input");
					return Result.Cancel;
				}

				if (!_GetServerURL(ref LastURL))
				{
					RhinoApp.WriteLine("Invalid URL Input");
					return Result.Nothing;
				}
			}

			if (CrashDoc is null)
			{
				CrashDoc = CrashDocRegistry.CreateAndRegisterDocument(doc);
			}

			_CreateCurrentUser(CrashDoc, name);

			await StartServer();

			return Result.Success;
		}

		private async Task StartServer()
		{
			if (await CommandUtils.StartLocalClient(crashDoc, LastURL))
			{
				InteractivePipe.Active.Enabled = true;
				UsersForm.ShowForm();
			}
			else
			{
				if (crashDoc?.LocalClient is not null)
				{
					await crashDoc.LocalClient.StopAsync();
				}

				RhinoApp.WriteLine($"Failed to load URL {LastURL}");
			}
		}


		private static bool _GetServerURL(ref string url)
		{
			return SelectionUtils.GetValidString("Server URL", ref url);
		}

		private static void _CreateCurrentUser(CrashDoc crashDoc, string name)
		{
			var user = new User(name);
			crashDoc.Users.CurrentUser = user;
		}
	}
}
