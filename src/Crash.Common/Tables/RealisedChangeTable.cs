using BidirectionalMap;

using Crash.Common.Document;

namespace Crash.Common.Tables
{
	// TODO : What does this thing do?


	/// <summary>
	///     This table holds onto
	///     This table holds onto temporary changes
	///     Temporary changes are changes that are baked into the Rhino Document
	///     When they're baked we don't also keep a copy of the Change that represents it
	///     It is no longer needed as the fully realised object is the best source of truth
	///     ...
	///     This table exists so that realised objects in the Rhino Document
	///     can be paired back to the changes and correctly updated
	///     ...
	///     When a realised ...
	///     TODO : How do we solve this for Layers etc?
	/// </summary>
	public sealed class RealisedChangeTable
	{
		/// <summary>The stored Id Key</summary>
		public const string ChangeIdKey = "ChangeID";

		/// <summary>The current Crash Document</summary>
		private readonly CrashDoc _crashDoc;

		private readonly BiMap<RhinoGuid, ChangeGuid> _deletedRhinoChangeMap;
		private readonly BiMap<RhinoGuid, ChangeGuid> _rhinoChangeMap;
		private readonly HashSet<ChangeGuid> _selected;

		/// <summary>
		///     Creates a new Realised Change Table
		/// </summary>
		public RealisedChangeTable(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
			_deletedRhinoChangeMap = new BiMap<RhinoGuid, ChangeGuid>();
			_rhinoChangeMap = new BiMap<RhinoGuid, ChangeGuid>();
			_selected = new HashSet<ChangeGuid>();
		}

		/// <summary>Returns all of the Rhino IDs</summary>
		public IEnumerable<Guid> GetRhinoIds()
		{
			return _rhinoChangeMap.Forward.Keys.Select(rg => rg.RhinoId);
		}

		/// <summary>Returns all of the Change IDs</summary>
		public IEnumerable<Guid> GetChangeIds()
		{
			return _rhinoChangeMap.Reverse.Keys.Select(cg => cg.ChangeId);
		}

		/// <summary>Adds a synced pair of Change and Rhino Id</summary>
		/// <param name="change">The Change to Add</param>
		/// <param name="rhinoId">The Rhino Id to Add</param>
		public void AddPair(IChange change, Guid rhinoId)
		{
			AddPair(change.Id, rhinoId);
		}

		/// <summary>Adds a synced pair of Change (Id) and Rhino Id</summary>
		/// <param name="changeId">The Change Id to Add</param>
		/// <param name="rhinoId">The Rhino Id to Add</param>
		public void AddPair(Guid changeId, Guid rhinoId)
		{
			if (ContainsRhinoId(rhinoId) || ContainsChangeId(changeId))
			{
				return;
			}

			_rhinoChangeMap.Add(new RhinoGuid(rhinoId), new ChangeGuid(changeId));
		}

		/// <summary>Checks if a Rhino Id Exists</summary>
		/// <param name="rhinoId">The RhinoId to find</param>
		/// <returns>True if the Rhino Id exists in the map</returns>
		public bool ContainsRhinoId(Guid rhinoId)
		{
			return _rhinoChangeMap.Forward.ContainsKey(new RhinoGuid(rhinoId));
		}

		/// <summary>Checks if a Change Id Exists</summary>
		/// <param name="changeId">The ChangeId to find</param>
		/// <returns>True if the Change Id exists in the map</returns>
		public bool ContainsChangeId(Guid changeId)
		{
			return _rhinoChangeMap.Reverse.ContainsKey(new ChangeGuid(changeId));
		}

		/// <summary>Checks if a Change exists</summary>
		/// <param name="change">The change to find</param>
		/// <returns>true if the Change (Id) exists in the map</returns>
		public bool ContainsChange(IChange change)
		{
			return ContainsChangeId(change.Id);
		}

		/// <summary>
		///     Checks to see if a Change is marked as Deleted
		/// </summary>
		/// <returns>True if Marked as Deleted</returns>
		public bool IsDeleted(Guid changeId)
		{
			return _deletedRhinoChangeMap.Reverse.ContainsKey(new ChangeGuid(changeId));
		}

		/// <summary>
		///     Removes the change from Realised, and stashes it in deleted.
		///     It also removes it from selected
		/// </summary>
		public void DeleteChange(Guid changeId)
		{
			if (!TryGetRhinoId(changeId, out var rhinoId))
			{
				return;
			}

			var rhinoGuid = new RhinoGuid(rhinoId);
			var changeGuid = new ChangeGuid(changeId);
			_rhinoChangeMap.Remove(rhinoGuid);
			_deletedRhinoChangeMap.Add(rhinoGuid, changeGuid);
			_selected.Remove(changeGuid);
		}

		/// <summary>Removes a Pair via the Change Id from the table</summary>
		public void PurgeChange(Guid changeId)
		{
			var changeGuid = new ChangeGuid(changeId);
			_selected.Remove(changeGuid);

			if (!_rhinoChangeMap.Reverse.ContainsKey(changeGuid))
			{
				return;
			}

			var rhinoId = _rhinoChangeMap.Reverse[changeGuid];

			_deletedRhinoChangeMap.Remove(rhinoId);
			_rhinoChangeMap.Remove(rhinoId);
		}

		/// <summary>
		///     Restores a Change. Removes from deleted and moves to added
		/// </summary>
		public void RestoreChange(Guid changeId)
		{
			var changeGuid = new ChangeGuid(changeId);
			if (!_deletedRhinoChangeMap.Reverse.ContainsKey(changeGuid))
			{
				return;
			}

			var rhinoGuid = _deletedRhinoChangeMap.Reverse[changeGuid];
			_deletedRhinoChangeMap.Remove(rhinoGuid);
			_rhinoChangeMap.Add(rhinoGuid, changeGuid);
		}

		private record struct RhinoGuid(Guid RhinoId);

		private record struct ChangeGuid(Guid ChangeId);

		#region Get Ids

		/// <summary>Returns a Rhino Id</summary>
		/// <param name="rhinoId">The paired Rhino Id</param>
		/// <param name="changeId">The found Change Id</param>
		/// <returns>True if the pair exists</returns>
		public bool TryGetRhinoId(Guid changeId, out Guid rhinoId)
		{
			rhinoId = Guid.Empty;
			var changeGuid = new ChangeGuid(changeId);
			if (ContainsChangeId(changeId))
			{
				rhinoId = _rhinoChangeMap.Reverse[changeGuid].RhinoId;
				return true;
			}

			if (IsDeleted(changeId))
			{
				rhinoId = _deletedRhinoChangeMap.Reverse[changeGuid].RhinoId;
				return true;
			}

			return false;
		}

		/// <summary>Returns a Rhino Id</summary>
		/// <param name="change">The paired Change</param>
		/// <param name="rhinoId">The found Rhino Id</param>
		/// <returns>True if the pair exists</returns>
		public bool TryGetRhinoId(IChange change, out Guid rhinoId)
		{
			return TryGetRhinoId(change.Id, out rhinoId);
		}

		/// <summary>Returns a Change Id</summary>
		/// <param name="rhinoId">The paired Rhino Id</param>
		/// <param name="changeId">The found Change Id</param>
		/// <returns>True if the pair exists</returns>
		public bool TryGetChangeId(Guid rhinoId, out Guid changeId)
		{
			changeId = default;
			if (!ContainsRhinoId(rhinoId))
			{
				return false;
			}

			changeId = _rhinoChangeMap.Forward[new RhinoGuid(rhinoId)].ChangeId;
			return true;
		}

		#endregion

		#region Selected

		/// <summary>Adds an Id to the Selection</summary>
		/// <param name="changeId"></param>
		public void AddSelected(Guid changeId)
		{
			_selected.Add(new ChangeGuid(changeId));
		}

		/// <summary>Removes an id from the Selection</summary>
		/// <param name="changeId"></param>
		public void RemoveSelected(Guid changeId)
		{
			_selected.Remove(new ChangeGuid(changeId));
		}

		/// <summary>Clears all Selected Ids</summary>
		/// <exception cref="NotImplementedException"></exception>
		public void ClearSelected()
		{
			_selected.Clear();
		}

		/// <summary>Returns all of the Currently Selected Changes</summary>
		public IEnumerable<Guid> GetSelected()
		{
			return _selected.Select(s => s.ChangeId);
		}

		/// <summary>Checks to see if a Change is Selected</summary>
		public bool IsSelected(Guid changeId)
		{
			return _selected.Contains(new ChangeGuid(changeId));
		}

		#endregion
	}
}
