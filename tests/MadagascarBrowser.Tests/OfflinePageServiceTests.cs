using MadagascarBrowser.Core.Data;
using Microsoft.Data.Sqlite;
using System.IO;

namespace MadagascarBrowser.Tests;

public class OfflinePageServiceTests
{
    [Fact]
    public void Save_ListAndRemove_ManageSnapshotAndMetadata()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var databasePath = Path.Combine(directory, "browser.db");
        var snapshotDirectory = Path.Combine(directory, "offline");
        Directory.CreateDirectory(directory);

        try
        {
            var service = new OfflinePageService(new Database(databasePath), snapshotDirectory);
            var saved = service.Save("https://example.com", "Example", "MIME-Version: 1.0\n\nSnapshot");

            Assert.True(File.Exists(saved.Path));
            Assert.Equal(saved.Id, Assert.Single(service.GetAll()).Id);
            Assert.Equal(saved.Path, service.GetPath(saved.Id));

            service.Remove(saved.Id);

            Assert.Empty(service.GetAll());
            Assert.False(File.Exists(saved.Path));
        }
        finally
        {
            using var connection = new SqliteConnection($"Data Source={databasePath}");
            SqliteConnection.ClearPool(connection);
            Directory.Delete(directory, recursive: true);
        }
    }
}