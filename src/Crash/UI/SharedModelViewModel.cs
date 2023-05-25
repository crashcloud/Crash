using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
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

namespace Crash.UI
{

	internal sealed class SharedModelViewModel : BaseViewModel
	{
		private const string PREVIOUS_MODELS_KEY = "PREVIOUS_SHARED_MODELS";

		internal List<SharedModel> SharedModels;

		public event EventHandler OnLoaded;

		[Serializable]
		public sealed class SharedModel
		{
			// Conditionals
			[JsonIgnore]
			internal bool Loaded { get; set; } = false;
			public Bitmap Signal => Loaded ? Icons.wifi.ToEto() : Icons.wifi_off.ToEto();
			public Bitmap UserIcon => Icons.user.ToEto();
			public Color BackgroundColour => Loaded ? new Color(0, 0, 255) : new Color(0, 255, 0);

			// View Properties
			[JsonIgnore]
			public string UserCount { get; set; } = "0";

			// Serialized Proeprties
			public Bitmap Thumbnail { get; set; }
			public string ModelAddress { get; set; }
			public string[] Users { get; set; } = Array.Empty<string>();

		}

		public event Action<Change[]> OnInitialize;
		public async Task LoadModel(SharedModel model)
		{
			HubConnection hub = new HubConnectionBuilder()
				   .WithUrl(model.ModelAddress).AddJsonProtocol()
				   .AddJsonProtocol((opts) => CrashClient.JsonOptions())
				   .WithAutomaticReconnect(new[] { TimeSpan.FromMilliseconds(10),
											   TimeSpan.FromMilliseconds(100),
											   TimeSpan.FromSeconds(1),
											   TimeSpan.FromSeconds(10) })
				   .Build();
			hub.On<Change[]>("Initialize", (Changes) => OnInitialize?.Invoke(Changes));
			OnInitialize += (changes) =>
			{
				model.Users = changes.Select(c => c.Owner).ToHashSet().ToArray() ?? Array.Empty<string>();
				model.Loaded = true;
			};

			try
			{
				await hub.StartAsync();
				OnLoaded?.Invoke(null, null);
			}
			catch(Exception ex)
			{
				;
			}
		}

		internal SharedModelViewModel()
		{
			SharedModels = new();
			LoadSharedModels();

			/*AddSharedModel(new SharedModel("http://mcneel.rhino.com"));
			AddSharedModel(new SharedModel("http://mcneel.yak.com"));
			AddSharedModel(new SharedModel("http://mcneel.rhino.com"));
			AddSharedModel(new SharedModel("http://localhost:5000"));*/

			AddSharedModel(new SharedModel() { ModelAddress = "http://localhost:5000/Crash" });
		}

		private void SaveSharedModels()
		{
			var json = JsonSerializer.Serialize(SharedModels);

			if (string.IsNullOrEmpty(json))
				return;

			Crash.CrashPlugin.Instance.Settings.SetString(PREVIOUS_MODELS_KEY, json);
		}

		private void LoadSharedModels()
		{
			if (Crash.CrashPlugin.Instance.Settings.TryGetString(PREVIOUS_MODELS_KEY, out string json))
			{
				var deserial = JsonSerializer.Deserialize<List<SharedModel>>(json);
				if (deserial is null) return;
				SharedModels = deserial;
			}
		}

		private async Task AddSharedModel(SharedModel model)
		{
			SharedModels.Add(model);
			await LoadModel(model);
		}

	}

}
