using System.Text.Json;

using Crash.Common.App;
using Crash.Common.Document;

namespace Crash.Handlers.Data
{
	public class SharedModelCache
	{

		public record SharedModelData
		{
			public List<SharedModel> Models { get; set; }

			public SharedModelData()
			{

			}

			public SharedModelData(List<SharedModel> models)
			{
				Models = models;
			}
		}

		private const string SharedModelCacheFileName = "SharedModelCache.json";

		private static JsonSerializerOptions GetJsonOptions()
		{
			var defaultOptions = new JsonSerializerOptions(JsonSerializerOptions.Default);
			defaultOptions.IgnoreReadOnlyFields = true;
			defaultOptions.IgnoreReadOnlyProperties = true;
			defaultOptions.IncludeFields = false;
			defaultOptions.WriteIndented = true;
			defaultOptions.PropertyNameCaseInsensitive = true;
			defaultOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

			return defaultOptions;
		}

		public static bool TryLoadSharedModels(out List<SharedModel> sharedModels)
		{
			sharedModels = new();
			try
			{
				if (CrashData.TryReadFileData<SharedModelData>(SharedModelCacheFileName, out var data))
				{
					foreach (var sharedModel in data.Models)
					{
						if (string.IsNullOrEmpty(sharedModel?.ModelAddress)) continue;
						if (sharedModels.Any(sm => SharedModel.Equals(sm, sharedModel))) continue;
						sharedModels.Add(sharedModel);
					}
				}
			}
			catch
			{
				return false;
			}

			return true;
		}


		public static bool TrySaveSharedModels(List<SharedModel> sharedModels)
		{
			try
			{
				if (sharedModels is null) return false;
				var crashDataDir = CrashData.CrashDataDirectory;
				var options = GetJsonOptions();
				var json = JsonSerializer.Serialize(new SharedModelData(sharedModels), options);
				if (string.IsNullOrEmpty(json)) return false;

				CrashData.WriteFile(json, SharedModelCacheFileName);
				return true;
			}
			catch { }
			return false;
		}

		public static bool TryGetSharedModelsData(CrashDoc crashDoc, out List<SharedModel> activeModels)
		{
			activeModels = new();
			if (!TryLoadSharedModels(out var sharedModels)) return false;
			activeModels = sharedModels;
			return activeModels?.Count > 0;
		}

	}
}
