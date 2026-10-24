using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class ZoomLevelTests
{
    [Theory]
    [InlineData(1.0, "100 %")]
    [InlineData(1.25, "125 %")]
    [InlineData(0.75, "75 %")]
    [InlineData(1.01, "101 %")]
    public void Format_AfficheLePourcentage(double factor, string expected)
    {
        Assert.Equal(expected, ZoomLevel.Format(factor));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Format_ValeurInvalide_RetombeSur100Pourcent(double factor)
    {
        Assert.Equal("100 %", ZoomLevel.Format(factor));
    }

    [Fact]
    public void Reset_RameneA100Pourcent()
    {
        Assert.Equal(1.0, ZoomLevel.Reset(2.5));
    }
}
