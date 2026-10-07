using MadagascarBrowser.Core.Data;
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
}
