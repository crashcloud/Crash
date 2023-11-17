using Crash.Common.Communications;
using Crash.Common.Document;
using Crash.Common.Events;
using Crash.Handlers;
using Crash.Handlers.InternalEvents;
using Crash.Handlers.Plugins;
using Crash.UI.UsersView;

using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Crash.Commands
{
	/// <summary>
	///     Command to start the shared model
	/// </summary>
	public sealed class StartSharedModel : Command
	{
		private readonly string LastClientURL = CrashClient.DefaultURL;
		private readonly string LastServerURL = CrashServer.DefaultUrl;
		private CrashDoc _crashDoc;
		private RhinoDoc _rhinoDoc;

		private bool includePreExistingGeometry;

		private int LastPort = CrashServer.DefaultPort;

		/// <summary>
		///     Empty constructor
		/// </summary>
		public StartSharedModel()
		{
			Instance = this;
		}

		private string LastServerURLAndPort => $"{LastServerURL}:{LastPort}";
		private string LastClientURLAndPort => $"{LastClientURL}:{LastPort}/Crash";

		/// <summary>
		///     Command Instance
		/// </summary>
		public static StartSharedModel Instance { get; private set; }


		public override string EnglishName => "StartSharedModel";


		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			_rhinoDoc = doc;
			_crashDoc = CrashDocRegistry.GetRelatedDocument(doc);

			if (CommandUtils.CheckForRunningServer(_crashDoc))
			{
				return Result.Cancel;
			}

			if (!CommandUtils.GetUserName(out var name))
			{
				return Result.Cancel;
			}

			// TODO : Add Port Suggestions to docs
			if (!CommandUtils.GetPortFromUser(ref LastPort))
			{
				RhinoApp.WriteLine("Invalid Port!");
				return Result.Nothing;
			}

			_crashDoc = CrashDocRegistry.CreateAndRegisterDocument(doc);

			_CreateCurrentUser(name);

			// TODO : What does this do?
#if DEBUG
			if (_PreExistingGeometryCheck(doc))
			{
				includePreExistingGeometry = IncludePreExistingGeometry() == true;
			}
#endif

			try
			{
				_crashDoc.LocalServer = new CrashServer(_crashDoc);

				_crashDoc.LocalServer.OnConnected += Server_OnConnected;
				_crashDoc.LocalServer.OnFailure += Server_OnFailure;

				_crashDoc.LocalServer.Start(LastServerURLAndPort);

				InteractivePipe.Active.Enabled = true;
				UsersForm.ShowForm();
			}
			catch (Exception ex)
			{
				RhinoApp.WriteLine("The server ran into difficulties starting.");
				RhinoApp.WriteLine($"More specifically ; {ex.Message}.");

				if (_GetForceCloseOptions() != true)
				{
					return Result.Cancel;
				}

				if (!CrashServer.ForceCloselocalServers(1000))
				{
					return Result.Cancel;
				}

				RhinoApp.RunScript(EnglishName, true);
			}

			return Result.Success;
		}

		private void AddPreExistingGeometry(CrashDoc crashDoc)
		{
			var user = crashDoc.Users?.CurrentUser.Name;
			if (string.IsNullOrEmpty(user))
			{
				RhinoApp.WriteLine("User is invalid!");
				return;
			}

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);

			var enumer = GetObjects(rhinoDoc).GetEnumerator();
			while (enumer.MoveNext())
			{
				var args = new CrashObjectEventArgs(enumer.Current);
				EventDispatcher.Instance.NotifyServerAsync(ChangeAction.Add, this, args, rhinoDoc);
			}
		}

		private bool? IncludePreExistingGeometry(bool defaultValue = false)
		{
			return SelectionUtils.GetBoolean(ref defaultValue,
			                                 "Would you like to include preExisting Geometry?",
			                                 "dontInclude",
			                                 "include");
		}

		private static IEnumerable<RhinoObject> GetObjects(RhinoDoc doc)
		{
			var settings = new ObjectEnumeratorSettings
			               {
				               ActiveObjects = true,
				               DeletedObjects = false,
				               HiddenObjects = true,
				               IncludeGrips = false,
				               IncludeLights = false,
				               LockedObjects = true,
				               NormalObjects = true
			               };
			return doc.Objects.GetObjectList(settings);
		}

		private static bool _PreExistingGeometryCheck(RhinoDoc doc)
		{
			var enumerator = doc.Objects.GetEnumerator();
			while (enumerator.MoveNext())
			{
				var rhinoObject = enumerator.Current;
				if (!rhinoObject.IsDeleted)
				{
					return true;
				}
			}

			return false;
		}

		private static void Server_OnFailure(object sender, CrashEventArgs e)
		{
			if (e.CrashDoc.LocalServer is not null)
			{
				e.CrashDoc.LocalServer.OnFailure -= Server_OnFailure;
			}

			RhinoApp.WriteLine("An Unknown Error occured");
		}

		private void Server_OnConnected(object sender, CrashEventArgs e)
		{
			if (e?.CrashDoc is null)
			{
				return;
			}

			e.CrashDoc.LocalServer.OnConnected -= Server_OnConnected;

			try
			{
				var userName = e.CrashDoc.Users.CurrentUser.Name;
				e.CrashDoc.LocalClient.RegisterConnection(userName, new Uri(LastClientURLAndPort));
				e.CrashDoc.LocalClient.StartLocalClientAsync();

				if (includePreExistingGeometry)
				{
					AddPreExistingGeometry(e.CrashDoc);
				}
			}
			catch (UriFormatException)
			{
				RhinoApp.Write("Please enter a valid host! The Port is likely bad.");
			}
			catch (Exception ex)
			{
				RhinoApp.WriteLine(ex.Message);
			}
		}

		private static bool? _GetForceCloseOptions()
		{
			var defaultValue = false;

			var go = new GetOption();
			go.AcceptEnterWhenDone(true);
			go.AcceptNothing(true);
			go.SetCommandPrompt("Would you like to Force Close any other servers?");
			var releaseValue = new OptionToggle(defaultValue, "NoThanks", "CloseAll");
			var releaseIndex = go.AddOptionToggle("Close", ref releaseValue);

			while (true)
			{
				var result = go.Get();
				switch (result)
				{
					case GetResult.Option:
						{
							var index = go.OptionIndex();
							if (index == releaseIndex)
							{
								defaultValue = !defaultValue;
							}

							break;
						}

					case GetResult.Nothing:
						return defaultValue;

					default:
					case GetResult.Cancel:
					case GetResult.ExitRhino:
						return null;
				}
			}
		}

		// TODO : Ensure name is not already taken!
		private bool _GetUsersName(ref string name)
		{
			return SelectionUtils.GetValidString("Your Name", ref name);
		}

		private void _CreateCurrentUser(string name)
		{
			var user = new User(name);
			_crashDoc.Users.CurrentUser = user;
		}
	}
}
