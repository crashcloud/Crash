using System.Net.Http;

using Crash.Common.Document;
using Crash.Handlers;

using Eto.Forms;

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

		/// <summary>Checks to see if the user is currently in a Shared Model</summary>
		internal static bool InSharedModel(CrashDoc crashDoc)
		{
			if (crashDoc?.LocalClient?.IsConnected != true)
			{
				RhinoApp.WriteLine("You aren't in a shared model.");
				return false;
			}

			return true;
		}

		/// <summary>
		///     Checks if already connected, and prompts user
		///     to take action if connected.
		/// </summary>
		/// <returns>True if already connected</returns>
		internal static async Task<bool> CheckAlreadyConnectedAsync(CrashDoc crashDoc)
		{
			try
			{
				if (crashDoc?.LocalClient?.IsConnected == true)
				{
					RhinoApp.WriteLine("You are currently part of a Shared Model Session.");
					return RhinoApp.RunScript(LeaveSharedModel.Instance.EnglishName, true);
				}

				return true;
			}
			catch (Exception e)
			{
				await CrashDocRegistry.DisposeOfDocumentAsync(crashDoc);
				return true;
			}
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
			var message = "An unexplained exception occured, try again.";

			try
			{
				var uri = GetCleanUri(url);
				crashDoc.LocalClient.RegisterConnection(userName, uri);

				await crashDoc.LocalClient.StartLocalClientAsync();
				return true;
			}
			catch (HttpRequestException)
			{
				message = "Server was not found! Please try retyping the address.";
			}
			catch (UriFormatException)
			{
				message = "The given address was invalid! Please try retying the address";
			}
			catch (InvalidOperationException)
			{
				message = "There was an issue with the local client";
			}
			catch (Exception ex)
			{
				message += $", {ex.Message}";
			}

			RhinoApp.InvokeOnUiThread(() =>
			                          {
				                          LoadingUtils.Close();
				                          MessageBox.Show(message, MessageBoxButtons.OK);
			                          });
			return false;
		}

		private static Uri GetCleanUri(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				throw new UriFormatException("Url is empty");
			}

			var cleanUrl = url;

			if (!cleanUrl.EndsWith("/Crash") &&
			    !cleanUrl.EndsWith("\\Crash"))
			{
				cleanUrl = $"{cleanUrl}/Crash";
			}

			cleanUrl = cleanUrl.Replace("//Crash", "/Crash");

			return new Uri(cleanUrl, false);
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
