using System;
using System.IO;
using MadagascarBrowser.Core.Paths;

namespace MadagascarBrowser.Tests;

public class BrowserPathsTests
{
    [Fact]
    public void Default_RepertoireSousLocalAppData()
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MadagascarBrowser");

        Assert.Equal(expected, BrowserPaths.Default.Root, ignoreCase: true);
    }

    [Fact]
    public void WebViewProfile_DansLeRepertoireApplicatif()
    {
        var paths = BrowserPaths.Default;

        Assert.Equal(
            Path.Combine(paths.Root, "WebView2"),
            paths.WebViewProfilePath,
            ignoreCase: true);
    }

    [Fact]
    public void DonneesEcrites_DansLeRepertoireApplicatif()
    {
        var paths = BrowserPaths.Default;

        Assert.Equal(Path.Combine(paths.Root, "browser.db"), paths.DatabasePath, ignoreCase: true);
        Assert.Equal(Path.Combine(paths.Root, "offline"), paths.OfflineDirectory, ignoreCase: true);
        Assert.Equal(Path.Combine(paths.Root, "logs"), paths.LogsDirectory, ignoreCase: true);
        Assert.Equal(Path.Combine(paths.Root, "logs", "app.log"), paths.LogFilePath, ignoreCase: true);
    }

    [Fact]
    public void ForRoot_AccepteUneRacinePersonnalisee()
    {
        var root = Path.Combine(Path.GetTempPath(), "mb-test-" + Guid.NewGuid().ToString("N"));

        var paths = BrowserPaths.ForRoot(root);

        Assert.Equal(Path.GetFullPath(root), paths.Root, ignoreCase: true);
        Assert.StartsWith(Path.GetFullPath(root), paths.WebViewProfilePath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ForRoot_RefuseUneRacineVide()
    {
        Assert.Throws<ArgumentException>(() => BrowserPaths.ForRoot("  "));
    }

    [Fact]
    public void FindDefaultProfileLeftovers_DetecteUnProfilACoteDeLExecutable()
    {
        var exeDirectory = Path.Combine(Path.GetTempPath(), "mb-leftover-" + Guid.NewGuid().ToString("N"));
        var localAppData = Path.Combine(Path.GetTempPath(), "mb-localappdata-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(exeDirectory);
        Directory.CreateDirectory(localAppData);
        var leftover = Path.Combine(exeDirectory, "MadagascarBrowser.WebView2");
        Directory.CreateDirectory(leftover);

        try
        {
            var leftovers = BrowserPaths.FindDefaultProfileLeftovers(exeDirectory, localAppData, "MadagascarBrowser");

            Assert.Contains(leftover, leftovers);
        }
        finally
        {
            Directory.Delete(exeDirectory, recursive: true);
            Directory.Delete(localAppData, recursive: true);
        }
    }

    [Fact]
    public void FindDefaultProfileLeftovers_NeSignaleRienSiLeDossierEstPropre()
    {
        var exeDirectory = Path.Combine(Path.GetTempPath(), "mb-clean-" + Guid.NewGuid().ToString("N"));
        var localAppData = Path.Combine(Path.GetTempPath(), "mb-clean-local-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(exeDirectory);
        Directory.CreateDirectory(localAppData);

        try
        {
            var leftovers = BrowserPaths.FindDefaultProfileLeftovers(exeDirectory, localAppData, "MadagascarBrowser");

            Assert.Empty(leftovers);
        }
        finally
        {
            Directory.Delete(exeDirectory, recursive: true);
            Directory.Delete(localAppData, recursive: true);
        }
    }
}
