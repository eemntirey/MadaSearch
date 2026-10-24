using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class SecurityIndicatorTests
{
    [Fact]
    public void Secure_AfficheLeCadenasEtLaCouleurVerte()
    {
        var indicator = SecurityIndicatorResolver.Resolve(ConnectionSecurity.Secure);

        Assert.Equal("\uE72E", indicator.Glyph);
        Assert.Equal("SecureBrush", indicator.BrushKey);
        Assert.Equal("Connexion chiffrée (HTTPS)", indicator.Label);
    }

    [Fact]
    public void Insecure_AfficheLAvertissementEtLaCouleurOrange()
    {
        var indicator = SecurityIndicatorResolver.Resolve(ConnectionSecurity.Insecure);

        Assert.Equal("\uE7BA", indicator.Glyph);
        Assert.Equal("WarnBrush", indicator.BrushKey);
        Assert.Equal("Connexion non chiffrée (HTTP)", indicator.Label);
    }

    [Fact]
    public void CertError_AfficheLeDangerEtLaCouleurRouge()
    {
        var indicator = SecurityIndicatorResolver.Resolve(ConnectionSecurity.CertError);

        Assert.Equal("\uEA39", indicator.Glyph);
        Assert.Equal("DangerBrush", indicator.BrushKey);
        Assert.Equal("Erreur de certificat", indicator.Label);
    }

    [Fact]
    public void None_AfficheLeGlobeEtLaCouleurDiscrete()
    {
        var indicator = SecurityIndicatorResolver.Resolve(ConnectionSecurity.None);

        Assert.Equal("\uE774", indicator.Glyph);
        Assert.Equal("FaintBrush", indicator.BrushKey);
        Assert.Equal("Aucune connexion réseau", indicator.Label);
    }

    [Fact]
    public void LesQuatreEtatsSontVisuellementDistincts()
    {
        var indicators = new[]
        {
            SecurityIndicatorResolver.Resolve(ConnectionSecurity.None),
            SecurityIndicatorResolver.Resolve(ConnectionSecurity.Insecure),
            SecurityIndicatorResolver.Resolve(ConnectionSecurity.Secure),
            SecurityIndicatorResolver.Resolve(ConnectionSecurity.CertError)
        };

        Assert.Equal(indicators.Length, indicators.Select(i => i.Glyph).Distinct().Count());
        Assert.Equal(indicators.Length, indicators.Select(i => i.BrushKey).Distinct().Count());
        Assert.Equal(indicators.Length, indicators.Select(i => i.Label).Distinct().Count());
    }
}
