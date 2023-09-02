using System.Text.Json;

using Crash.Common.Serialization;
using Crash.Common.View;

namespace Crash.Common.Changes
{
	/// <summary>Captures a Change of a Camera</summary>
	public struct CameraChange : IChange
	{
		public const string ChangeType = "Crash.CameraChange";

		public Camera Camera { get; private set; }

		public DateTime Stamp { get; private set; }

		public Guid Id { get; private set; }

		public string? Owner { get; private set; }

		public string? Payload { get; private set; }

		public ChangeAction Action { get; set; } = ChangeAction.Add;

		public readonly string Type => ChangeType;

		public CameraChange() { }

		/// <summary>Creates a new Camera Change from an IChange</summary>
		public static CameraChange CreateFrom(IChange change)
		{
			return new CameraChange
			       {
				       Camera = JsonSerializer.Deserialize<Camera>(change.Payload),
				       Stamp = change.Stamp,
				       Id = change.Id,
				       Owner = change.Owner,
				       Payload = change.Payload,
				       Action = ChangeAction.Add
			       };
		}

		/// <summary>Creates a new Camera Change from the required parts</summary>
		public static CameraChange CreateNew(Camera camera, string userName)
		{
			return new CameraChange
			       {
				       Camera = camera,
				       Stamp = DateTime.UtcNow,
				       Id = Guid.NewGuid(),
				       Owner = userName,
				       Payload = JsonSerializer.Serialize(camera, Options.Default),
				       Action = ChangeAction.Add
			       };
		}
	}
}
