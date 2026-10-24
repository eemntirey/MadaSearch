using MadagascarBrowser.Core.Navigation;
using MadagascarBrowser.Core.Security;

namespace MadagascarBrowser.Tests;

public sealed class NativeWebMessagePolicyTests
{
    private const string LocalHost = "startpage.localhost";

    [Theory]
    [InlineData("t", AcceleratorCommand.NewTab)]
    [InlineData("w", AcceleratorCommand.CloseTab)]
    [InlineData("l", AcceleratorCommand.FocusAddressBar)]
    public void TryResolveCommand_AllowsKnownCommandFromBrowserOwnedOrigin(
        string message,
        AcceleratorCommand expected)
    {
        var allowed = NativeWebMessagePolicy.TryResolveCommand(
            "https://startpage.localhost/index.html",
            message,
            LocalHost,
            out var actual);

        Assert.True(allowed);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("https://example.com/")]
    [InlineData("http://startpage.localhost/")]
    [InlineData("https://startpage.localhost.example.com/")]
    [InlineData("https://startpage.localhost:444/")]
    [InlineData("https://user@startpage.localhost/")]
    [InlineData("file:///C:/page.html")]
    [InlineData("not a uri")]
    [InlineData(null)]
    public void TryResolveCommand_RejectsUntrustedOrigin(string? source)
    {
        var allowed = NativeWebMessagePolicy.TryResolveCommand(
            source,
            "w",
            LocalHost,
            out var command);

        Assert.False(allowed);
        Assert.Equal(AcceleratorCommand.None, command);
    }

    [Theory]
    [InlineData("")]
    [InlineData("close-window")]
    [InlineData("javascript:alert(1)")]
    [InlineData("T")]
    [InlineData("w ")]
    public void TryResolveCommand_RejectsUnknownOrNonCanonicalMessage(string message)
    {
        var allowed = NativeWebMessagePolicy.TryResolveCommand(
            "https://startpage.localhost/index.html",
            message,
            LocalHost,
            out var command);

        Assert.False(allowed);
        Assert.Equal(AcceleratorCommand.None, command);
    }
}
