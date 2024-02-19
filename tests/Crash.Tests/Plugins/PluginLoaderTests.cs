using System.Collections;

using Crash.Plugins;

namespace Crash.Tests.Plugins
{
	[RhinoFixture]
	public sealed class PluginLoaderTests
	{
		public static IEnumerable PluginSources
		{
			get
			{
				yield return "Example Plugin Directory";
			}
		}

		[TestCaseSource(nameof(PluginSources))]
		public void LoadPlugin(string pluginSource)
		{
			var loader = new CrashPluginLoader(new[] { pluginSource });
			var changeDefinitions = loader.LoadCrashPlugins();
			Assert.That(changeDefinitions, Is.Not.Null.Or.Empty);
			Assert.That(changeDefinitions.Count, Is.GreaterThan(0));
		}
	}
}
