using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Crash.Client;
using Crash.Properties;

using Eto.Drawing;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using Rhino.UI;

[assembly: InternalsVisibleTo("Crash.UI.Tests")]
namespace Crash.UI.JoinModel
{

	/// <summary>The Join View Model</summary>
	internal sealed class JoinViewModel : BaseViewModel, IDisposable
	{
		private const string PREVIOUS_MODELS_KEY = "PREVIOUS_SHARED_MODELS";

		internal ObservableCollection<SharedModel> SharedModels { get; private set; }
		internal SharedModel AddModel { get; private set; }
		internal ObservableCollection<SharedModel> AddModels => new ObservableCollection<SharedModel>(new List<SharedModel> { AddModel });

		public event EventHandler OnLoaded;

		internal JoinViewModel()
		{
			SharedModels = new();

			SetAddModel();
			LoadSharedModels();

			RhinoDoc.BeginSaveDocument += SaveSharedModels;
		}


		JsonSerializerOptions opts = new JsonSerializerOptions()
		{
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			IncludeFields = false
		};

		internal void SaveSharedModels(object sender, DocumentSaveEventArgs args)
		{
			var json = JsonSerializer.Serialize(SharedModels, opts);

			if (string.IsNullOrEmpty(json))
				return;

			CrashPlugin.Instance.Settings.SetString(PREVIOUS_MODELS_KEY, json);
		}

		private void LoadSharedModels()
		{
			var inst = CrashPlugin.Instance;
			if (inst is null) return;

			if (inst.Settings.TryGetString(PREVIOUS_MODELS_KEY, out var json))
			{
				var deserial = JsonSerializer.Deserialize<List<SharedModel>>(json, opts);
				if (deserial is null) return;

				foreach (var sharedModel in deserial)
				{
					AddSharedModel(sharedModel);
				}
			}
		}

		internal bool ModelIsNew(SharedModel model)
		{
			var alreadyExists = SharedModels.Select(sm => sm.ModelAddress.ToUpperInvariant())
										.Contains(model.ModelAddress.ToUpperInvariant());
			if (alreadyExists)
				return false;

			if (string.IsNullOrEmpty(model.ModelAddress))
				return false;

			return true;
		}

		internal async Task AddSharedModel(SharedModel model)
		{
			if (ModelIsNew(model))
			{
				SharedModels.Add(new SharedModel(model));
				SetAddModel();

				await model.Connect();
			}
		}

		internal void SetAddModel()
		{
			AddModel = new SharedModel() { Loaded = true, ModelAddress = "" };
		}

		public void Dispose()
		{
			RhinoDoc.BeginSaveDocument -= SaveSharedModels;
		}

	}

}
