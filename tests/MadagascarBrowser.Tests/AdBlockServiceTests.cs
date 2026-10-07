using MadagascarBrowser.Core.Blocking;
using System;

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
    public void BlockedCount_Increments()
    {
        var ad = new AdBlockService();
        var c0 = ad.BlockedCount;
        ad.ShouldBlock(new Uri("https://pageads.info/ads/x.js"));
        Assert.True(ad.BlockedCount > c0);
    }
}