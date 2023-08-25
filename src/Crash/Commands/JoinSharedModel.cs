using System.Threading.Tasks;

using Crash.Client;
using Crash.Common.Document;
using Crash.Communications;
using Crash.Handlers;
using Crash.Properties;

using Rhino.Commands;
using Rhino.UI;


namespace Crash.Commands
{

	/// <summary>Command to Open a Shared Model</summary>
	[CommandStyle(Style.ScriptRunner)]
	public sealed class JoinSharedModel : Command
	{

		private RhinoDoc rhinoDoc;
		private CrashDoc? crashDoc;

		private string LastURL = $"{CrashClient.DefaultURL}:{CrashServer.DefaultPort}";

		/// <summary>Default Constructor</summary>
		public JoinSharedModel()
		{
			Instance = this;
		}

		/// <inheritdoc />
		public static JoinSharedModel Instance { get; private set; }

		/// <inheritdoc />
		public override string EnglishName => "JoinSharedModel";

		/// <inheritdoc />
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			rhinoDoc = doc;
			crashDoc = CrashDocRegistry.GetRelatedDocument(doc);

			CommandUtils.CheckAlreadyConnected(crashDoc);

			string name = Environment.UserName;

			if (mode == RunMode.Interactive)
			{
				var window = new JoinWindow();
				window.Show();

				LastURL = window.ChosenAddress;

				return Result.Cancel;
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

			crashDoc = CrashDocRegistry.CreateAndRegisterDocument(doc);
			_CreateCurrentUser(crashDoc, name);

			StartServer();

			return Result.Success;
		}

		private async Task StartServer()
		{
			bool success = await CommandUtils.StartLocalClient(crashDoc, LastURL);
			if (success)
			{
				InteractivePipe.Active.Enabled = true;
				UsersForm.ShowForm();
			}
			else
			{
				await crashDoc.LocalClient.StopAsync();
				RhinoApp.WriteLine($"Failed to load URL {LastURL}");
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
