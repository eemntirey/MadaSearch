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
    public void Evaluate_Video_ReturnsBlock()
    {
        var ds = new DataSaverService { Enabled = true };
        Assert.Equal(DataSaverDecision.Block, ds.Evaluate(new Uri("https://x.com/v.mp4"), "video/mp4"));
    }
}