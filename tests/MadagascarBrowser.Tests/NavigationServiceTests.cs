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

    [Fact]
    public void ResolveNavigation_GivenFileUrl_IsNotModified()
    {
        var result = _nav.ResolveNavigation("file:///C:/Temp/page.mhtml");

        Assert.Equal("file:///C:/Temp/page.mhtml", result);
    }

    [Fact]
    public void ResolveNavigation_GivenFileUrlWithSpaces_KeepsFileScheme()
    {
        var result = _nav.ResolveNavigation("file:///C:/mon dossier/page.mhtml");

        Assert.StartsWith("file:///", result);
        Assert.DoesNotContain("https://file", result);
    }

    [Fact]
    public void ResolveNavigation_GivenRootedWindowsPath_BecomesFileUrl()
    {
        var result = _nav.ResolveNavigation(@"C:\Temp\page.mhtml");

        Assert.StartsWith("file:///", result);
        Assert.DoesNotContain("https://C", result);
    }

    [Fact]
    public void ResolveNavigation_GivenDriveLetterWithText_IsTreatedAsSearch()
    {
        var result = _nav.ResolveNavigation("c: documents", "https://duckduckgo.com/?q=");

        Assert.StartsWith("https://duckduckgo.com/?q=", result);
    }

    [Fact]
    public void LooksLikeUrl_GivenFileUrl_IsAUrl()
    {
        Assert.True(_nav.LooksLikeUrl("file:///C:/Temp/page.mhtml"));
    }
}
