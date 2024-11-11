
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Eto.Drawing;

namespace Crash.Handlers.Data
{

	public interface ISharedModel
	{
		double UserCount { get; }

		string ModelAddress { get; }

		DateTime LastOpened { get; }
	}

	public record AddModel : ISharedModel
	{
		public double UserCount => 0;

		public string ModelAddress => "Add New Model";

		public DateTime LastOpened => DateTime.UtcNow;

		public override string ToString() => "Add";
	}

	public record SandboxModel : ISharedModel
	{
		public double UserCount { get; set; }

		public string ModelAddress => "Try Crash";

		public DateTime LastOpened { get; set; } = DateTime.UtcNow;

		public override string ToString() => "[Sandbox]";
	}

	public record DebugModel : ISharedModel
	{
		public double UserCount { get; set; }

		public string ModelAddress => "http://0.0.0.0:8080";

		public DateTime LastOpened { get; set; } = DateTime.UtcNow;

		public override string ToString() => "<Debug>";
	}

	public sealed class SharedModel : ISharedModel, IEquatable<SharedModel>
	{
		public double UserCount { get; set; }

		[JsonConverter(typeof(BitmapConverter))]
		public Bitmap Thumbnail { get; set; }

		public string ModelAddress { get; set; }

		public DateTime LastOpened { get; set; } = DateTime.UtcNow;

		public SharedModel() { }

		public SharedModel(string address)
		{
			ModelAddress = address;
		}

		public bool Equals(SharedModel? other)
		{
			if (other is null) return false;
			if (!AddressIsSame(ModelAddress, other.ModelAddress)) return false;
			// C Sykes 10th Nov 2024
			// I think Last Opened and the Bitmap are irrelivent

			return true;
		}

		public override string ToString() => ModelAddress ?? "+";

		private const string pattern = @"[\/\\";
		private static bool AddressIsSame(string addr1, string addr2)
		{
			try
			{
				var uri1 = new Uri(addr1);
				var uri2 = new Uri(addr2);
				return uri1.Equals(uri2);
			}
			catch { }
			return false;
		}

	}

}
