using System.Runtime.Serialization;

using Crash.Common.Serialization;
using Crash.Geometry;

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
				var packet = PayloadSerialization.GetPayloadPacket(change.Payload);
				GeometryBase geometry = null;
				if (!string.IsNullOrEmpty(change.Payload))
				{
					geometry = CommonObject.FromJSON(packet.Data) as GeometryBase;
				}

				if (packet.Transform.IsValid() &&
				    !packet.Transform.Equals(CTransform.Unset) &&
				    geometry is not null)
				{
					var transform = packet.Transform.ToRhino();
					if (transform.IsValid)
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
			catch (InvalidOperationException invalidEx)
			{
				// TODO : This means CommonObject.JSON recieved an empty string!
				;
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
				       Payload = SerializeGeometry(geometry),
				       Action = ChangeAction.Add | ChangeAction.Temporary
			       };
		}

		/// <summary>Creates a new Change for sending to the server</summary>
		/// <param name="id">The id of the change to remove</param>
		/// <param name="user">The user who deleted this change</param>
		/// <param name="action">The action for the Change</param>
		/// <param name="payload">The payload if an add, otherwise none.</param>
		/// <returns>A change suitable for sending to the server</returns>
		public static Change CreateChange(Guid id, string user, ChangeAction action, GeometryBase? geometry = null)
		{
			return new Change
			       {
				       Id = id,
				       Owner = user,
				       Action = action,
				       Payload = SerializeGeometry(geometry),
				       Type = ChangeType
			       };
		}

		/// <summary>Serializes Geometry Correctly to Rhino version 7</summary>
		public static string SerializeGeometry(GeometryBase geometry)
		{
			if (geometry is null)
			{
				return string.Empty;
			}

			return geometry?.ToJSON(new SerializationOptions { RhinoVersion = 70, WriteUserData = false });
		}

		/// <summary>Sets the Geometry of the Change</summary>
		public void SetGeometry(GeometryBase geometry)
		{
			Geometry = geometry;
		}

	}
}
