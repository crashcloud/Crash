namespace Crash.Handlers.Plugins
{
	public interface CrashPlugin
	{
		/// <summary>The Id of the Plugin</summary>
		public Guid Id { get; }

		/// <summary>The Name of the Plugin</summary>
		public string Name { get; }

		/// <summary>Contains all of the Change Definitions of this PlugIn</summary>
		IEnumerable<IChangeDefinition> Changes { get; }
	}
}
