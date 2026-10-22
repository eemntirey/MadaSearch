using MadagascarBrowser.Core.Blocking;
using System;

namespace MadagascarBrowser.Tests;

public class DataSaverServiceTests
{
    [Fact]
    public void Evaluate_Disabled_ReturnsPass()
    {
        var ds = new DataSaverService { Enabled = false };
        Assert.Equal(DataSaverDecision.Pass, ds.Evaluate(new Uri("https://x.com/img.jpg"), "image/jpeg"));
    }

    [Fact]
    public void Evaluate_ImageLarge_ReturnsPlaceholder()
    {
        var ds = new DataSaverService { Enabled = true };
        Assert.Equal(DataSaverDecision.Placeholder, ds.Evaluate(new Uri("https://x.com/big.jpg"), "image/jpeg", 300000));
    }

    [Fact]
    public void Evaluate_ImageAboveConfiguredThreshold_ReturnsPlaceholder()
    {
        var ds = new DataSaverService { Enabled = true, ImageThresholdBytes = 200 * 1024 };

        Assert.Equal(
            DataSaverDecision.Placeholder,
            ds.Evaluate(new Uri("https://x.com/image"), "image/jpeg", 200 * 1024 + 1));
    }

    [Fact]
    public void Evaluate_ImageAtOrBelowThreshold_ReturnsPass()
    {
        var ds = new DataSaverService { Enabled = true, ImageThresholdBytes = 200 * 1024 };

        Assert.Equal(DataSaverDecision.Pass, ds.Evaluate(new Uri("https://x.com/image"), "image/jpeg", 200 * 1024));
        Assert.Equal(DataSaverDecision.Pass, ds.Evaluate(new Uri("https://x.com/image"), "image/jpeg", 200 * 1024 - 1));
    }

    [Fact]
    public void Evaluate_ImageThreshold_IsConfigurable()
    {
        var service = new DataSaverService { Enabled = true, ImageThresholdBytes = 100_000 };

        Assert.Equal(DataSaverDecision.Pass, service.Evaluate(new Uri("https://x.com/image"), "image/jpeg", 100_000));
        Assert.Equal(DataSaverDecision.Placeholder, service.Evaluate(new Uri("https://x.com/image"), "image/jpeg", 100_001));
    }

    [Fact]
    public void ImageThresholdBytes_NonPositiveValue_IsRejected()
    {
        var service = new DataSaverService();

        Assert.Throws<ArgumentOutOfRangeException>(() => service.ImageThresholdBytes = 0);
    }

    [Fact]
    public void Evaluate_ImageWithoutContentLength_ReturnsPass()
    {
        var ds = new DataSaverService { Enabled = true };

        Assert.Equal(DataSaverDecision.Pass, ds.Evaluate(new Uri("https://x.com/image"), "image/jpeg"));
    }

    [Fact]
    public void Evaluate_Video_ReturnsBlock()
    {
        var ds = new DataSaverService { Enabled = true };
        Assert.Equal(DataSaverDecision.Block, ds.Evaluate(new Uri("https://x.com/video"), "video/mp4"));
        Assert.Equal(DataSaverDecision.Block, ds.Evaluate(new Uri("https://x.com/v.mp4"), string.Empty));
        Assert.Equal(DataSaverDecision.Pass, ds.Evaluate(new Uri("https://x.com/v.mp4"), "text/html"));
    }

    [Fact]
    public void Evaluate_ExemptDomain_ReturnsPassForImageAndVideo()
    {
        var ds = new DataSaverService { Enabled = true };

        Assert.Equal(DataSaverDecision.Pass, ds.Evaluate(new Uri("https://trusted.example/image"), "image/jpeg", 500_000, true));
        Assert.Equal(DataSaverDecision.Pass, ds.Evaluate(new Uri("https://trusted.example/video"), "video/mp4", 2_000_000, true));
    }
}