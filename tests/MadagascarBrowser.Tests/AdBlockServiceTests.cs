using MadagascarBrowser.Core.Blocking;
using System;
using System.IO;

namespace MadagascarBrowser.Tests;

public class AdBlockServiceTests
{
    [Fact]
    public void ShouldBlock_KnownDomain_ReturnsTrue()
    {
        var ad = new AdBlockService();
        Assert.True(ad.ShouldBlock(new Uri("https://pageads.info/ads/x.js")));
    }

    [Fact]
    public void ShouldBlock_NonBlocked_ReturnsFalse()
    {
        var ad = new AdBlockService();
        Assert.False(ad.ShouldBlock(new Uri("https://www.midi.mg/style.css")));
    }

    [Fact]
    public void ShouldBlock_SubdomainOfBlocked_ReturnsTrue()
    {
        var ad = new AdBlockService();
        Assert.True(ad.ShouldBlock(new Uri("https://sub.tracker.io/pixel.png")));
    }

    [Fact]
    public void SetDomainExempt_ExemptsDomain()
    {
        var ad = new AdBlockService();
        ad.SetDomainExempt("tracker.io");
        Assert.False(ad.ShouldBlock(new Uri("https://tracker.io/ad.js")));
    }

    [Fact]
    public void DomainExemption_IsDetectedForHostAndSubdomain()
    {
        var ad = new AdBlockService();
        ad.SetDomainExempt("tracker.io");

        Assert.True(ad.IsDomainExempt(new Uri("https://tracker.io/image.jpg")));
        Assert.True(ad.IsDomainExempt(new Uri("https://cdn.tracker.io/video")));
        Assert.False(ad.IsDomainExempt(new Uri("https://nottracker.io/image.jpg")));
    }

    [Fact]
    public void SetDomainExempt_ExemptsSubdomainsButNotLookalikeDomains()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var filterListPath = Path.Combine(directory, "filters.txt");
        File.WriteAllText(filterListPath, "||tracker.io^\n||nottracker.io^\n");
        try
        {
            var ad = new AdBlockService(filterListPath);
            ad.SetDomainExempt("tracker.io");

            Assert.False(ad.ShouldBlock(new Uri("https://tracker.io/ad.js")));
            Assert.False(ad.ShouldBlock(new Uri("https://sub.tracker.io/ad.js")));
            Assert.True(ad.ShouldBlock(new Uri("https://nottracker.io/ad.js")));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void BlockedCount_Increments()
    {
        var ad = new AdBlockService();
        var c0 = ad.BlockedCount;
        ad.ShouldBlock(new Uri("https://pageads.info/ads/x.js"));
        Assert.True(ad.BlockedCount > c0);
    }

    [Fact]
    public void ShouldBlock_WhenDisabled_ReturnsFalse()
    {
        var ad = new AdBlockService { Enabled = false };

        Assert.False(ad.ShouldBlock(new Uri("https://pageads.info/ads/x.js")));
    }
}