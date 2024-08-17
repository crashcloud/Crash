using Crash.Common.Document;

namespace Crash.Common.Tables
{

	/// <summary>
	/// Stores all of the tables related to the Crash Doc
	/// </summary>
	public sealed class CacheTable
	{
		private CrashDoc CrashDoc { get; }

		internal CacheTable(CrashDoc hostDoc)
		{
			CrashDoc = hostDoc;
			CachedTables = new Dictionary<string, ICacheTable>();
		}

		private Dictionary<string, ICacheTable> CachedTables { get; }

		/// <summary>
		/// Adds a table into the cache
		/// </summary>
		/// <param name="table">The table to add</param>
		public void AddTable(ICacheTable table)
		{
			if (table is null) return;

			var key = table.GetType().Name.ToLowerInvariant();
			if (!CachedTables.ContainsKey(key))
			{
				CachedTables.Add(key, table);
			}
		}

		/// <summary>
		/// Returns the table, or null if it doesn't exist
		/// </summary>
		/// <typeparam name="TTable">The type related to the table</typeparam>
		/// <returns>Returns the table if found, null otherwise</returns>
		public TTable? Get<TTable>() where TTable : class
		{
			var key = typeof(TTable).Name.ToLowerInvariant();
			CachedTables.TryGetValue(key, out var table);
			return table as TTable;
		}

		/// <summary>
		/// Attempts to return a table
		/// </summary>
		/// <typeparam name="TTable">The type related to the table</typeparam>
		/// <returns>True if the table exists</returns>
		public bool TryGet<TTable>(out TTable table) where TTable : class
		{
			var key = typeof(TTable).Name.ToLowerInvariant();
			CachedTables.TryGetValue(key, out var cachedTable);
			table = cachedTable as TTable;
			return table is not null;
		}

	}
}
