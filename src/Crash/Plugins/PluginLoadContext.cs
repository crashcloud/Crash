#if NET7_0_OR_GREATER
using System.Reflection;
using System.Runtime.Loader;
#endif

namespace Crash.Plugins
{
#if NET7_0_OR_GREATER
	internal class PluginLoadContext : AssemblyLoadContext
	{
		private readonly AssemblyDependencyResolver _resolver;

		public PluginLoadContext(string pluginPath)
		{
			_resolver = new AssemblyDependencyResolver(pluginPath);
		}

		protected override Assembly? Load(AssemblyName assemblyName)
		{
			var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
			if (assemblyPath is not null)
			{
				return LoadFromAssemblyPath(assemblyPath);
			}

			return null;
		}

		protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
		{
			var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
			if (libraryPath is not null)
			{
				return LoadUnmanagedDllFromPath(libraryPath);
			}

			return IntPtr.Zero;
		}
	}
#endif
}
