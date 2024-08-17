using System.Collections;

using Crash.Common.Document;

namespace Crash.Common.Tables
{
	/// <summary>
	///     This table holds onto temporary changes
	///     Temporary changes are changes that another user has created
	///     But not yet released
	///     These temporary changes are, if necessary displayed
	///     in the pipeline to show pending changes
	/// </summary>
	public sealed class TemporaryChangeTable : ICacheTable, IEnumerable<IChange>
	{
		private readonly ConcurrentDictionary<Guid, IChange> _cache;
		private readonly CrashDoc _crashDoc;
		private readonly ConcurrentDictionary<Guid, IChange> _deleted;

		/// <summary>
		///     Local cache constructor
		/// </summary>
		public TemporaryChangeTable(CrashDoc hostDoc)
		{
			_cache = new ConcurrentDictionary<Guid, IChange>();
			_deleted = new ConcurrentDictionary<Guid, IChange>();
			_crashDoc = hostDoc;
		}

		public IEnumerator<IChange> GetEnumerator()
		{
			return _cache.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _cache.Values.GetEnumerator();
		}

		/// <summary>Clears the Cache</summary>
		internal void Clear()
		{
			_cache?.Clear();
		}

		/// <summary>
		///     Method to get Changes
		/// </summary>
		/// <returns>returns a list of the Changes</returns>
		public IEnumerable<IChange> GetChanges()
		{
			return _cache.Values;
		}


		public IEnumerator<T> GetEnumerator<T>()
		{
			return _cache.Values.Where(x => x is T).Cast<T>().GetEnumerator();
		}

		/// <summary>
		///     Method to update a Change
		/// </summary>
		/// <param name="cache">the Changes</param>
		/// <returns>returns the update task</returns>
		public void UpdateChange(IChange cache)
		{
			_cache.TryRemove(cache.Id, out _);
			_cache.TryAdd(cache.Id, cache);
			_deleted.TryRemove(cache.Id, out _);
		}

		/// <summary>
		///     Remove a Change for the cache
		/// </summary>
		/// <param name="changeId">the Change to remove</param>
		public void RemoveChange(Guid changeId)
		{
			_cache.TryRemove(changeId, out _);
		}

		/// <summary>
		///     Deletes a Change from the Cache temporarily, hiding it
		/// </summary>
		/// <param name="changeId"></param>
		public void DeleteChange(Guid changeId)
		{
			if (!_cache.TryRemove(changeId, out var change))
			{
				return;
			}

			_deleted.TryAdd(changeId, change);
		}

		/// <summary>
		///     Checks to see if a Change is Deleted
		/// </summary>
		public bool IsDeleted(Guid changeId)
		{
			return _deleted.ContainsKey(changeId);
		}

		/// <summary>
		///     Checks that a pairing with this RhinoId exists.
		/// </summary>
		public bool HasPairing(Guid rhinoId)
		{
			return _cache.ContainsKey(rhinoId);
		}

		/// <summary>
		///     Attempts to find the Change associated to this RhinoId
		/// </summary>
		public bool TryGetChange(Guid rhinoId, out IChange change)
		{
			return _cache.TryGetValue(rhinoId, out change);
		}

		/// <summary>
		///     Restores a Deleted Change
		/// </summary>
		public void RestoreChange(Guid changeId)
		{
			if (!_deleted.TryRemove(changeId, out var change))
			{
				return;
			}

			_cache.TryAdd(changeId, change);
		}

		/// <summary>
		///     Remove multiple Changes from the cache
		/// </summary>
		/// <param name="changes">the Changes to remove</param>
		public void RemoveChanges(IEnumerable<IChange> changes)
		{
			if (changes is null)
			{
				return;
			}

			foreach (var change in changes)
			{
				RemoveChange(change.Id);
			}
		}

		/// <summary>Returns the matching change if it is of the correct type</summary>
		/// <param name="id">The Id of the Change</param>
		/// <param name="change">The change out</param>
		/// <typeparam name="T">The type of the Change</typeparam>
		/// <returns>True if the change is in the table and of the T type</returns>
		public bool TryGetChangeOfType<T>(Guid id, out T change) where T : IChange
		{
			change = default;

			if (!_cache.TryGetValue(id, out var cachedChange))
			{
				return false;
			}

			if (cachedChange is not T castChange)
			{
				return false;
			}

			change = castChange;

			return true;
		}
	}
}
