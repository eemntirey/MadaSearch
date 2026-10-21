using System;
using System.Globalization;
using MadagascarBrowser.Core.Data;

namespace MadagascarBrowser.Tests;

public class DateTimeStorageTests
{
    [Fact]
    public void ToStorage_UsesInvariantRoundTripFormat()
    {
        var value = new DateTime(2026, 3, 15, 8, 30, 0, DateTimeKind.Utc);

        var stored = DateTimeStorage.ToStorage(value);

        Assert.Equal(value.ToString("o", CultureInfo.InvariantCulture), stored);
    }

    [Fact]
    public void ToStorage_ConvertsToUtc()
    {
        var local = new DateTime(2026, 3, 15, 8, 30, 0, DateTimeKind.Local);

        var stored = DateTimeStorage.ToStorage(local);

        Assert.Equal(local.ToUniversalTime(), DateTimeStorage.Parse(stored));
    }

    [Fact]
    public void Parse_RoundTripsToStorage()
    {
        var value = new DateTime(2026, 3, 15, 8, 30, 0, DateTimeKind.Utc);

        var parsed = DateTimeStorage.Parse(DateTimeStorage.ToStorage(value));

        Assert.Equal(value, parsed);
        Assert.Equal(DateTimeKind.Utc, parsed.Kind);
    }

    [Fact]
    public void Parse_IgnoresCurrentCulture()
    {
        // Une date écrite avec la culture fr-FR doit être relue à l'identique.
        var value = new DateTime(2026, 3, 15, 8, 30, 0, DateTimeKind.Utc);

        var parsed = DateTimeStorage.Parse(value.ToString("o", CultureInfo.GetCultureInfo("fr-FR")));

        Assert.Equal(value, parsed);
    }
}
