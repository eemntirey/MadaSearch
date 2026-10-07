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
}
