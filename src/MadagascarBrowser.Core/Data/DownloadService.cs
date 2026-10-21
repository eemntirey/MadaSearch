using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MadagascarBrowser.Core.Data
{
    public class DownloadService
    {
        private readonly string _path;

        public DownloadService(Database database)
        {
            _path = database.Path;
        }

        public BrowserDownload Start(string url, string destination)
        {
            var startedAt = DateTime.UtcNow;
            using var connection = new SqliteConnection($"Data Source={_path}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO downloads (url,path,bytes_received,total_bytes,state,started_at) VALUES ($url,$path,0,0,$state,$started_at); SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$url", url);
            command.Parameters.AddWithValue("$path", destination);
            command.Parameters.AddWithValue("$state", DownloadState.InProgress.ToString());
            command.Parameters.AddWithValue("$started_at", DateTimeStorage.ToStorage(startedAt));
            var id = (long)command.ExecuteScalar()!;
            return new BrowserDownload(id, url, destination, 0, 0, DownloadState.InProgress, startedAt);
        }

        public void UpdateProgress(long id, long bytesReceived, long totalBytes)
        {
            Update(id, bytesReceived, totalBytes, DownloadState.InProgress);
        }

        public void Complete(long id, long bytesReceived, long totalBytes)
        {
            Update(id, bytesReceived, totalBytes, DownloadState.Completed);
        }

        public void Cancel(long id)
        {
            SetState(id, DownloadState.Cancelled);
        }

        public void Fail(long id)
        {
            SetState(id, DownloadState.Failed);
        }

        public List<BrowserDownload> GetRecent(int limit = 50)
        {
            var downloads = new List<BrowserDownload>();
            using var connection = new SqliteConnection($"Data Source={_path}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id,url,path,bytes_received,total_bytes,state,started_at FROM downloads ORDER BY id DESC LIMIT $limit";
            command.Parameters.AddWithValue("$limit", Math.Max(0, limit));
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                downloads.Add(new BrowserDownload(
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetInt64(3),
                    reader.GetInt64(4),
                    Enum.Parse<DownloadState>(reader.GetString(5)),
                    DateTimeStorage.Parse(reader.GetString(6))));
            }
            return downloads;
        }

        private void Update(long id, long bytesReceived, long totalBytes, DownloadState state)
        {
            using var connection = new SqliteConnection($"Data Source={_path}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE downloads SET bytes_received=$received,total_bytes=$total,state=$state WHERE id=$id";
            command.Parameters.AddWithValue("$received", Math.Max(0, bytesReceived));
            command.Parameters.AddWithValue("$total", Math.Max(0, totalBytes));
            command.Parameters.AddWithValue("$state", state.ToString());
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        private void SetState(long id, DownloadState state)
        {
            using var connection = new SqliteConnection($"Data Source={_path}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE downloads SET state=$state WHERE id=$id";
            command.Parameters.AddWithValue("$state", state.ToString());
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }
    }
}