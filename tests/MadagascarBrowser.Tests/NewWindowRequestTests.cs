using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class NewWindowRequestTests
{
    [Fact]
    public void Resolve_EmptyUri_ReturnsBlankTab()
    {
        Assert.Equal(NewWindowRequestResolver.BlankUrl, NewWindowRequestResolver.Resolve(null));
        Assert.Equal(NewWindowRequestResolver.BlankUrl, NewWindowRequestResolver.Resolve(""));
        Assert.Equal(NewWindowRequestResolver.BlankUrl, NewWindowRequestResolver.Resolve("   "));
    }

    [Fact]
    public void Resolve_AboutBlank_ReturnsBlankTab()
    {
        Assert.Equal(NewWindowRequestResolver.BlankUrl, NewWindowRequestResolver.Resolve("about:blank"));
    }

    [Fact]
    public void Resolve_HttpUri_IsKeptAsIs()
    {
        Assert.Equal("https://exemple.mg/page", NewWindowRequestResolver.Resolve("https://exemple.mg/page"));
    }

    [Fact]
    public void Resolve_UriWithSpaces_IsTrimmed()
    {
        Assert.Equal("https://exemple.mg/page", NewWindowRequestResolver.Resolve("  https://exemple.mg/page  "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("about:blank")]
    [InlineData("  ABOUT:BLANK ")]
    public void IsBlank_EmptyOrAboutBlank_IsBlank(string? uri)
    {
        Assert.True(NewWindowRequestResolver.IsBlank(uri));
    }

    [Fact]
    public void IsBlank_RealUri_IsNotBlank()
    {
        Assert.False(NewWindowRequestResolver.IsBlank("https://exemple.mg/page"));
    }
}
