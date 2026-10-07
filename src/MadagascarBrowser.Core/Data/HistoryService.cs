using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace MadagascarBrowser.Core.Data
{
    public class HistoryService
    {
        private readonly string _path;
        public HistoryService(Database db)
        {
            _path = db.Path;
        }

        public void AddVisit(string url, string title)
        {
            using var conn = new SqliteConnection($"Data Source={_path}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO history (url, title, visited_at) VALUES (, , )";
            cmd.Parameters.AddWithValue("", url);
            cmd.Parameters.AddWithValue("", title);
            cmd.Parameters.AddWithValue("", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public List<Visit> Recent(int limit = 20)
        {
            var list = new List<Visit>();
            using var conn = new SqliteConnection($"Data Source={_path}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT url, title, visited_at FROM history ORDER BY id DESC LIMIT ";
            cmd.Parameters.AddWithValue("", limit);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new Visit(rdr.GetString(0), rdr.GetString(1), DateTime.Parse(rdr.GetString(2))));
            }
            return list;
        }

        public List<Visit> Search(string query, int limit = 10)
        {
            var list = new List<Visit>();
            using var conn = new SqliteConnection($"Data Source={_path}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT url, title, visited_at FROM history WHERE url LIKE  OR title LIKE  ORDER BY id DESC LIMIT ";
            cmd.Parameters.AddWithValue("", "%" + query + "%");
            cmd.Parameters.AddWithValue("", limit);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new Visit(rdr.GetString(0), rdr.GetString(1), DateTime.Parse(rdr.GetString(2))));
            }
            return list;
        }
    }
}
