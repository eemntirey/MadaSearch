using MadagascarBrowser.Core.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace MadagascarBrowser.Tests;

public class BrowserSettingsServiceTests
{
    [Fact]
    public void Load_ReturnsDefaultsAndPersistsChanges()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var databasePath = Path.Combine(directory, "browser.db");

        try
        {
            var database = new Database(databasePath);
            var settingsService = new BrowserSettingsService(database);
            var defaults = settingsService.Load();

            Assert.Equal("google", defaults.SearchProvider);
            Assert.True(defaults.AdBlockEnabled);
            Assert.False(defaults.DataSaverEnabled);
            Assert.False(defaults.RestoreSession);

            settingsService.Save(new BrowserSettings
            {
                SearchProvider = "duckduckgo",
                AdBlockEnabled = false,
                DataSaverEnabled = true,
                RestoreSession = true
            });

            var reloaded = new BrowserSettingsService(new Database(databasePath)).Load();

            Assert.Equal("duckduckgo", reloaded.SearchProvider);
            Assert.False(reloaded.AdBlockEnabled);
            Assert.True(reloaded.DataSaverEnabled);
            Assert.True(reloaded.RestoreSession);
            Assert.Empty(reloaded.AllowedDomains);
        }
        finally
        {
            using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                SqliteConnection.ClearPool(connection);
            }
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Load_PersistsAllowedDomains()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var databasePath = Path.Combine(directory, "browser.db");

        try
        {
            var settingsService = new BrowserSettingsService(new Database(databasePath));
            settingsService.Save(new BrowserSettings
            {
                AllowedDomains = new List<string> { "exemple.mg", "mada.mg" }
            });

            var reloaded = new BrowserSettingsService(new Database(databasePath)).Load();

            Assert.Equal(new[] { "exemple.mg", "mada.mg" }, reloaded.AllowedDomains);
        }
        finally
        {
            using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                SqliteConnection.ClearPool(connection);
            }
            Directory.Delete(directory, recursive: true);
        }
    }
}