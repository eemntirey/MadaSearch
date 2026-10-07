using Microsoft.Data.Sqlite;
using System.IO;

namespace MadagascarBrowser.Core.Data
{
    public class Database
    {
        private readonly string _path;
        public Database(string path)
        {
            _path = path;
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (var conn = new SqliteConnection($"Data Source={_path}"))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS history (id INTEGER PRIMARY KEY AUTOINCREMENT, url TEXT NOT NULL, title TEXT NOT NULL, visited_at TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS favorites (id INTEGER PRIMARY KEY AUTOINCREMENT, url TEXT NOT NULL UNIQUE, title TEXT NOT NULL, added_at TEXT NOT NULL);";
                cmd.ExecuteNonQuery();
            }
        }
        public string Path => _path;
    }
}
