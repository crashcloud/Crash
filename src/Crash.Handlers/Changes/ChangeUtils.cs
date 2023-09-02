using Rhino;
using Rhino.DocObjects;

namespace Crash.Utils
{
	/// <summary>Utilities for Change Objects.</summary>
	public static class ChangeUtils
	{
		private static readonly string ChangeIdKey = "ChangeID";

		// TODO : Not multi-doc compatible!!!
		private static readonly Dictionary<Guid, RhinoObject> RhinoChangeKeys;
		private static readonly HashSet<Guid> SelectedObjects;

		static ChangeUtils()
		{
			RhinoChangeKeys = new Dictionary<Guid, RhinoObject>();
			SelectedObjects = new HashSet<Guid>();
			RhinoDoc.SelectObjects += (sender, args) =>
			                          {
				                          foreach (var obj in args.RhinoObjects)
				                          {
					                          if (!TryGetChangeId(obj, out var ChangeId))
					                          {
						                          continue;
					                          }

					                          SelectedObjects.Add(ChangeId);
				                          }
			                          };
			RhinoDoc.DeselectObjects += (sender, args) =>
			                            {
				                            foreach (var obj in args.RhinoObjects)
				                            {
					                            if (!TryGetChangeId(obj, out var ChangeId))
					                            {
						                            continue;
					                            }

					                            SelectedObjects.Remove(ChangeId);
				                            }
			                            };
		}

		internal static void ClearSelected()
		{
			SelectedObjects.Clear();
		}

		internal static HashSet<Guid> GetSelected()
		{
			return SelectedObjects;
		}

		/// <summary>Acquires the ChangeId from the Rhino Object</summary>
		public static bool TryGetChangeId(this RhinoObject rObj, out Guid id)
		{
			id = Guid.Empty;
			if (rObj is null)
			{
				return false;
			}

			return rObj.Geometry.UserDictionary.TryGetGuid(ChangeIdKey, out id);
		}

		/// <summary>Acquires the Rhino Object given the RhinoId from an IRhinoChange</summary>
		public static bool TryGetRhinoObject(this IChange change, out RhinoObject rhinoObject)
		{
			rhinoObject = default;
			if (change is null)
			{
				return false;
			}

			return RhinoChangeKeys.TryGetValue(change.Id, out rhinoObject);
		}

		/// <summary>Adds the ChangeId to the Rhino Object and vice Verse.</summary>
		public static void SyncHost(this RhinoObject rObj, IChange change)
		{
			if (change is null || rObj is null)
			{
				return;
			}

			if (rObj.Geometry.UserDictionary.TryGetGuid(ChangeIdKey, out var changeId))
			{
				rObj.Geometry.UserDictionary.Remove(ChangeIdKey);
				RhinoChangeKeys.Remove(changeId);
			}

			rObj.Geometry.UserDictionary.Set(ChangeIdKey, change.Id);

			RhinoChangeKeys.Remove(change.Id);
			RhinoChangeKeys.Add(change.Id, rObj);
		}

		/// <summary>Check for Oversied Payload</summary>
		public static bool IsOversized(this IChange change)
		{
			return change.Payload?.Length > ushort.MaxValue;
		}
	}
}
