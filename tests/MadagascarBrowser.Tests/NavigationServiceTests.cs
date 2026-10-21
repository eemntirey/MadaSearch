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

    [Fact]
    public void LooksLikeUrl_GivenTextWithSpace_IsNotAUrl()
    {
        Assert.False(_nav.LooksLikeUrl("python 3.11"));
    }

    [Fact]
    public void LooksLikeUrl_GivenDomainWithTld_IsAUrl()
    {
        Assert.True(_nav.LooksLikeUrl("exemple.mg"));
    }

    [Fact]
    public void LooksLikeUrl_GivenAbsoluteUrl_IsAUrl()
    {
        Assert.True(_nav.LooksLikeUrl("https://a.b"));
    }

    [Fact]
    public void ResolveNavigation_GivenTextWithSpace_IsTreatedAsSearch()
    {
        var result = _nav.ResolveNavigation("python 3.11", "https://duckduckgo.com/?q=");

        Assert.StartsWith("https://duckduckgo.com/?q=", result);
        Assert.DoesNotContain("https://python", result);
    }
}
