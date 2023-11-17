using System.Text.Json;

using Crash.Common.Serialization;
using Crash.Geometry;

namespace Crash.Common.Changes
{
	/// <summary>Captures a Transformation Change</summary>
	public struct TransformChange : IChange
	{
		public const string ChangeType = "Crash.GeometryChange";

		/// <summary>The CTransform</summary>
		public CTransform Transform { get; private set; }

		public DateTime Stamp { get; private set; }

		public Guid Id { get; private set; }

		public string? Owner { get; private set; }

		public string? Payload { get; private set; }

		public string Type => ChangeType;

		public ChangeAction Action { get; set; } = ChangeAction.Transform;

		public TransformChange() { }

		public static Change CreateChange(Guid id, string userName, CTransform transform)
		{
			return new Change
			       {
				       Id = id,
				       Owner = userName,
				       Payload = JsonSerializer.Serialize(transform, Options.Default),
				       Stamp = DateTime.UtcNow,
				       Action = ChangeAction.Transform,
				       Type = ChangeType
			       };
		}

		/// <summary>IChange wrapping Constructor</summary>
		public static TransformChange CreateFrom(IChange change)
		{
			return new TransformChange
			       {
				       Transform = JsonSerializer.Deserialize<CTransform>(change.Payload),
				       Payload = change.Payload,
				       Owner = change.Owner,
				       Stamp = change.Stamp,
				       Id = change.Id,
				       Action = ChangeAction.Transform
			       };
		}

		/// <summary>Creates a Transform Change</summary>
		public static TransformChange CreateNew(CTransform transform, string userName, Guid id)
		{
			return new TransformChange
			       {
				       Id = id,
				       Owner = userName,
				       Payload = JsonSerializer.Serialize(transform, Options.Default),
				       Stamp = DateTime.UtcNow,
				       Action = ChangeAction.Transform
			       };
		}
	}
}
