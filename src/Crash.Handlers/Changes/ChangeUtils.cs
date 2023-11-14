using Crash.Common.Document;
using Crash.Common.Tables;
using Crash.Handlers;

using Rhino.DocObjects;

using ChangeGuid = System.Guid;
using RhinoGuid = System.Guid;


namespace Crash.Utils
{
	/// <summary>Utilities for Change Objects.</summary>
	public static class ChangeUtils
	{
		public static bool IsActiveChange(this RhinoObject rhinoObject, CrashDoc crashDoc)
		{
			return crashDoc.RealisedChangeTable.ContainsRhinoId(rhinoObject.Id);
		}

		/// <summary>Acquires the Rhino Object given the RhinoId from an IRhinoChange</summary>
		public static bool TryGetRhinoObject(this IChange change, CrashDoc crashDoc, out RhinoObject rhinoObject)
		{
			rhinoObject = default;
			if (change is null || crashDoc is null)
			{
				return false;
			}

			if (!crashDoc.RealisedChangeTable.TryGetRhinoId(change, out var rhinoId))
			{
				return false;
			}

			var rhinoDoc = CrashDocRegistry.GetRelatedDocument(crashDoc);
			rhinoObject = rhinoDoc.Objects.FindId(rhinoId);

			return rhinoObject is not null;
		}

		public static bool TryGetChangeId(this RhinoObject rhinoObject, out Guid changeId)
		{
			return rhinoObject.Geometry.UserDictionary.TryGetGuid(RealisedChangeTable.ChangeIdKey, out changeId);
		}

		/// <summary>Adds the ChangeId to the Rhino Object and vice Verse.</summary>
		public static bool SyncHost(this RhinoObject rhinoObject, IChange change, CrashDoc crashDoc)
		{
			if (change is null || rhinoObject is null || crashDoc is null)
			{
				return false;
			}

			rhinoObject.Geometry.UserDictionary.Remove(RealisedChangeTable.ChangeIdKey);
			rhinoObject.Geometry.UserDictionary.Set(RealisedChangeTable.ChangeIdKey, change.Id);
			crashDoc.RealisedChangeTable.Add(change, rhinoObject.Id);

			return true;
		}

		/// <summary>Check for Oversied Payload</summary>
		public static bool IsOversized(this IChange change)
		{
			return change.Payload?.Length > ushort.MaxValue;
		}
	}
}
