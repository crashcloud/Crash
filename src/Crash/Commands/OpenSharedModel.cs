using Crash.Client;
using Crash.Common.Document;
using Crash.Communications;
using Crash.Handlers;
using Crash.Properties;

using Rhino.Commands;
using Rhino.UI;

using static Crash.UI.SharedModelViewModel;

namespace Crash.Commands
{

	/// <summary>Command to Open a Shared Model</summary>
	[CommandStyle(Style.ScriptRunner)]
	public sealed class OpenSharedModel : Command
	{

		private RhinoDoc rhinoDoc;
		private CrashDoc? crashDoc;

		private string LastURL = $"{CrashClient.DefaultURL}:{CrashServer.DefaultPort}";


		/// <summary>Default Constructor</summary>
		public OpenSharedModel()
		{
			Instance = this;
		}

		/// <inheritdoc />
		public static OpenSharedModel Instance { get; private set; }

		/// <inheritdoc />
		public override string EnglishName => "OpenSharedModel";

		SharedModelViewModel model = new SharedModelViewModel();

		/// <inheritdoc />
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			rhinoDoc = doc;
			crashDoc = CrashDocRegistry.GetRelatedDocument(doc);

			CommandUtils.CheckAlreadyConnected(crashDoc);

			if (!CommandUtils.GetUserName(out string name))
			{
				return Result.Cancel;
			}

			if (mode == RunMode.Interactive)
			{
				var window = new SharedModelWindow();
				var dialog = new Eto.Forms.Dialog<SharedModel>
				{
					Title = "Available Models",
					Content = window,
					DataContext = window.Model,
					Icon = Icons.crashlogo.ToEto(),
				};

				var model = dialog.ShowModal(Rhino.UI.RhinoEtoApp.MainWindowForDocument(doc));

				if (model is null) return Result.Failure;
				LastURL = model.ModelAddress;
			}
			else
			{
				if (!_GetServerURL(ref LastURL))
				{
					RhinoApp.WriteLine("Invalid URL Input");
					return Result.Nothing;
				}
			}

			crashDoc = CrashDocRegistry.CreateAndRegisterDocument(doc);
			_CreateCurrentUser(crashDoc, name);

			bool success = CommandUtils.StartLocalClient(crashDoc, LastURL).Wait(3000);
			// Rhino.UI.StatusBar.UpdateProgressMeter(0, true)
			if (success)
			{
				InteractivePipe.Active.Enabled = true;
				UsersForm.ShowForm();
				return Result.Success;
			}
			else
			{
				crashDoc.LocalClient.StopAsync();
				RhinoApp.WriteLine($"Failed to load URL {LastURL}");
				return Result.Failure;
			}
		}


		private bool _GetServerURL(ref string url)
			=> SelectionUtils.GetValidString("Server URL", ref url);

		private void _CreateCurrentUser(CrashDoc crashDoc, string name)
		{
			User user = new User(name);
			crashDoc.Users.CurrentUser = user;
		}

	}

}
