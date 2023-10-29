using System.Runtime.Serialization;
using System.Text.Json;

using Rhino;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Runtime;

namespace Crash.Handlers.Changes
{
	/// <summary>A Change encapsulating Rhino Geometry</summary>
	public sealed class GeometryChange : IChange
	{
		public const string ChangeType = "Crash.GeometryChange";

		public GeometryBase Geometry { get; private set; }

		public DateTime Stamp { get; private set; } = DateTime.UtcNow;

		public Guid Id { get; internal set; }

		public string? Owner { get; private set; }

		public string? Payload { get; private set; }

		public ChangeAction Action { get; set; }

		public string Type => ChangeType;

		/// <summary>Inheritance Constructor</summary>
		public static GeometryChange CreateFrom(IChange change)
		{
			try
			{
				var packet = JsonSerializer.Deserialize<PayloadPacket>(change.Payload);
				var geometry = CommonObject.FromJSON(packet.Data) as GeometryBase;
				if (packet.Transform.IsValid() && false)
				{
					// TODO : Fix Transforms!
					var transform = packet.Transform.ToRhino();
					if (transform.IsValid && false)
					{
						geometry.Transform(transform);
					}
				}

				return new GeometryChange
				       {
					       Geometry = geometry,
					       Stamp = change.Stamp,
					       Id = change.Id,
					       Owner = change.Owner,
					       Payload = change.Payload,
					       Action = change.Action
				       };
			}
			catch (SerializationException serialEx)
			{
				RhinoApp.WriteLine(serialEx.Message);
			}
			catch (Exception ex)
			{
				RhinoApp.WriteLine(ex.Message);
			}

			return null;
		}

		/// <summary>Creates a new Geometry Change</summary>
		public static GeometryChange CreateNew(GeometryBase geometry, string userName)
		{
			return new GeometryChange
			       {
				       Geometry = geometry,
				       Stamp = DateTime.UtcNow,
				       Id = Guid.NewGuid(),
				       Owner = userName,
				       Payload = geometry?.ToJSON(new SerializationOptions()),
				       Action = ChangeAction.Add | ChangeAction.Temporary
			       };
		}

		/// <summary>Creates a new Change for sending to the server</summary>
		/// <param name="id">The id of the change to remove</param>
		/// <param name="user">The user who deleted this change</param>
		/// <param name="action">The action for the Change</param>
		/// <param name="payload">The payload if an add, otherwise none.</param>
		/// <returns>A change suitable for sending to the server</returns>
		public static Change CreateChange(Guid id, string user, ChangeAction action, string? payload = null)
		{
			return new Change
			       {
				       Id = id,
				       Owner = user,
				       Action = action,
				       Payload = payload,
				       Type = ChangeType
			       };
		}
	}
}
