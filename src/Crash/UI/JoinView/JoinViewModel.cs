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
			var sharedModels = new List<ISharedModel>();
			sharedModels.Add(new AddModel());
			sharedModels.Add(new SandboxModel());

			if (SharedModelCache.TryLoadSharedModels(out var loadedSharedModels))
			{
				sharedModels.AddRange(loadedSharedModels);
			}

#if DEBUG
			sharedModels.Add(new DebugModel());
			sharedModels.Add(new SharedModel("https://cheddar.com"));
			sharedModels.Add(new SharedModel("https://192.168.1.1:7070"));
			sharedModels.Add(new SharedModel("https://edam.com/"));
			sharedModels.Add(new SharedModel("https://brie.co.uk"));
			sharedModels.Add(new SharedModel("https://gorgonzola.io/tasty/"));
			sharedModels.Add(new SharedModel("https://parmesan.app"));
#endif

			SharedModels = new(sharedModels);
		}

		internal ObservableCollection<ISharedModel> SharedModels { get; }

		internal SharedModel TemporaryModel { get; set; }

		internal bool ModelIsNew(ISharedModel model)
		{
			var alreadyExists = SharedModels.Select(sm => sm.ModelAddress?.ToUpperInvariant() ?? string.Empty)
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

		internal bool AddSharedModel(ISharedModel model)
		{
			if (ModelIsNew(model))
			{
				SharedModels.Add(model);
				return true;
			}

			return false;
		}

		internal void JoinSelected()
		{
			throw new NotImplementedException();
		}

		internal void ReloadAll()
		{
			throw new NotImplementedException();
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
