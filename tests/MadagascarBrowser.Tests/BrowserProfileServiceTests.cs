using MadagascarBrowser.Core.Data;
using Microsoft.Data.Sqlite;
using System.IO;

namespace MadagascarBrowser.Tests;

public class BrowserProfileServiceTests
{
    [Fact]
    public void ExportAndImport_RoundTripsLocalProfileData()
    {
        var sourceDirectory = CreateDirectory();
        var targetDirectory = CreateDirectory();
        var exportPath = Path.Combine(sourceDirectory, "profile.json");
        var sourceDatabasePath = Path.Combine(sourceDirectory, "source.db");
        var targetDatabasePath = Path.Combine(targetDirectory, "target.db");

        try
        {
            var sourceDatabase = new Database(sourceDatabasePath);
            new FavoritesService(sourceDatabase).Add("https://example.com", "Example");
            new FavoritesService(sourceDatabase).Add("https://new.example", "New");
            new HistoryService(sourceDatabase).AddVisit("https://example.com/page", "Example page");
            new HistoryService(sourceDatabase).AddVisit("https://example.com/new", "New page");
            var sourceSettings = new BrowserSettingsService(sourceDatabase);
            sourceSettings.Save(new BrowserSettings
            {
                SearchProvider = "duckduckgo",
                AdBlockEnabled = false,
                DataSaverEnabled = true,
                RestoreSession = true
            });
            new BrowserProfileService(sourceDatabase, sourceSettings).Export(exportPath);

            var targetDatabase = new Database(targetDatabasePath);
            new FavoritesService(targetDatabase).Add("https://old.example", "Old");
            var targetSettings = new BrowserSettingsService(targetDatabase);
            new BrowserProfileService(targetDatabase, targetSettings).Import(exportPath);

            Assert.Equal("https://new.example", new FavoritesService(targetDatabase).GetAll()[0].Url);
            Assert.Equal("https://example.com/new", new HistoryService(targetDatabase).Recent()[0].Url);
            Assert.Equal("duckduckgo", targetSettings.Load().SearchProvider);
            Assert.False(targetSettings.Load().AdBlockEnabled);
            Assert.True(targetSettings.Load().DataSaverEnabled);
            Assert.True(targetSettings.Load().RestoreSession);
        }
        finally
        {
            ClearDatabase(sourceDirectory, sourceDatabasePath);
            ClearDatabase(targetDirectory, targetDatabasePath);
        }
    }

    [Fact]
    public void Import_RejectsUnsupportedVersionWithoutChangingProfile()
    {
        var directory = CreateDirectory();
        var databasePath = Path.Combine(directory, "browser.db");
        var profilePath = Path.Combine(directory, "invalid.json");

        try
        {
            var database = new Database(databasePath);
            var favorites = new FavoritesService(database);
            favorites.Add("https://existing.example", "Existing");
            var settings = new BrowserSettingsService(database);
            File.WriteAllText(profilePath, "{\"Version\":99}");
            var service = new BrowserProfileService(database, settings);

            Assert.Throws<InvalidDataException>(() => service.Import(profilePath));
            Assert.Equal("https://existing.example", Assert.Single(favorites.GetAll()).Url);
        }
        finally
        {
            ClearDatabase(directory, databasePath);
        }
    }

    private static string CreateDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void ClearDatabase(string directory, string databasePath)
    {
        using var connection = new SqliteConnection($"Data Source={databasePath}");
        SqliteConnection.ClearPool(connection);
        Directory.Delete(directory, recursive: true);
    }
}