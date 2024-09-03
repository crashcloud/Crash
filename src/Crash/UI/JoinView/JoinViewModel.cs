using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Crash.Handlers.Data;

using Rhino.UI;

[assembly: InternalsVisibleTo("Crash.UI.Tests")]

namespace Crash.UI.JoinView
{
	/// <summary>The Join View Model</summary>
	internal sealed class JoinViewModel
	{
		private const string PREVIOUS_MODELS_KEY = "PREVIOUS_SHARED_MODELS";


		private static JsonSerializerOptions JsonOptions => new()
		{
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			IncludeFields = false
		};

		internal JoinViewModel()
		{
			if (SharedModelCache.TryLoadSharedModels(out var sharedModels))
			{
				var notNull = (sharedModels?.Where(sm => sm is not null) ?? Array.Empty<SharedModel>())!.ToList();
				SharedModels = new ObservableCollection<SharedModel>(notNull);
			}
			else
			{
				SharedModels = new ObservableCollection<SharedModel>();
			}
		}

		internal ObservableCollection<SharedModel> SharedModels { get; }

		internal SharedModel TemporaryModel { get; set; }

		internal bool ModelIsNew(SharedModel model)
		{
			var alreadyExists = SharedModels.Select(sm => sm.ModelAddress.ToUpperInvariant())
											.Contains(model.ModelAddress.ToUpperInvariant());
			if (alreadyExists)
			{
				return false;
			}

			if (string.IsNullOrEmpty(model.ModelAddress))
			{
				return false;
			}

			return true;
		}

		internal bool AddSharedModel(SharedModel model)
		{
			if (ModelIsNew(model))
			{
				SharedModels.Add(model);
				return true;
			}

			return false;
		}
	}
}
