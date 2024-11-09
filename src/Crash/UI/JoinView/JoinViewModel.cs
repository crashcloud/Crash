using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Crash.Handlers.Data;

using Rhino.UI;

[assembly: InternalsVisibleTo("Crash.UI.Tests")]

namespace Crash.UI.JoinView
{
	/// <summary>The Join View Model</summary>
	internal sealed class JoinViewModel : BaseViewModel
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
				SharedModels = new ObservableCollection<SharedModel>(sharedModels);
			}
			else
			{
				// TODO : Only in Debug Mode?
				SharedModels = new ObservableCollection<SharedModel>();
			}

#if DEBUG
			SharedModels.Add(new() { ModelAddress = "https://cheddar.com" });
			SharedModels.Add(new() { ModelAddress = "https://192.168.1.1:7070" });
			SharedModels.Add(new() { ModelAddress = "https://edam.com/" });
			SharedModels.Add(new() { ModelAddress = "https://brie.co.uk" });
			SharedModels.Add(new() { ModelAddress = "https://gorgonzola.io/tasty/" });
			SharedModels.Add(new() { ModelAddress = "https://parmesan.app" });
#endif
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

		internal string VersionText
		{
			get
			{
				var assem = typeof(JoinViewModel).Assembly;
				var name = assem.GetName();
				return $"Version {name.Version} - wip";
			}
		}
	}
}
