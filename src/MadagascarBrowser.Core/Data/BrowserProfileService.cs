using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MadagascarBrowser.Core.Data
{
    public class BrowserProfileService
    {
        private readonly Database _database;
        private readonly BrowserSettingsService _settingsService;

        public BrowserProfileService(Database database, BrowserSettingsService settingsService)
        {
            _database = database;
            _settingsService = settingsService;
        }

        public void Export(string path)
        {
            var profile = new BrowserProfileDocument
            {
                Version = 1,
                Settings = _settingsService.Load(),
                Favorites = new FavoritesService(_database).GetAll(),
                History = new HistoryService(_database).Recent(int.MaxValue)
            };
            File.WriteAllText(path, JsonSerializer.Serialize(profile, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }

        public void Import(string path)
        {
            BrowserProfileDocument? profile;
            try
            {
                profile = JsonSerializer.Deserialize<BrowserProfileDocument>(File.ReadAllText(path));
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The profile file is not valid JSON.", exception);
            }

            Validate(profile);

            using var connection = new SqliteConnection($"Data Source={_database.Path}");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM history; DELETE FROM favorites; DELETE FROM settings;";
            command.ExecuteNonQuery();

            foreach (var favorite in profile!.Favorites!.AsEnumerable().Reverse())
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO favorites (url,title,added_at) VALUES ($url,$title,$added_at)";
                command.Parameters.AddWithValue("$url", favorite.Url);
                command.Parameters.AddWithValue("$title", favorite.Title);
                command.Parameters.AddWithValue("$added_at", favorite.AddedAt.ToUniversalTime().ToString("o"));
                command.ExecuteNonQuery();
            }

            foreach (var visit in profile.History!.AsEnumerable().Reverse())
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO history (url,title,visited_at) VALUES ($url,$title,$visited_at)";
                command.Parameters.AddWithValue("$url", visit.Url);
                command.Parameters.AddWithValue("$title", visit.Title);
                command.Parameters.AddWithValue("$visited_at", visit.VisitedAt.ToUniversalTime().ToString("o"));
                command.ExecuteNonQuery();
            }

            SaveSetting(command, "search_provider", profile.Settings!.SearchProvider);
            SaveSetting(command, "ad_block_enabled", profile.Settings.AdBlockEnabled.ToString());
            SaveSetting(command, "data_saver_enabled", profile.Settings.DataSaverEnabled.ToString());
            SaveSetting(command, "restore_session", profile.Settings.RestoreSession.ToString());
            SaveSetting(command, "allowed_domains", string.Join(",", profile.Settings.AllowedDomains ?? new List<string>()));
            transaction.Commit();
        }

        private static void Validate(BrowserProfileDocument? profile)
        {
            if (profile?.Version != 1 || profile.Settings == null ||
                profile.Favorites == null || profile.History == null)
                throw new InvalidDataException("The profile document is incomplete or unsupported.");

            if (profile.Settings.SearchProvider is not ("google" or "duckduckgo" or "bing"))
                throw new InvalidDataException("The profile contains an unsupported search provider.");

            if (profile.Favorites.Any(favorite => favorite == null ||
                    !IsWebUrl(favorite.Url) || favorite.Title == null) ||
                profile.History.Any(visit => visit == null || !IsWebUrl(visit.Url) || visit.Title == null))
                throw new InvalidDataException("The profile contains an invalid history or favorite entry.");
        }

        private static bool IsWebUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private static void SaveSetting(SqliteCommand command, string key, string value)
        {
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO settings (key,value) VALUES ($key,$value)";
            command.Parameters.AddWithValue("$key", key);
            command.Parameters.AddWithValue("$value", value);
            command.ExecuteNonQuery();
        }
    }

    internal sealed class BrowserProfileDocument
    {
        public int Version { get; set; }
        public BrowserSettings? Settings { get; set; }
        public List<Favorite>? Favorites { get; set; }
        public List<Visit>? History { get; set; }
    }
}