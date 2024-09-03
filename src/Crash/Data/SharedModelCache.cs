using System.Text.Json;

using Crash.Common.App;
using Crash.Common.Document;

namespace Crash.Data
{
	public class SharedModelCache
	{

		private record SharedModelData(List<SharedModel> SharedModels) : ICrashInstance;

		private const string SharedModelCacheFileName = "SharedModelCache.json";

		private static JsonSerializerOptions JsonOptions => new()
		{
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			IncludeFields = false
		};

		public static bool TryLoadSharedModels(out List<SharedModel> sharedModels)
		{
			sharedModels = new();
			try
			{
				if (CrashData.TryReadFileData<List<SharedModel>>(SharedModelCacheFileName, out var foundModels))
				{
					foreach (var sharedModel in foundModels)
					{
						if (sharedModel is null) continue;
						sharedModels.Add(sharedModel);
					}
					return true;
				}
			}
			catch { }

			sharedModels = new();
			sharedModels.Add(new SharedModel { ModelAddress = "http://localhost:8080" });

			return sharedModels is not null && sharedModels.Count > 0;
		}


		public static bool TrySaveSharedModels(List<SharedModel> sharedModels)
		{
			try
			{
				var crashDataDir = CrashData.CrashDataDirectory;
				var json = JsonSerializer.Serialize(sharedModels, JsonOptions);
				if (string.IsNullOrEmpty(json)) return false;

				CrashData.WriteFile(json, SharedModelCacheFileName);
			}
			catch { }
			return false;
		}

		public static bool TryGetSharedModelsData(CrashDoc crashDoc, out List<SharedModel> activeModels)
		{
			activeModels = new();
			if (!CrashInstances.TryGetInstance(crashDoc, out SharedModelData sharedModelData)) return false;
			activeModels = sharedModelData.SharedModels;
			return activeModels?.Count > 0;
		}

	}
}
