using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class NavigationFailureResolverTests
{
    [Theory]
    [InlineData("HostNameNotResolved", "Site introuvable")]
    [InlineData("CannotConnect", "Serveur inaccessible")]
    [InlineData("Timeout", "Délai de réponse dépassé")]
    [InlineData("ConnectionReset", "Connexion interrompue")]
    [InlineData("CertificateExpired", "Certificat non valide")]
    [InlineData("ErrorHttpInvalidServerResponse", "Réponse du site invalide")]
    public void Resolve_UsesMessageSpecificToWebErrorStatus(string status, string expectedTitle)
    {
        var failure = NavigationFailureResolver.Resolve(status);

        Assert.Equal(expectedTitle, failure.Title);
        Assert.False(string.IsNullOrWhiteSpace(failure.Message));
    }

    [Fact]
    public void Resolve_UnknownStatusIncludesStatusInMessage()
    {
        var failure = NavigationFailureResolver.Resolve("NewWebViewError");

        Assert.Contains("NewWebViewError", failure.Message);
    }

    [Fact]
    public void Resolve_EmptyStatusIsRejected()
    {
        Assert.Throws<ArgumentException>(() => NavigationFailureResolver.Resolve(""));
    }
}
