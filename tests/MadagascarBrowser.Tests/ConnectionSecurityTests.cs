using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class ConnectionSecurityTests
{
    private const string LocalHost = "startpage.local";

    [Fact]
    public void Resolve_HttpsUrl_IsSecure()
    {
        var state = ConnectionSecurityResolver.Resolve("https://exemple.mg/page", false, LocalHost);

        Assert.Equal(ConnectionSecurity.Secure, state);
    }

    [Fact]
    public void Resolve_HttpUrl_IsInsecure()
    {
        var state = ConnectionSecurityResolver.Resolve("http://exemple.mg/page", false, LocalHost);

        Assert.Equal(ConnectionSecurity.Insecure, state);
    }

    [Fact]
    public void Resolve_CertificateError_TakesPrecedenceOverScheme()
    {
        var state = ConnectionSecurityResolver.Resolve("https://exemple.mg/page", true, LocalHost);

        Assert.Equal(ConnectionSecurity.CertError, state);
    }

    [Fact]
    public void Resolve_StartPage_IsNone()
    {
        var state = ConnectionSecurityResolver.Resolve("https://startpage.local/index.html", false, LocalHost);

        Assert.Equal(ConnectionSecurity.None, state);
    }

    [Fact]
    public void Resolve_AboutBlank_IsNone()
    {
        var state = ConnectionSecurityResolver.Resolve("about:blank", false, LocalHost);

        Assert.Equal(ConnectionSecurity.None, state);
    }

    [Fact]
    public void Resolve_LocalFile_IsNone()
    {
        var state = ConnectionSecurityResolver.Resolve("file:///C:/temp/page.mhtml", false, LocalHost);

        Assert.Equal(ConnectionSecurity.None, state);
    }

    [Theory]
    [InlineData("CertificateExpired")]
    [InlineData("CertificateCommonNameIsIncorrect")]
    [InlineData("CertificateRevoked")]
    [InlineData("CertificateIsInvalid")]
    [InlineData("ClientCertificateContainsErrors")]
    public void IsCertificateError_CertificateStatuses_AreDetected(string status)
    {
        Assert.True(ConnectionSecurityResolver.IsCertificateError(status));
    }

    [Fact]
    public void IsCertificateError_UnknownCertificateStatus_IsDetected()
    {
        // Tout état d'erreur mentionnant un certificat est une erreur de certificat.
        Assert.True(ConnectionSecurityResolver.IsCertificateError("CertificateHostNameMismatch"));
    }

    [Theory]
    [InlineData("HostNameNotResolved")]
    [InlineData("ConnectionReset")]
    [InlineData("Unknown")]
    [InlineData("")]
    [InlineData(null)]
    public void IsCertificateError_OtherStatuses_AreNotDetected(string? status)
    {
        Assert.False(ConnectionSecurityResolver.IsCertificateError(status));
    }
}
