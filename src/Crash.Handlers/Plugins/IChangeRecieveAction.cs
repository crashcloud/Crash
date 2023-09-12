using Crash.Common.Document;

namespace Crash.Handlers.Plugins
{

	/// <summary>Handles recieved changes from the Server</summary>
	public interface IChangeRecieveAction
	{
		/// <summary>The Action this ICreateAction responds to</summary>
		bool CanRecieve(IChange change);

		/// <summary>Deserializes a Server Sent Change</summary>
		public Task OnRecieveAsync(CrashDoc crashDoc, Change recievedChange);
	}
}
