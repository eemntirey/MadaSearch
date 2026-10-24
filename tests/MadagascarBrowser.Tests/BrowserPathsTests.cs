using MadagascarBrowser.Core.Paths;

namespace MadagascarBrowser.Tests;

public class BrowserPathsTests
{
    [Fact]
    public void ForRoot_GroupsAllDataUnderRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "madagascar-browser-paths");

        var paths = BrowserPaths.ForRoot(root);

        Assert.Equal(Path.GetFullPath(root), paths.Root);
        Assert.Equal(Path.Combine(paths.Root, "browser.db"), paths.DatabasePath);
        Assert.Equal(Path.Combine(paths.Root, "WebView2"), paths.WebViewProfilePath);
        Assert.Equal(Path.Combine(paths.Root, "WebView2-Private"), paths.PrivateWebViewProfilePath);
        Assert.Equal(Path.Combine(paths.Root, "offline"), paths.OfflineDirectory);
        Assert.Equal(Path.Combine(paths.Root, "logs", "app.log"), paths.LogFilePath);
    }

    [Fact]
    public void ForRoot_EmptyRoot_Throws()
    {
        Assert.Throws<ArgumentException>(() => BrowserPaths.ForRoot("  "));
    }

    [Fact]
    public void ComposeRoot_UsesApplicationFolderName()
    {
        var root = BrowserPaths.ComposeRoot("/base");

        Assert.Equal(Path.Combine("/base", BrowserPaths.ApplicationFolderName), root);
    }

    [Fact]
    public void FindDefaultProfileLeftovers_NoLeftovers_ReturnsEmpty()
    {
        var executableDirectory = Path.Combine(Path.GetTempPath(), "madagascar-no-leftover-" + Guid.NewGuid().ToString("N"));
        var localAppData = Path.Combine(Path.GetTempPath(), "madagascar-no-leftover-data-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(executableDirectory);
        Directory.CreateDirectory(localAppData);

        var leftovers = BrowserPaths.FindDefaultProfileLeftovers(executableDirectory, localAppData, "MadagascarBrowser");

        Assert.Empty(leftovers);
    }

    [Fact]
    public void FindDefaultProfileLeftovers_DefaultProfileBesideExecutable_IsReported()
    {
        var executableDirectory = Path.Combine(Path.GetTempPath(), "madagascar-leftover-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(executableDirectory, "MadagascarBrowser.WebView2"));

        var leftovers = BrowserPaths.FindDefaultProfileLeftovers(
            executableDirectory,
            Path.Combine(Path.GetTempPath(), "madagascar-unused"),
            "MadagascarBrowser");

        Assert.Contains(Path.Combine(executableDirectory, "MadagascarBrowser.WebView2"), leftovers);
    }

    [Fact]
    public void FindDefaultProfileLeftovers_EbWebViewBesideExecutable_IsReported()
    {
        var executableDirectory = Path.Combine(Path.GetTempPath(), "madagascar-leftover-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(executableDirectory, "EBWebView"));

        var leftovers = BrowserPaths.FindDefaultProfileLeftovers(
            executableDirectory,
            Path.Combine(Path.GetTempPath(), "madagascar-unused"),
            "MadagascarBrowser");

        Assert.Contains(Path.Combine(executableDirectory, "EBWebView"), leftovers);
    }

    [Fact]
    public void FindDefaultProfileLeftovers_EmptyInputs_ReturnsEmpty()
    {
        Assert.Empty(BrowserPaths.FindDefaultProfileLeftovers("", "", ""));
        Assert.Empty(BrowserPaths.FindDefaultProfileLeftovers(null!, null!, null!));
    }
}
