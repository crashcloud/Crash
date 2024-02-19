using Crash.Common.Document;

namespace Crash.Common.Tables
{
	public sealed class CacheTable
	{
		private readonly CrashDoc _crashDoc;

		internal CacheTable(CrashDoc hostDoc)
		{
			_crashDoc = hostDoc;
			CachedTables = new Dictionary<string, ICacheTable>();
		}

		private Dictionary<string, ICacheTable> CachedTables { get; }

		public void AddTable(ICacheTable table)
		{
			var key = table.GetType().Name;
			if (!CachedTables.ContainsKey(key))
			{
				CachedTables.Add(key, table);
			}
		}

		public TTable? Get<TTable>() where TTable : class
		{
			var key = typeof(TTable).Name;
			CachedTables.TryGetValue(key, out var table);
			return table as TTable;
		}
	}
}
