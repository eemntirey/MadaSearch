using MadagascarBrowser.Core.Data;

namespace MadagascarBrowser.Tests;

public class DownloadProgressWriterTests
{
    private static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ShouldPersist_FirstCall_IsPersisted()
    {
        var writer = new DownloadProgressWriter();

        Assert.True(writer.ShouldPersist(0, 1000, Start));
    }

    [Fact]
    public void ShouldPersist_WithinIntervalAndBelowStep_IsSkipped()
    {
        var writer = new DownloadProgressWriter();
        writer.ShouldPersist(0, 1000, Start);

        Assert.False(writer.ShouldPersist(10, 1000, Start.AddMilliseconds(100)));
    }

    [Fact]
    public void ShouldPersist_AfterInterval_IsPersisted()
    {
        var writer = new DownloadProgressWriter();
        writer.ShouldPersist(0, 1000, Start);

        Assert.True(writer.ShouldPersist(10, 1000, Start.AddMilliseconds(500)));
    }

    [Fact]
    public void ShouldPersist_PercentStepReached_IsPersisted()
    {
        var writer = new DownloadProgressWriter();
        writer.ShouldPersist(0, 1000, Start);

        // 5 % d'écart : persisté même avant la fin de l'intervalle.
        Assert.True(writer.ShouldPersist(50, 1000, Start.AddMilliseconds(10)));
    }

    [Fact]
    public void ShouldPersist_UnknownTotal_UsesIntervalOnly()
    {
        var writer = new DownloadProgressWriter();
        writer.ShouldPersist(0, 0, Start);

        Assert.False(writer.ShouldPersist(10, 0, Start.AddMilliseconds(100)));
        Assert.True(writer.ShouldPersist(20, 0, Start.AddMilliseconds(500)));
    }

    [Fact]
    public void ShouldPersist_CustomIntervalAndStep_AreHonored()
    {
        var writer = new DownloadProgressWriter(TimeSpan.FromSeconds(2), 50);
        writer.ShouldPersist(0, 1000, Start);

        Assert.False(writer.ShouldPersist(400, 1000, Start.AddMilliseconds(100)));
        Assert.True(writer.ShouldPersist(500, 1000, Start.AddMilliseconds(100)));
    }
}
