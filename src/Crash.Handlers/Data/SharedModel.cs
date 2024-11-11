
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

		ModelRenderState State { get; set; }
	}

	public record AddModel : ISharedModel
	{
		public double UserCount => 0;

		public string ModelAddress => "Add New Model";

		public DateTime LastOpened => DateTime.UtcNow;

		public ModelRenderState State { get; set; }

		public override string ToString() => "Add";
	}

	public record SandboxModel : ISharedModel
	{
		public double UserCount { get; set; }

		public string ModelAddress => "Try Crash Free!";

		public DateTime LastOpened { get; set; } = DateTime.UtcNow;

		public ModelRenderState State { get; set; }

		public override string ToString() => "[Sandbox]";
	}

	public record DebugModel : ISharedModel
	{
		public double UserCount { get; set; }

		public string ModelAddress => "http://localhost:8080";

		public DateTime LastOpened { get; set; } = DateTime.UtcNow;

		public ModelRenderState State { get; set; }

		public override string ToString() => "<Debug>";
	}

	public sealed class SharedModel : ISharedModel, IEquatable<ISharedModel>
	{
		public double UserCount { get; set; }

		[JsonConverter(typeof(BitmapConverter))]
		public Bitmap Thumbnail { get; set; }

		public string ModelAddress { get; set; }

		public DateTime LastOpened { get; set; } = DateTime.UtcNow;

		public ModelRenderState State { get; set; }

		public SharedModel() { }

		public SharedModel(string address)
		{
			ModelAddress = address;
		}

		public bool Equals(object? other) => other is ISharedModel model && Equals(model);

		public bool Equals(ISharedModel? other) => Equals(this, other);

		public static bool Equals(ISharedModel left, ISharedModel right)
		{
			if (!AddressIsSame(left?.ModelAddress, right?.ModelAddress)) return false;
			// C Sykes 10th Nov 2024
			// I think Last Opened and the Bitmap are irrelivent
			return true;
		}

		public override string ToString() => ModelAddress ?? "+";

		private const string pattern = @"[\/\\";
		private static bool AddressIsSame(string addr1, string addr2)
		{
			if (string.Equals(addr1, addr2, StringComparison.CurrentCultureIgnoreCase)) return true;
			if (string.IsNullOrEmpty(addr1)) return false;
			if (string.IsNullOrEmpty(addr2)) return false;
			// Note : This is very slow
			try
			{
				Uri.TryCreate(addr1, UriKind.Absolute, out var uri1);
				Uri.TryCreate(addr2, UriKind.Absolute, out var uri2);
				return uri1?.Equals(uri2) == true;
			}
			catch { }
			return false;
		}

	}

}
