using System.Text;
using System.Text.Json;

using Crash.Common.App;
using Crash.Common.Document;

namespace Crash.Handlers.Data
{
	public class CrashData : ICrashInstance
	{

		public static string CrashDataDirectory
		{
			get
			{
				var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				return System.IO.Path.Combine(appData, "crash", "desktop");
			}
		}

		public static bool WriteFile(string data, string filename)
		{
			try
			{
				var crashDataDir = CrashData.CrashDataDirectory;
				var path = System.IO.Path.Combine(crashDataDir, filename);

				if (!Directory.Exists(crashDataDir))
				{
					Directory.CreateDirectory(crashDataDir);
				}

				System.IO.File.WriteAllText(path, data);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool TryReadFileData<TData>(string filename, out TData data)
		{
			try
			{
				var crashDataDir = CrashData.CrashDataDirectory;
				var path = System.IO.Path.Combine(crashDataDir, filename);

				if (System.IO.File.Exists(path))
				{
					var json = System.IO.File.ReadAllText(path);
					var options = new JsonDocumentOptions
					{
						AllowTrailingCommas = true,
						CommentHandling = JsonCommentHandling.Skip,
					};

					data = JsonSerializer.Deserialize<TData>(json);
					return data is not null;
				}
			}
			catch { }

			data = default;
			return false;
		}

	}
}
