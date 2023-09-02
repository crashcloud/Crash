using System.Text.Json.Serialization;

using Crash.Common.Communications;
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
		public SharedModel() { }

		internal SharedModel(SharedModel sharedModel)
		{
			Loaded = sharedModel.Loaded;
			modelAddress = sharedModel.ModelAddress;
			Users = sharedModel.Users;
		}

		// Conditionals
		[JsonIgnore] internal bool? Loaded { get; set; } = false;

		[JsonIgnore]
		public Bitmap Signal => Loaded switch
		                        {
			                        true  => Icons.wifi.ToEto(),
			                        false => Icons.wifi_off.ToEto(),
			                        null  => Icons.wifi_unstable.ToEto()
		                        };

		[JsonIgnore] public Bitmap UserIcon => Icons.user.ToEto();

		[JsonIgnore] public string UserCount => Users?.Length.ToString() ?? "0";

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

		public event Action<Change[]> OnInitialize;

		public async Task<bool> Connect()
		{
			var hub = new HubConnectionBuilder()
			          .WithUrl($"{ModelAddress}/Crash").AddJsonProtocol()
			          .AddJsonProtocol(opts => CrashClient.JsonOptions())
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

		internal event EventHandler OnAddressChanged;

		private class SharedModelRetryPolicy : IRetryPolicy
		{
			public TimeSpan? NextRetryDelay(RetryContext retryContext)
			{
				return TimeSpan.FromMilliseconds(100);
			}
		}
	}
}
