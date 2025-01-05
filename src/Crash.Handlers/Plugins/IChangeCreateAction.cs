namespace Crash.Handlers.Plugins
{
	/// <summary>Describes how to Convert an Event into a Change</summary>
	public interface IChangeCreateAction
	{
		/// <summary>The Action this <see cref="IChangeCreateAction" /> responds to</summary>
		ChangeAction Action { get; }

		/// <summary>Attempts to convert an Event into an <see cref="IChange" /></summary>
		public bool TryConvert(object? sender, CreateRecieveArgs crashArgs, out IEnumerable<Change> changes);

		/// <summary>Tests Event Args for Conversion</summary>
		public bool CanConvert(object? sender, CreateRecieveArgs crashArgs);
	}
}
