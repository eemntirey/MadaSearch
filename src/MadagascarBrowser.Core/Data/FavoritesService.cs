using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MadagascarBrowser.Core.Data
{
    public class FavoritesService
    {
        private readonly string _path;
        public FavoritesService(Database db)
        {
            _path = db.Path;
        }

        public void Add(string url, string title)
        {
            using var conn = new SqliteConnection($"Data Source={_path}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT OR REPLACE INTO favorites (url, title, added_at) VALUES (, , )";
            cmd.Parameters.AddWithValue("", url);
            cmd.Parameters.AddWithValue("", title);
            cmd.Parameters.AddWithValue("", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public void Remove(string url)
        {
            using var conn = new SqliteConnection($"Data Source={_path}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM favorites WHERE url = ";
            cmd.Parameters.AddWithValue("", url);
            cmd.ExecuteNonQuery();
        }

        public List<Favorite> GetAll()
        {
            var list = new List<Favorite>();
            using var conn = new SqliteConnection($"Data Source={_path}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT url, title, added_at FROM favorites ORDER BY id DESC";
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new Favorite(rdr.GetString(0), rdr.GetString(1), DateTime.Parse(rdr.GetString(2))));
            }
            return list;
        }

        public void ExportJson(string path)
        {
            var data = GetAll();
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public void ImportJson(string path)
        {
            if (!File.Exists(path))
                return;
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<List<Favorite>>(json);
            if (data == null)
                return;
            using var conn = new SqliteConnection($"Data Source={_path}");
            conn.Open();
            foreach (var f in data)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT OR REPLACE INTO favorites (url, title, added_at) VALUES (, , )";
                cmd.Parameters.AddWithValue("", f.Url);
                cmd.Parameters.AddWithValue("", f.Title);
                cmd.Parameters.AddWithValue("", f.AddedAt.ToString("o"));
                cmd.ExecuteNonQuery();
            }
        }
    }

    public record Favorite(string Url, string Title, DateTime AddedAt);
}
