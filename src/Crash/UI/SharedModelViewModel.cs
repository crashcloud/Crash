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
namespace Crash.UI
{

	internal sealed class SharedModelViewModel : BaseViewModel, IDisposable
	{
		private const string PREVIOUS_MODELS_KEY = "PREVIOUS_SHARED_MODELS";

		internal ObservableCollection<SharedModel> SharedModels { get; private set; }
		internal SharedModel AddModel { get; private set; }
		internal ObservableCollection<SharedModel> AddModels => new ObservableCollection<SharedModel>(new List<SharedModel> { AddModel });

		public event EventHandler OnLoaded;

		[Serializable]
		public sealed class SharedModel
		{
			// Conditionals
			[JsonIgnore]
			internal bool? Loaded { get; set; } = false;
			[JsonIgnore]
			public Bitmap Signal => Loaded switch
			{
				true => Icons.wifi.ToEto(),
				false => Icons.wifi_off.ToEto(),
				null => Icons.wifi_unstable.ToEto(),
			};
			[JsonIgnore]
			public Bitmap UserIcon => Icons.user.ToEto();

			// View Properties
			[JsonIgnore]
			public string UserCount => Users?.Length.ToString() ?? "0";

			// Serialized Proeprties
			// [JsonConverter]
			public Bitmap Thumbnail { get; set; }

			private string modelAddress { get; set; }
			public string ModelAddress
			{
				get => modelAddress;
				set
				{
					modelAddress = value;
					OnAddressChanged?.Invoke(this, EventArgs.Empty);
				}
			}
			public string[] Users { get; set; } = Array.Empty<string>();

			public SharedModel() { }

			internal SharedModel(SharedModel sharedModel)
			{
				this.Loaded = sharedModel.Loaded;
				this.modelAddress = sharedModel.ModelAddress;
				this.Users = sharedModel.Users;
			}

			public event Action<Change[]> OnInitialize;
			public async Task<bool> Connect()
			{
				HubConnection hub = new HubConnectionBuilder()
					   .WithUrl($"{this.ModelAddress}/Crash").AddJsonProtocol()
					   .AddJsonProtocol((opts) => CrashClient.JsonOptions())
					   .WithAutomaticReconnect(new SharedModelRetryPolicy())
					   .Build();

				try
				{
					await hub.StartAsync();
					return true;
				}
				catch (Exception ex)
				{
					return false;
				}
			}

			private class SharedModelRetryPolicy : IRetryPolicy
			{
				public TimeSpan? NextRetryDelay(RetryContext retryContext)
					=> TimeSpan.FromMilliseconds(100);
			}

			internal event EventHandler OnAddressChanged;

		}


		internal SharedModelViewModel()
		{
			SharedModels = new();

			SetAddModel();
			LoadSharedModels();
			SharedModels.Add(new SharedModel() { ModelAddress = "address" });
			SharedModels.Add(new SharedModel() { ModelAddress = "1.2.3.4" });

			RhinoDoc.BeginSaveDocument += SaveSharedModels;
		}

		public void Dispose()
		{
			RhinoDoc.BeginSaveDocument -= SaveSharedModels;
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

			Crash.CrashPlugin.Instance.Settings.SetString(PREVIOUS_MODELS_KEY, json);
		}

		private void LoadSharedModels()
		{
			var inst = Crash.CrashPlugin.Instance;
			if (inst is null) return;

			if (inst.Settings.TryGetString(PREVIOUS_MODELS_KEY, out string json))
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
			bool alreadyExists = SharedModels.Select(sm => sm.ModelAddress.ToUpperInvariant())
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
	}

}
