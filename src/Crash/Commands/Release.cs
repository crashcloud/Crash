using System.Threading;
using System.Threading.Tasks;

using Crash.Common.Document;
using Crash.Handlers;
using Crash.Handlers.Authentication;

using Rhino.Commands;


namespace Crash.Commands
{

	/// <summary>Command to Release Changes</summary>
	[CommandStyle(Style.DoNotRepeat | Style.NotUndoable)]
	public sealed class Release : Command
	{

		/// <summary>Default Constructor</summary>
		public Release()
		{
			Instance = this;
		}

		/// <inheritdoc />
		public static Release Instance { get; private set; }

		/// <inheritdoc />
		public override string EnglishName => "Release";

		/// <inheritdoc />
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			GetOpenIdToken();

			// TODO : Wait for response for data integrity check
			//CrashDoc? crashDoc = CrashDocRegistry.GetRelatedDocument(doc);
			//crashDoc?.LocalClient?.DoneAsync();

			return Result.Success;
		}

		private async Task<string> GetOpenIdToken()
		{
			string secret = "/*Put Secret Here*/";
			string id = "crash";
			
			var cancelToken = new CancellationTokenSource(5000).Token;
			var token = await ClientAuthentication.GetRhinoToken(id, secret, cancelToken);

			var openId = token.Item1;
			var oauth2 = token.Item2;

			return openId.RawToken;
		}
	}

}
