using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MadagascarBrowser.Core.Data
{
    public class OfflinePageService
    {
        private readonly string _databasePath;
        private readonly string _storageDirectory;

        public OfflinePageService(Database database, string storageDirectory)
        {
            _databasePath = database.Path;
            _storageDirectory = Path.GetFullPath(storageDirectory);
        }

        public OfflinePage Save(string url, string title, string mhtml)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException("Only HTTP and HTTPS pages can be saved.", nameof(url));
            ArgumentNullException.ThrowIfNull(mhtml);

            Directory.CreateDirectory(_storageDirectory);
            var id = Guid.NewGuid().ToString("N");
            var path = Path.Combine(_storageDirectory, id + ".mhtml");
            var temporaryPath = path + ".tmp";
            var savedAt = DateTime.UtcNow;

            File.WriteAllText(temporaryPath, mhtml, new UTF8Encoding(false));
            File.Move(temporaryPath, path);
            try
            {
                using var connection = SqliteConnectionFactory.Open(_databasePath);

                using var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO offline_pages (id,url,title,path,saved_at) VALUES ($id,$url,$title,$path,$saved_at)";
                command.Parameters.AddWithValue("$id", id);
                command.Parameters.AddWithValue("$url", uri.AbsoluteUri);
                command.Parameters.AddWithValue("$title", title ?? string.Empty);
                command.Parameters.AddWithValue("$path", path);
                command.Parameters.AddWithValue("$saved_at", DateTimeStorage.ToStorage(savedAt));
                command.ExecuteNonQuery();
            }
            catch
            {
                File.Delete(path);
                throw;
            }

            return new OfflinePage(id, uri.AbsoluteUri, title ?? string.Empty, path, savedAt);
        }

        public List<OfflinePage> GetAll()
        {
            var pages = new List<OfflinePage>();
            using var connection = SqliteConnectionFactory.Open(_databasePath);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id,url,title,path,saved_at FROM offline_pages ORDER BY saved_at DESC";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                pages.Add(new OfflinePage(
                    reader.GetString(0), reader.GetString(1), reader.GetString(2),
                    reader.GetString(3),
                    DateTimeStorage.Parse(reader.GetString(4))));
            }
            return pages;
        }

        public string? GetPath(string id)
        {
            using var connection = SqliteConnectionFactory.Open(_databasePath);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT path FROM offline_pages WHERE id=$id";
            command.Parameters.AddWithValue("$id", id);
            var path = command.ExecuteScalar() as string;
            if (path == null)
                return null;

            var fullPath = Path.GetFullPath(path);
            var root = _storageDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? fullPath : null;
        }

        public void Remove(string id)
        {
            var path = GetPath(id);
            using (var connection = SqliteConnectionFactory.Open(_databasePath))
            {

                using var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM offline_pages WHERE id=$id";
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }

            if (path != null && File.Exists(path))
                File.Delete(path);
        }
    }
}