using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Crash.Client;
using Crash.Properties;

using Eto.Drawing;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using Rhino.UI;

namespace Crash.UI
{

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

		[JsonIgnore]
		public string UserCount => Users?.Length.ToString() ?? "0";

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

}
