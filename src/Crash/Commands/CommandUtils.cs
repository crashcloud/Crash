using Crash.Common.Communications;
using Crash.Common.Document;

using Rhino.Geometry;

namespace Crash.Commands
{
	/// <summary>Helpful Utilities for Commands</summary>
	internal static class CommandUtils
	{
		private static readonly Dictionary<Interval, string> PortValidation = new()
																			  {
																				  {
																					  new Interval(int.MinValue, 1000),
																					  "Port number is too small!"
																				  },
																				  {
																					  new Interval(10000, int.MaxValue),
																					  "Port number is too high!"
																				  },
																				  {
																					  new Interval(5000, 5001),
																					  "Port number is already in use on Macs"
																				  }
																			  };

		/// <summary>
		///     Checks if already connected, and prompts user
		///     to take action if connected.
		/// </summary>
		/// <returns>True if already connected</returns>
		internal static bool CheckAlreadyConnected(CrashDoc crashDoc)
		{
			if (crashDoc?.LocalClient?.IsConnected == true)
			{
				RhinoApp.WriteLine("You are currently part of a Shared Model Session.");

				if (!_NewModelOrExit(false))
				{
					return false;
				}

				if (RhinoApp.RunScript(LeaveSharedModel.Instance.EnglishName, true))
				{
					RhinoApp.RunScript(JoinSharedModel.Instance.EnglishName, true);
				}
			}

			return true;
		}


		private static bool _NewModelOrExit(bool defaultValue)
		{
			return SelectionUtils.GetBoolean(ref defaultValue,
											 "Would you like to close this model?",
											 "ExitCommand",
											 "CloseModel") == true;
		}

		/// <summary>Gets the Users Name from a command line prompt</summary>
		/// <param name="name">The User Name</param>
		/// <returns>True if name is valid</returns>
		internal static bool GetUserName(out string name)
		{
			name = Environment.UserName;
			if (!_GetUsersName(ref name))
			{
				RhinoApp.WriteLine("Invalid Name Input");
				return false;
			}

			return true;
		}

		private static bool _GetUsersName(ref string name)
		{
			return SelectionUtils.GetValidString("Your Name", ref name);
		}


		internal static async Task<bool> StartLocalClient(CrashDoc crashDoc, string url)
		{
			var userName = crashDoc.Users.CurrentUser.Name;

			try
			{
				var crashClient = new CrashClient(crashDoc, userName, new Uri($"{url}/Crash"));
				crashDoc.LocalClient = crashClient;

				await crashClient.StartLocalClientAsync();
				return true;
			}
			catch (HttpRequestException)
			{
				RhinoApp.WriteLine("Server was not found! Try retyping the address.");
			}
			catch (UriFormatException)
			{
				RhinoApp.WriteLine("Address was invalid!");
			}
			catch (InvalidOperationException)
			{
				RhinoApp.WriteLine("There was an issue with the local client");
			}
			catch (Exception ex)
			{
				RhinoApp.WriteLine("An unexplained exception occured, try again.");
				RhinoApp.WriteLine(ex.Message);
			}

			return false;
		}

		/// <summary>Checks if a local server is running</summary>
		/// <returns>True if Server is Running</returns>
		internal static bool CheckForRunningServer(CrashDoc crashDoc)
		{
			if (crashDoc?.LocalServer?.IsRunning == true)
			{
				var leaveModelCommandName = LeaveSharedModel.Instance.EnglishName;
				RhinoApp.WriteLine("You are currently part of a Shared Model Session. " +
								   $"Please use the {leaveModelCommandName} command.");

				return true;
			}

			return false;
		}

		/// <summary>Prompts the User for a Port with validatiobn</summary>
		/// <returns>True on success</returns>
		internal static bool GetPortFromUser(ref int port)
		{
			var portIsNotValid = true;
			while (portIsNotValid)
			{
				if (!SelectionUtils.GetPositiveInteger("Server port", ref port))
				{
					return false;
				}

				var notValid = false;
				foreach (var validationPair in PortValidation)
				{
					if (validationPair.Key.IncludesParameter(port))
					{
						notValid = true;
						RhinoApp.WriteLine(validationPair.Value);
						break;
					}
				}

				portIsNotValid = notValid;
			}

			return true;
		}
	}
}
