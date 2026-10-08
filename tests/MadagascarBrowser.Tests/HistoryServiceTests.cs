using MadagascarBrowser.Core.Data;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace MadagascarBrowser.Tests;

public class HistoryServiceTests
{
    [Fact]
    public void AddVisit_ThenRecent_ReturnsVisit()
    {
        var dbPath = Path.GetTempFileName();
        var db = new Database(dbPath);
        var hist = new HistoryService(db);
        hist.AddVisit("https://example.com", "Example");
        var recent = hist.Recent(5);
        Assert.Single(recent);
        Assert.Equal("https://example.com", recent[0].Url);
        Assert.Equal("Example", recent[0].Title);
        try { File.Delete(dbPath); } catch { }
    }

    [Fact]
    public void Remove_DeletesOnlyVisitsForTheGivenUrl()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
        var history = new HistoryService(new Database(dbPath));
        history.AddVisit("https://one.example", "One");
        history.AddVisit("https://two.example", "Two");

        history.Remove("https://one.example");

        var recent = history.Recent(5);
        Assert.Single(recent);
        Assert.Equal("https://two.example", recent[0].Url);
        ClearDatabase(dbPath);
    }

    [Fact]
    public void Clear_RemovesAllVisits()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
        var history = new HistoryService(new Database(dbPath));
        history.AddVisit("https://one.example", "One");
        history.AddVisit("https://two.example", "Two");

        history.Clear();

        Assert.Empty(history.Recent(5));
        ClearDatabase(dbPath);
    }

    private static void ClearDatabase(string path)
    {
        using var connection = new SqliteConnection($"Data Source={path}");
        SqliteConnection.ClearPool(connection);
        File.Delete(path);
    }
}
