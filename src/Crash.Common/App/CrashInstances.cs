using Crash.Common.Document;

namespace Crash.Common.App
{

	public interface ICrashInstance { }

	/// <summary>
	/// Stashes Instances against a Crash Doc
	/// This avoids statics
	/// </summary>
	public static class CrashInstances
	{

		public class CrashInstanceSet
		{
			private Dictionary<string, ICrashInstance> Instances { get; } = new();

			public bool TryGetInstance<TInstance>(out TInstance instance) where TInstance : ICrashInstance
			{
				instance = default;
				if (Instances.TryGetValue(typeof(TInstance).Name, out var crashInstance))
				{
					if (crashInstance is TInstance typedInstance)
					{
						instance = typedInstance;
						return true;
					}
				}

				return false;
			}

			public bool SetInstance(ICrashInstance instance)
			{
				if (instance is null) return false;
				if (Instances.ContainsKey(instance.GetType().Name)) return false;
				Instances.Add(instance.GetType().Name, instance);
				return true;
			}

			public bool Remove(Type type)
			{
				if (type is null) return false;
				return Instances.Remove(type.Name);
			}

		}

		private static Dictionary<CrashDoc, CrashInstanceSet> Instances { get; } = new();

		public static bool TryGetInstance<TInstance>(CrashDoc crashDoc, out TInstance instance) where TInstance : ICrashInstance
		{
			instance = default;
			if (crashDoc is null) return false;
			if (!Instances.TryGetValue(crashDoc, out CrashInstanceSet? instanceSet)) return false;
			return instanceSet.TryGetInstance(out instance);
		}

		public static bool TrySetInstance<TInstance>(CrashDoc crashDoc, TInstance instance) where TInstance : ICrashInstance
		{
			if (crashDoc is null) return false;
			if (!Instances.TryGetValue(crashDoc, out var instanceSet))
			{
				instanceSet = new CrashInstanceSet();
				instanceSet.SetInstance(instance);
				Instances.Add(crashDoc, instanceSet);
			}
			else
			{
				instanceSet.SetInstance(instance);
			}

			return true;
		}

		public static bool RemoveInstance(CrashDoc crashDoc, Type type)
		{
			if (crashDoc is null) return false;
			if (!Instances.TryGetValue(crashDoc, out var instanceSet)) return false;
			return instanceSet.Remove(type);
		}

		public static void DestroyInstance(CrashDoc crashDoc)
		{
			if (crashDoc is null) return;
			Instances.Remove(crashDoc);
		}

	}

}
