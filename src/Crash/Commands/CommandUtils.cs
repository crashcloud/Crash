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
		internal static async Task<bool> TryLeaveModel(CrashDoc crashDoc)
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
			catch
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


		internal static async Task<bool> StartLocalClient(CrashDoc crashDoc, string url, bool headless = false)
		{
			if (crashDoc is null) return false;
			var userName = crashDoc.Users?.CurrentUser.Name ?? string.Empty;

			var uriResult = TryGetCleanUri(url, out var uri);
			if (!ParseConnectionResult(uriResult, url, headless)) return false;

			var connectionResult = crashDoc.LocalClient.RegisterConnection(userName, uri);
			if (!ParseConnectionResult(connectionResult, url, headless)) return false;

			var startResult = await crashDoc.LocalClient.StartLocalClientAsync();
			if (!ParseConnectionResult(startResult, url, headless)) return false;

			return true;
		}

		private static bool ParseConnectionResult(Exception result, string url, bool headless = false)
		{
			var message = "An unexplained exception occured, try again.";
			var registerResult = result switch
			{
				HttpRequestException => $"Server was not found at {url}! Please try retyping the address.",
				UriFormatException => $"The given address ({url}) was invalid! Please try retying the address",
				NotSupportedException => $"The given address ({url}) was invalid! Please try retying the address",
				InvalidOperationException => "There was an issue with the local client",
				Exception ex => $"An unexplained exception occured, try again. ({ex.Message}), please contact a developer for assistance.",
				_ => string.Empty
			};

			if (!string.IsNullOrEmpty(registerResult))
			{
				message = registerResult;
				AlertUser(message, headless);
				RhinoApp.Idle += ReOpenJoinWindow;

				return false;
			}

			return true;
		}

		internal static void AlertUser(string message, bool headless = false)
		{
			RhinoApp.InvokeOnUiThread(() =>
									{
										LoadingUtils.Close();
										if (headless)
										{
											RhinoApp.WriteLine(message);
										}
										else
										{
											MessageBox.Show(message, MessageBoxButtons.OK);
										}
									});
		}

		private static void ReOpenJoinWindow(object? sender, EventArgs e)
		{
			RhinoApp.Idle -= ReOpenJoinWindow;
			RhinoApp.RunScript(JoinSharedModel.Instance.EnglishName, false);
		}

		private static Exception? TryGetCleanUri(string url, out Uri uri)
		{
			try
			{
				if (string.IsNullOrEmpty(url))
				{
					uri = null;
					return new UriFormatException("Url is empty");
				}

				var cleanUrl = url;

				if (!cleanUrl.EndsWith("/Crash") &&
					!cleanUrl.EndsWith("\\Crash"))
				{
					cleanUrl = $"{cleanUrl}/Crash";
				}

				cleanUrl = cleanUrl.Replace("//Crash", "/Crash");

				uri = new Uri(cleanUrl);
				return null;
			}
			catch (Exception ex)
			{
				uri = null;
				return ex;
			}
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
