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

    [Theory]
    [InlineData("google", "https://www.google.mg/search?q=")]
    [InlineData("duckduckgo", "https://duckduckgo.com/?q=")]
    [InlineData("bing", "https://www.bing.com/search?q=")]
    public void ResolveNavigation_ConfiguredProviderIsUsed(string provider, string expectedTemplate)
    {
        var navigation = new NavigationService(provider);

        Assert.StartsWith(expectedTemplate, navigation.ResolveNavigation("recherche Madagascar"));
    }

    [Fact]
    public void ResolveNavigation_UnknownProviderUsesGoogleMadagascar()
    {
        var navigation = new NavigationService("unsupported");

        Assert.StartsWith("https://www.google.mg/search?q=", navigation.ResolveNavigation("recherche"));
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

    [Fact]
    public void ResolveNavigation_GivenFileUrl_KeepsTheScheme()
    {
        // Une page enregistrée hors ligne s'ouvre par son URI file:// :
        // la préfixer de « https:// » la rendrait inatteignable.
        var result = _nav.ResolveNavigation("file:///C:/Users/moi/page.mhtml", "https://duckduckgo.com/?q=");

        Assert.Equal("file:///C:/Users/moi/page.mhtml", result);
    }

    [Fact]
    public void LooksLikeUrl_GivenFileUrl_IsAUrl()
    {
        Assert.True(_nav.LooksLikeUrl("file:///C:/Users/moi/page.mhtml"));
    }

    [Fact]
    public void ResolveNavigation_GivenAboutUrl_KeepsTheScheme()
    {
        var result = _nav.ResolveNavigation("about:blank", "https://duckduckgo.com/?q=");

        Assert.Equal("about:blank", result);
    }

    [Fact]
    public void ResolveNavigation_GivenLocalPath_KeepsThePath()
    {
        // Un chemin Windows absolu est un fichier local, pas une recherche.
        var result = _nav.ResolveNavigation(@"C:\Users\moi\page.mhtml", "https://duckduckgo.com/?q=");

        Assert.Equal(@"C:\Users\moi\page.mhtml", result);
    }

    [Fact]
    public void ResolveNavigation_GivenFileUrl_ProducesALocalFileTarget()
    {
        var url = _nav.ResolveNavigation("file:///C:/Users/moi/page.mhtml", "https://duckduckgo.com/?q=");

        var kind = NavigationTargetResolver.Resolve(url, out var target, out var title);

        Assert.Equal(NavigationTargetKind.LocalFile, kind);
        Assert.StartsWith("file:///", target, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("page.mhtml", title);
    }
}
