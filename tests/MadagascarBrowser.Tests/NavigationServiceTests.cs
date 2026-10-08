using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class NavigationServiceTests
{
    private readonly NavigationService _nav = new();

    [Fact]
    public void ResolveNavigation_GivenDomainWithoutScheme_PrefixesHttps()
    {
        var result = _nav.ResolveNavigation("ged.mada.gov");
        Assert.StartsWith("https://ged.mada.gov", result);
    }

    [Fact]
    public void ResolveNavigation_SearchUsesSelectedProvider()
    {
        var result = _nav.ResolveNavigation("histoire Madagascar", "https://duckduckgo.com/?q=");

        Assert.StartsWith("https://duckduckgo.com/?q=", result);
        Assert.Contains("histoire%20Madagascar", result);
    }
}
