﻿using System.Text.Json;

namespace Crash.Common.Serialization
{

	/// <summary>Helps Serialize and Deserialize PayloadPackets</summary>
	public static class PayloadSerialization
	{

		/// <summary>Guarantees a PayloadPacket</summary>
		/// <param name="payload"></param>
		/// <returns></returns>
		public static PayloadPacket GetPayloadPacket(string payload)
		{
			try
			{
				// TODO : This needs reworking
				var packet = JsonSerializer.Deserialize<PayloadPacket>(payload);

				if (string.IsNullOrEmpty(packet.Data))
					packet.Data = payload;

				return packet;
			}
			catch (Exception ex)
			{
				return new PayloadPacket() { Data = payload };
			}
		}

		/*
		public static string SerializePayload(string payload)
		{
			return 
		}
		*/

	}
}
