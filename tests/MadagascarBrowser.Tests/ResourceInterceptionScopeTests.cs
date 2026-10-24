using MadagascarBrowser.Core.Blocking;

namespace MadagascarBrowser.Tests;

public class ResourceInterceptionScopeTests
{
    [Theory]
    [InlineData("https://exemple.mg/page")]
    [InlineData("http://exemple.mg/page")]
    public void IsInterceptable_LesRessourcesHttpSontInterceptees(string url)
    {
        Assert.True(ResourceInterceptionScope.IsInterceptable(url));
    }

    [Theory]
    [InlineData("file:///C:/Users/moi/pages/ma-page.mhtml")]
    [InlineData("about:blank")]
    [InlineData("data:text/html,<p>bonjour</p>")]
    [InlineData("chrome://settings")]
    public void IsInterceptable_LesRessourcesHorsHttpNeSontPasInterceptees(string url)
    {
        // Les pages enregistrées hors ligne (file://) doivent rester hors d'atteinte
        // du bloqueur de publicités et de l'économiseur de données.
        Assert.False(ResourceInterceptionScope.IsInterceptable(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("pas une url")]
    public void IsInterceptable_UneUrlInvalideEstIgnoree(string? url)
    {
        Assert.False(ResourceInterceptionScope.IsInterceptable(url));
    }

    [Fact]
    public void UrlPatterns_CouvrentUniquementHttpEtHttps()
    {
        Assert.Equal(new[] { "http://*", "https://*" }, ResourceInterceptionScope.UrlPatterns);
    }

    [Fact]
    public void FetchPatterns_CouvrentUniquementHttpEtHttps()
    {
        Assert.Equal(new[] { "http://*/*", "https://*/*" }, ResourceInterceptionScope.FetchPatterns);
    }
}
