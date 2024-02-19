using System.Reflection;

using Crash.Handlers.Plugins;

namespace Crash.Plugins
{
	public class CrashPluginLoader
	{
		public const string Extension = ".mup";

		internal CrashPluginLoader(IEnumerable<string>? crashPluginLocations)
		{
			CrashPluginLocations = crashPluginLocations ?? Array.Empty<string>();
		}

		private IEnumerable<string> CrashPluginLocations { get; }


		internal List<IChangeDefinition> LoadCrashPlugins()
		{
			List<IChangeDefinition> changeDefinitions = new();
			foreach (var pluginDirectory in CrashPluginLocations)
			{
				if (!Directory.Exists(pluginDirectory))
				{
					continue;
				}

				var crashPluginExtensions = Directory.EnumerateFiles(pluginDirectory, $"*{Extension}")?.ToArray() ??
				                            Array.Empty<string>();

				if (crashPluginExtensions.Length == 0)
				{
					continue;
				}

				foreach (var pluginAssembly in crashPluginExtensions)
				{
					var loadedChangeDefinitions = LoadCrashPlugin(pluginAssembly);
					changeDefinitions.AddRange(loadedChangeDefinitions);
				}
			}

			return changeDefinitions;
		}

		private List<IChangeDefinition> LoadCrashPlugin(string crashAssembly)
		{
			var assembly = LoadPlugin(crashAssembly);
			var changeDefinitionTypes =
				assembly.ExportedTypes.Where(et => typeof(IChangeDefinition).IsAssignableFrom(et)).ToList();

			if (changeDefinitionTypes is null || changeDefinitionTypes.Count() == 0)
			{
				RhinoApp.WriteLine($"Could not find any type in {crashAssembly} that implements type {nameof(IChangeDefinition)}");
				return new List<IChangeDefinition>();
			}

			List<IChangeDefinition> newChangeDefinitions = new(changeDefinitionTypes.Count);
			foreach (var changeDefinitionType in changeDefinitionTypes)
			{
				var changeDefinition = Activator.CreateInstance(changeDefinitionType) as IChangeDefinition;
				if (changeDefinition is null)
				{
					RhinoApp.WriteLine($"Could not load {changeDefinitionType.Name}");
					continue;
				}

				newChangeDefinitions.Add(changeDefinition);
			}

			return newChangeDefinitions;
		}

		private Assembly LoadPlugin(string crashAssembly)
		{
#if NETFRAMEWORK
			return Assembly.LoadFrom(crashAssembly);

#elif NET7_0_OR_GREATER
			RhinoApp.WriteLine($"Loading commands from: {crashAssembly}");
			var loadContext = new PluginLoadContext(crashAssembly);
			return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(crashAssembly)));

#else
			RhinoApp.WriteLine("An Unsupported Framework has been loaded");
#endif
		}
	}
}
