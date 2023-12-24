using Crash.Common.Document;

namespace Crash.Common.Communications
{
	/// <summary>Abstracted Server Notification Contract</summary>
	public interface IEventDispatcher
	{
		/// <summary>
		///     Notifies the Dispatcher of any Events that should notify the server
		///     Avoid Subscribing to events and pinging the server yourself
		///     Wrap any related events with this method.
		/// </summary>
		/// <param name="changeAction">The ChangeAction</param>
		/// <param name="sender">The sender of the Event</param>
		/// <param name="args">The EventArgs</param>
		/// <param name="crashDoc">The associated RhinoDoc</param>
		Task NotifyServerAsync(ChangeAction changeAction, object sender, EventArgs args, CrashDoc crashDoc);
	}
}
