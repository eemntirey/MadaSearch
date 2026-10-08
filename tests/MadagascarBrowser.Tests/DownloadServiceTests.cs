using MadagascarBrowser.Core.Data;
using Microsoft.Data.Sqlite;
using System.IO;

namespace MadagascarBrowser.Tests;

public class DownloadServiceTests
{
    [Fact]
    public void DownloadLifecycle_PersistsProgressAndCompletion()
    {
        var (directory, databasePath) = CreateDatabasePath();

        try
        {
            var service = new DownloadService(new Database(databasePath));
            var started = service.Start("https://example.com/file.zip", "C:\\Downloads\\file.zip");
            service.UpdateProgress(started.Id, 50, 100);
            service.Complete(started.Id, 100, 100);

            var completed = Assert.Single(service.GetRecent());
            Assert.Equal(DownloadState.Completed, completed.State);
            Assert.Equal(100, completed.BytesReceived);
            Assert.Equal(100, completed.TotalBytes);
        }
        finally
        {
            ClearDatabase(directory, databasePath);
        }
    }

    [Fact]
    public void GetRecent_ReturnsNewestFirstAndCancelledState()
    {
        var (directory, databasePath) = CreateDatabasePath();

        try
        {
            var service = new DownloadService(new Database(databasePath));
            service.Start("https://example.com/old", "C:\\Downloads\\old");
            var newest = service.Start("https://example.com/new", "C:\\Downloads\\new");
            service.Cancel(newest.Id);

            var downloads = service.GetRecent(1);

            Assert.Single(downloads);
            Assert.Equal("https://example.com/new", downloads[0].Url);
            Assert.Equal(DownloadState.Cancelled, downloads[0].State);
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