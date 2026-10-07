using MadagascarBrowser.Core.Data;
using System.IO;

namespace MadagascarBrowser.Tests;

public class FavoritesServiceTests
{
    [Fact]
    public void Add_Remove_GetAll()
    {
        var dbPath = Path.GetTempFileName();
        var db = new Database(dbPath);
        var fav = new FavoritesService(db);
        fav.Add("https://example.com", "Example");
        fav.Add("https://example.com", "Example");
        var all = fav.GetAll();
        Assert.Single(all);
        fav.Remove("https://example.com");
        all = fav.GetAll();
        Assert.Empty(all);
        try { File.Delete(dbPath); } catch { }
    }
}
