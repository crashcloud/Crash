using Crash.Common.Document;

namespace Crash.Common.Tables
{
	public sealed class CacheTable
	{
		private readonly CrashDoc _crashDoc;

		internal CacheTable(CrashDoc hostDoc)
		{
			_crashDoc = hostDoc;
			CachedTables = new Dictionary<string, ICrashTable>();
		}

		private Dictionary<string, ICrashTable> CachedTables { get; }

		public void AddTable(ICrashTable table)
		{
			var key = table.GetType().Name;
			if (!CachedTables.ContainsKey(key))
			{
				CachedTables.Add(key, table);
			}
		}

		public TTable? Get<TTable>() where TTable : class
		{
			var name = CachedTables.GetType().Name;
			CachedTables.TryGetValue(name, out var table);
			return table as TTable;
		}
	}
}
