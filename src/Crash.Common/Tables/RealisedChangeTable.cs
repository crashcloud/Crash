using BidirectionalMap;

using Crash.Common.Document;

using RhinoGuid = System.Guid;
using ChangeGuid = System.Guid;

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

		private readonly BiMap<RhinoGuid, ChangeGuid> _rhinoChangeMap;

		private readonly HashSet<ChangeGuid> Selected;

		public RealisedChangeTable(CrashDoc crashDoc)
		{
			_crashDoc = crashDoc;
			_rhinoChangeMap = new BiMap<RhinoGuid, ChangeGuid>();
			Selected = new HashSet<Guid>();
		}

		/// <summary>Adds a synced pair of Change and Rhino Id</summary>
		/// <param name="change">The Change to Add</param>
		/// <param name="rhinoId">The Rhino Id to Add</param>
		public void AddPair(IChange change, RhinoGuid rhinoId)
		{
			AddPair(change.Id, rhinoId);
		}

		/// <summary>Adds a synced pair of Change (Id) and Rhino Id</summary>
		/// <param name="changeId">The Change Id to Add</param>
		/// <param name="rhinoId">The Rhino Id to Add</param>
		public void AddPair(ChangeGuid changeId, RhinoGuid rhinoId)
		{
			if (ContainsRhinoId(rhinoId) || ContainsChangeId(changeId))
			{
				return;
			}

			_rhinoChangeMap.Add(rhinoId, changeId);
		}

		/// <summary>Checks if a Rhino Id Exists</summary>
		/// <param name="rhinoId">The RhinoId to find</param>
		/// <returns>True if the Rhino Id exists in the map</returns>
		public bool ContainsRhinoId(RhinoGuid rhinoId)
		{
			return _rhinoChangeMap.Forward.ContainsKey(rhinoId);
		}

		/// <summary>Checks if a Change Id Exists</summary>
		/// <param name="changeId">The ChangeId to find</param>
		/// <returns>True if the Change Id exists in the map</returns>
		public bool ContainsChangeId(ChangeGuid changeId)
		{
			return _rhinoChangeMap.Reverse.ContainsKey(changeId);
		}

		/// <summary>Checks if a Change exists</summary>
		/// <param name="change">The change to find</param>
		/// <returns>true if the Change (Id) exists in the map</returns>
		public bool ContainsChange(IChange change)
		{
			return ContainsChangeId(change.Id);
		}

		/// <summary>Returns a Rhino Id</summary>
		/// <param name="rhinoId">The paired Rhino Id</param>
		/// <param name="changeId">The found Change Id</param>
		/// <returns>True if the pair exists</returns>
		public bool TryGetRhinoId(ChangeGuid changeId, out RhinoGuid rhinoId)
		{
			rhinoId = Guid.Empty;
			if (!ContainsChangeId(changeId))
			{
				return false;
			}

			rhinoId = _rhinoChangeMap.Reverse[changeId];
			return true;
		}

		/// <summary>Returns a Rhino Id</summary>
		/// <param name="change">The paired Change</param>
		/// <param name="rhinoId">The found Rhino Id</param>
		/// <returns>True if the pair exists</returns>
		public bool TryGetRhinoId(IChange change, out RhinoGuid rhinoId)
		{
			return TryGetRhinoId(change.Id, out rhinoId);
		}

		/// <summary>Returns a Change Id</summary>
		/// <param name="rhinoId">The paired Rhino Id</param>
		/// <param name="changeId">The found Change Id</param>
		/// <returns>True if the pair exists</returns>
		public bool TryGetChangeId(RhinoGuid rhinoId, out ChangeGuid changeId)
		{
			changeId = default;
			if (!ContainsRhinoId(rhinoId))
			{
				return false;
			}

			changeId = _rhinoChangeMap.Forward[rhinoId];
			return true;
		}

		/// <summary>Adds an Id to the Selection</summary>
		/// <param name="changeId"></param>
		public void AddSelected(ChangeGuid changeId)
		{
			Selected.Add(changeId);
		}

		/// <summary>Removes an id from the Selection</summary>
		/// <param name="changeId"></param>
		public void RemoveSelected(ChangeGuid changeId)
		{
			Selected.Remove(changeId);
		}

		/// <summary>Clears all Selected Ids</summary>
		/// <exception cref="NotImplementedException"></exception>
		public void ClearSelected()
		{
			Selected.Clear();
		}

		/// <summary>Returns all of the Currently Selected Changes</summary>
		public IEnumerable<ChangeGuid> GetSelected()
		{
			return Selected;
		}
	}
}
