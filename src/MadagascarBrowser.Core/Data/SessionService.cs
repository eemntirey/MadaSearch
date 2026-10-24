using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MadagascarBrowser.Core.Data
{
    public class SessionService
    {
        private readonly string _path;

        public SessionService(Database database)
        {
            _path = database.Path;
        }

        public void Save(BrowserSession session)
        {
            var urls = session.Urls?.ToList() ?? new List<string>();
            var activeTabIndex = urls.Count == 0
                ? -1
                : Math.Clamp(session.ActiveTabIndex, 0, urls.Count - 1);

            using var connection = SqliteConnectionFactory.Open(_path);

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT OR REPLACE INTO session_state (id, urls_json, active_tab_index) VALUES (1, $urls, $active)";
            command.Parameters.AddWithValue("$urls", JsonSerializer.Serialize(urls));
            command.Parameters.AddWithValue("$active", activeTabIndex);
            command.ExecuteNonQuery();
        }

        public BrowserSession Load()
        {
            using var connection = SqliteConnectionFactory.Open(_path);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT urls_json, active_tab_index FROM session_state WHERE id = 1";
            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return new BrowserSession(Array.Empty<string>(), -1);

            List<string>? urls;
            try
            {
                urls = JsonSerializer.Deserialize<List<string>>(reader.GetString(0));
            }
            catch (JsonException exception)
            {
                System.Diagnostics.Trace.TraceWarning(
                    $"Session illisible (JSON corrompu), démarrage sans restauration : {exception.Message}");
                return new BrowserSession(Array.Empty<string>(), -1);
            }

            if (urls == null || urls.Count == 0)
                return new BrowserSession(Array.Empty<string>(), -1);

            var activeTabIndex = Math.Clamp(reader.GetInt32(1), 0, urls.Count - 1);
            return new BrowserSession(urls, activeTabIndex);
        }
    }
}