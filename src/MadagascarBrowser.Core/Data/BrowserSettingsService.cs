using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace MadagascarBrowser.Core.Data
{
    public class BrowserSettingsService
    {
        private readonly string _path;

        public BrowserSettingsService(Database database)
        {
            _path = database.Path;
        }

        public BrowserSettings Load()
        {
            var settings = new BrowserSettings();
            using var connection = new SqliteConnection($"Data Source={_path}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT key, value FROM settings";
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var value = reader.GetString(1);
                switch (reader.GetString(0))
                {
                    case "search_provider":
                        settings.SearchProvider = value;
                        break;
                    case "ad_block_enabled":
                        settings.AdBlockEnabled = bool.TryParse(value, out var adBlockEnabled) && adBlockEnabled;
                        break;
                    case "data_saver_enabled":
                        settings.DataSaverEnabled = bool.TryParse(value, out var dataSaverEnabled) && dataSaverEnabled;
                        break;
                    case "restore_session":
                        settings.RestoreSession = bool.TryParse(value, out var restoreSession) && restoreSession;
                        break;
                    case "allowed_domains":
                        settings.AllowedDomains = ParseDomains(value);
                        break;
                    case "theme":
                        settings.Theme = value;
                        break;
                    case "density":
                        settings.Density = value;
                        break;
                    case "accent":
                        settings.Accent = value;
                        break;
                }
            }

            return settings;
        }

        public void Save(BrowserSettings settings)
        {
            using var connection = new SqliteConnection($"Data Source={_path}");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "INSERT OR REPLACE INTO settings (key, value) VALUES ($key, $value)";

            SaveValue(command, "search_provider", settings.SearchProvider);
            SaveValue(command, "ad_block_enabled", settings.AdBlockEnabled.ToString());
            SaveValue(command, "data_saver_enabled", settings.DataSaverEnabled.ToString());
            SaveValue(command, "restore_session", settings.RestoreSession.ToString());
            SaveValue(command, "allowed_domains", string.Join(",", settings.AllowedDomains ?? new List<string>()));
            SaveValue(command, "theme", settings.Theme);
            SaveValue(command, "density", settings.Density);
            SaveValue(command, "accent", settings.Accent);
            transaction.Commit();
        }

        /// <summary>
        /// Découpe la liste de domaines autorisés stockée en base.
        /// </summary>
        private static List<string> ParseDomains(string value)
        {
            var domains = new List<string>();
            foreach (var part in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var domain = part.Trim();
                if (domain.Length > 0)
                    domains.Add(domain);
            }
            return domains;
        }

        private static void SaveValue(SqliteCommand command, string key, string value)
        {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("$key", key);
            command.Parameters.AddWithValue("$value", value);
            command.ExecuteNonQuery();
        }
    }
}