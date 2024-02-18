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

		public void AddTable(ICrashTable cachedTables)
		{
			CachedTables.Add(cachedTables.GetType().Name, cachedTables);
		}

		public TTable Get<TTable>() where TTable : class
		{
			var name = CachedTables.GetType().Name;
			CachedTables.TryGetValue(name, out var table);
			return table as TTable
		}
	}
}
