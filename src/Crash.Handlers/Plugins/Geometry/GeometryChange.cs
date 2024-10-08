﻿using System.Runtime.Serialization;
using System.Text.Json;

using Crash.Changes.Utils;
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
				if (!PayloadUtils.TryGetPayloadFromChange(change, out var packet))
				{
					// TODO : Do something about this, log it etc
					return null;
				}

				GeometryBase geometry = null;
				if (!string.IsNullOrEmpty(packet.Data))
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
				Payload = GeneratePayload(geometry),
				Action = ChangeAction.Add | ChangeAction.Temporary
			};
		}

		/// <summary>Creates a new Change for sending to the server</summary>
		/// <param name="id">The id of the change to remove</param>
		/// <param name="user">The user who deleted this change</param>
		/// <param name="action">The action for the Change</param>
		/// <param name="payload">The payload if an add, otherwise none.</param>
		/// <returns>A change suitable for sending to the server</returns>
		public static Change CreateChange(Guid id, string user, ChangeAction action, GeometryBase? geometry = null,
			Dictionary<string, string> updates = null)
		{
			return new Change
			{
				Id = id,
				Owner = user,
				Action = action,
				Payload = GeneratePayload(geometry, updates),
				Type = ChangeType
			};
		}

		/// <summary>Serializes Geometry Correctly to Rhino version 7</summary>
		public static string GeneratePayload(GeometryBase geometry = null, Dictionary<string, string> updates = null)
		{
			PayloadPacket payload = new()
			{
				Data = geometry?.ToJSON(new SerializationOptions
				{
					RhinoVersion = 70,
					WriteUserData = false
				}) ?? string.Empty,
				Updates = updates ?? new Dictionary<string, string>()
			};

			return JsonSerializer.Serialize(payload);
		}

		/// <summary>Sets the Geometry of the Change</summary>
		public void SetGeometry(GeometryBase geometry)
		{
			Geometry = geometry;
		}
	}
}
