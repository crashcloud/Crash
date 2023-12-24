using Crash.Common.Changes;
using Crash.Common.Document;
using Crash.Handlers.Authentication;

using Rhino.Commands;

namespace Crash.Commands
{
	/// <summary>Command to Release Changes</summary>
	[CommandStyle(Style.DoNotRepeat | Style.NotUndoable)]
	public sealed class Release : AsyncCommand
	{
		public Release()
		{
			Instance = this;
		}

		public static Release Instance { get; private set; }

		public override string EnglishName => "Release";

		protected override async Task<Result> RunCommandAsync(RhinoDoc doc, CrashDoc crashDoc, RunMode mode)
		{
			GetOpenIdToken();
			if (!CommandUtils.InSharedModel(crashDoc))
			{
				RhinoApp.WriteLine("You aren't in a shared model.");
				return Result.Failure;
			}

			var doneChange = DoneChange.GetDoneChange(crashDoc.Users.CurrentUser.Name);

			await crashDoc.LocalClient.PushChangeAsync(doneChange);

			doc.Objects.UnselectAll();
			doc.Views.Redraw();

			return Result.Success;
		}

		private async Task<string> GetOpenIdToken()
		{
			var secret = "/*Put Secret Here*/";
			var id = "crash";

			var cancelToken = new CancellationTokenSource(5000).Token;
			var token = await ClientAuthentication.GetRhinoToken(id, secret, cancelToken);

			var openId = token.Item1;
			var oauth2 = token.Item2;

			return openId.RawToken;
		}
	}
}
