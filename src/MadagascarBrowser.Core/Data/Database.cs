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
CREATE TABLE IF NOT EXISTS favorites (id INTEGER PRIMARY KEY AUTOINCREMENT, url TEXT NOT NULL UNIQUE, title TEXT NOT NULL, added_at TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS settings (key TEXT PRIMARY KEY, value TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS downloads (id INTEGER PRIMARY KEY AUTOINCREMENT, url TEXT NOT NULL, path TEXT NOT NULL, bytes_received INTEGER NOT NULL, total_bytes INTEGER NOT NULL, state TEXT NOT NULL, started_at TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS session_state (id INTEGER PRIMARY KEY CHECK (id = 1), urls_json TEXT NOT NULL, active_tab_index INTEGER NOT NULL);
CREATE TABLE IF NOT EXISTS offline_pages (id TEXT PRIMARY KEY, url TEXT NOT NULL, title TEXT NOT NULL, path TEXT NOT NULL, saved_at TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS data_saver_metrics (id INTEGER PRIMARY KEY CHECK (id = 1), saved_bytes INTEGER NOT NULL, blocked_ads INTEGER NOT NULL, replaced_images INTEGER NOT NULL, blocked_videos INTEGER NOT NULL);
INSERT OR IGNORE INTO data_saver_metrics (id, saved_bytes, blocked_ads, replaced_images, blocked_videos) VALUES (1, 0, 0, 0, 0);";
                cmd.ExecuteNonQuery();
            }
        }
        public string Path => _path;
    }
}
