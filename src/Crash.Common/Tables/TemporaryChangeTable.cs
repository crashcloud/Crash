using System.Collections;

using Crash.Changes.Utils;
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
	public sealed class TemporaryChangeTable : IEnumerable<IChange>
	{
		private readonly ConcurrentDictionary<Guid, IChange> _cache;
		private readonly CrashDoc _crashDoc;

		/// <summary>
		///     Local cache constructor
		/// </summary>
		public TemporaryChangeTable(CrashDoc hostDoc)
		{
			_cache = new ConcurrentDictionary<Guid, IChange>();
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
			if (cache is null)
			{
				return;
			}

			var newChange = cache;
			if (_cache.TryGetValue(cache.Id, out var cachedChange))
			{
				newChange = ChangeUtils.CombineChanges(cachedChange, cache);
				RemoveChange(cache.Id);
			}

			_cache.TryAdd(newChange.Id, newChange);
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
