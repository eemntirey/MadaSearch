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
			using var connection = new SqliteConnection($"Data Source={_path}");
			connection.Open();
			using var command = connection.CreateCommand();
			command.CommandText = "INSERT INTO history (url,title,visited_at) VALUES ($url,$title,$visited_at)";
			command.Parameters.AddWithValue("$url", url);
			command.Parameters.AddWithValue("$title", title);
			command.Parameters.AddWithValue("$visited_at", DateTimeStorage.ToStorage(DateTime.UtcNow));
			command.ExecuteNonQuery();
		}

		public List<Visit> Recent(int limit = 20)
		{
			var visits = new List<Visit>();
			using var connection = new SqliteConnection($"Data Source={_path}");
			connection.Open();
			using var command = connection.CreateCommand();
			command.CommandText = "SELECT url,title,visited_at FROM history ORDER BY id DESC LIMIT $limit";
			command.Parameters.AddWithValue("$limit", limit);
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				visits.Add(new Visit(reader.GetString(0), reader.GetString(1), DateTimeStorage.Parse(reader.GetString(2))));
			}
			return visits;
		}

		public List<Visit> Search(string query, int limit = 10)
		{
			var visits = new List<Visit>();
			using var connection = new SqliteConnection($"Data Source={_path}");
			connection.Open();
			using var command = connection.CreateCommand();
			command.CommandText = "SELECT url,title,visited_at FROM history WHERE url LIKE $q OR title LIKE $q ORDER BY id DESC LIMIT $limit";
			command.Parameters.AddWithValue("$q", "%" + query + "%");
			command.Parameters.AddWithValue("$limit", limit);
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				visits.Add(new Visit(reader.GetString(0), reader.GetString(1), DateTimeStorage.Parse(reader.GetString(2))));
			}
			return visits;
		}

		public void Remove(string url)
		{
			using var connection = new SqliteConnection($"Data Source={_path}");
			connection.Open();
			using var command = connection.CreateCommand();
			command.CommandText = "DELETE FROM history WHERE url = $url";
			command.Parameters.AddWithValue("$url", url);
			command.ExecuteNonQuery();
		}

		public void Clear()
		{
			using var connection = new SqliteConnection($"Data Source={_path}");
			connection.Open();
			using var command = connection.CreateCommand();
			command.CommandText = "DELETE FROM history";
			command.ExecuteNonQuery();
		}
	}
}