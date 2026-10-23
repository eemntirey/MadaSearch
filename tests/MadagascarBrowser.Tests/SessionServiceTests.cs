using MadagascarBrowser.Core.Data;
using Microsoft.Data.Sqlite;
using System.IO;

namespace MadagascarBrowser.Tests;

public class SessionServiceTests
{
    [Fact]
    public void SaveAndLoad_PreservesTabOrderAndActiveTab()
    {
        var (directory, databasePath) = CreateDatabasePath();

        try
        {
            var service = new SessionService(new Database(databasePath));
            var expected = new BrowserSession(
                new List<string> { "https://one.example", "https://two.example" }, 1);

            service.Save(expected);

            var actual = service.Load();
            Assert.Equal(expected.Urls, actual.Urls);
            Assert.Equal(expected.ActiveTabIndex, actual.ActiveTabIndex);
        }
        finally
        {
            ClearDatabase(directory, databasePath);
        }
    }

    [Fact]
    public void Save_ClampsActiveIndexAndUsesMinusOneForEmptySession()
    {
        var (directory, databasePath) = CreateDatabasePath();

        try
        {
            var service = new SessionService(new Database(databasePath));
            service.Save(new BrowserSession(new List<string> { "https://one.example" }, 9));
            Assert.Equal(0, service.Load().ActiveTabIndex);

            service.Save(new BrowserSession(new List<string>(), 0));
            Assert.Equal(-1, service.Load().ActiveTabIndex);
        }
        finally
        {
            ClearDatabase(directory, databasePath);
        }
    }

    [Fact]
    public void SavedSnapshot_CanBeLoadedByANewServiceInstanceAfterAbruptStop()
    {
        var (directory, databasePath) = CreateDatabasePath();

        try
        {
            var snapshot = new BrowserSession(
                new List<string> { "https://one.example", "https://two.example", "https://three.example" }, 2);

            // Simulates the last periodic snapshot being committed before process termination.
            new SessionService(new Database(databasePath)).Save(snapshot);

            var recovered = new SessionService(new Database(databasePath)).Load();
            Assert.Equal(snapshot.Urls, recovered.Urls);
            Assert.Equal(2, recovered.ActiveTabIndex);
        }
        finally
        {
            ClearDatabase(directory, databasePath);
        }
    }

    private static (string Directory, string DatabasePath) CreateDatabasePath()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return (directory, Path.Combine(directory, "browser.db"));
    }

    private static void ClearDatabase(string directory, string databasePath)
    {
        using var connection = new SqliteConnection($"Data Source={databasePath}");
        SqliteConnection.ClearPool(connection);
        Directory.Delete(directory, recursive: true);
    }
}