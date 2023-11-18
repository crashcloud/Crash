using Rhino.PlugIns;

namespace Crash.Handlers.Plugins
{
	/// <summary>
	///     All CrashPlugins should inherit from this base
	/// </summary>
	public abstract class CrashPluginBase : PlugIn
	{
		/// <summary>The Id of the Crash Plugin. DO NOT reuse this!</summary>
		protected const string CrashPluginId = "53CB2393-C71F-4079-9CEC-97464FF9D14E";

		/// <summary>Contains all of the Change Definitions of this PlugIn</summary>
		public readonly Stack<IChangeDefinition> Changes;

		/// <summary>Ensure to inherit and call this</summary>
		protected CrashPluginBase()
		{
			Changes = new Stack<IChangeDefinition>();
		}

		/// <summary>Forces LoadTimes to be after the main Crash Plugin</summary>
		public sealed override PlugInLoadTime LoadTime
			=> Id == new Guid(CrashPluginId) ? PlugInLoadTime.AtStartup : PlugInLoadTime.WhenNeeded;

		/// <summary>Registers Change Definitions to Crash</summary>
		protected void RegisterChangeSchema(IChangeDefinition changeDefinition)
		{
			if (changeDefinition is null)
			{
				return;
			}

			Changes.Push(changeDefinition);
		}
	}
}
