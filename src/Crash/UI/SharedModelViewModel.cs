using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Crash.Client;
using Crash.Common.Document;
using Crash.Properties;

using Eto.Drawing;
using Eto.Forms;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using Rhino.UI;

using static Crash.UI.SharedModelViewModel;

[assembly:InternalsVisibleTo("Crash.UI.Tests")]
namespace Crash.UI
{

	internal sealed class SharedModelViewModel : BaseViewModel, IDisposable
	{
		private const string PREVIOUS_MODELS_KEY = "PREVIOUS_SHARED_MODELS";

		internal ObservableCollection<SharedModel> SharedModels { get; private set; }
		internal ObservableCollection<SharedModel> AddModels { get; private set; }

		public event EventHandler OnLoaded;

		[Serializable]
		public sealed class SharedModel
		{
			[JsonIgnore]
			internal SharedModelViewModel ViewModel;

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
			public string ModelAddress { get; set; }
			public string[] Users { get; set; } = Array.Empty<string>();

			public SharedModel() { }

			public SharedModel(SharedModelViewModel sharedModel)
			{
				ViewModel = sharedModel;
			}

			public event Action<Change[]> OnInitialize;
			public async Task LoadModel()
			{
				HubConnection hub = new HubConnectionBuilder()
					   .WithUrl($"{this.ModelAddress}/Crash").AddJsonProtocol()
					   .AddJsonProtocol((opts) => CrashClient.JsonOptions())
					   .WithAutomaticReconnect(new[] { TimeSpan.FromMilliseconds(10),
											   TimeSpan.FromMilliseconds(100),
											   TimeSpan.FromSeconds(1),
											   TimeSpan.FromSeconds(2) })
					   .Build();
				hub.On<Change[]>("Initialize", (Changes) => OnInitialize?.Invoke(Changes));
				OnInitialize += (changes) =>
				{
					var uniqueUsers = changes.Select(c => c.Owner).ToHashSet().ToArray();
					this.Users = uniqueUsers;
					this.Loaded = true;
				};
				hub.Closed += async (args) =>
				{
					this.Loaded = false;
					ViewModel.OnLoaded?.Invoke(this, null);
					await Task.CompletedTask;
				};
				hub.Reconnecting += async (args) =>
				{
					this.Loaded = null;
					ViewModel.OnLoaded?.Invoke(this, null);
					await Task.CompletedTask;
				};

				try
				{
					await hub.StartAsync();
					ViewModel?.OnLoaded?.Invoke(this, null);
				}
				catch (Exception ex)
				{
					;
				}
			}
		}


		internal SharedModelViewModel()
		{
			SharedModels = new();
			AddModels = new();
			AddModels.Add(new SharedModel() { Loaded = true, ModelAddress = "" });
			LoadSharedModels();

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

				foreach(var sharedModel in deserial)
				{
					AddSharedModel(sharedModel);
				}
			}
		}

		internal async Task AddSharedModel(SharedModel model)
		{
			if (string.IsNullOrEmpty(model?.ModelAddress)) return;

			var models = SharedModels.ToArray();
			bool alreadyExists = models.Select(sm => sm.ModelAddress.ToLowerInvariant()).Contains(model.ModelAddress.ToLowerInvariant());
			if (!alreadyExists)
			{
				model.ViewModel = this;
				 
				SharedModels.Add(model);
				await model.LoadModel();
			}
		}

	}

}
