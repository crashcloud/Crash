namespace Crash.Handlers.Plugins
{
	/// <summary>
	///     Defines a Crash PlugIn
	/// </summary>
	public abstract class CrashPlugIn
	{
		/// <summary>Required constructor</summary>
		/// <param name="name"></param>
		/// <param name="id"></param>
		protected CrashPlugIn(string name, Guid id)
		{
			Name = name;
			Id = id;
			Changes = new List<IChangeDefinition>();
		}

		/// <summary>The Id of the Plugin</summary>
		public Guid Id { get; }

		/// <summary>The Name of the Plugin</summary>
		public string Name { get; }

		/// <summary>Contains all of the Change Definitions of this PlugIn</summary>
		private IEnumerable<IChangeDefinition> Changes { get; }
	}
}
