using System.IO;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;

namespace MadagascarBrowser.E2E;

public class BrowserE2ETests
{
    [Fact]
    public void Navigation_LoadsPageAndReflectsTitleInStatusBar()
    {
        using var server = new LocalTestServer();
        using var browser = BrowserSession.Launch();

        browser.Navigate(server.Url("/"));
        var status = browser.WaitForStatus("Accueil E2E");

        Assert.Contains("Accueil E2E", status);
        browser.Capture("01-navigation-accueil");
    }

    [Fact]
    public void BackAndForward_NavigateThroughHistoryWithoutStaleContent()
    {
        using var server = new LocalTestServer();
        using var browser = BrowserSession.Launch();

        browser.Navigate(server.Url("/"));
        browser.WaitForStatus("Accueil E2E");
        browser.Navigate(server.Url("/deux"));
        browser.WaitForStatus("Page Deux");

        browser.WaitForElement("BackButton").AsButton().Invoke();
        browser.WaitForStatus("Accueil E2E");

        browser.WaitForElement("ForwardButton").AsButton().Invoke();
        browser.WaitForStatus("Page Deux");
        browser.Capture("02-historique-retour-avant");
    }

    [Fact]
    public void PrivateWindow_IsExplicitlyMarkedAndOpenedFromMenu()
    {
        using var browser = BrowserSession.Launch();

        browser.ClickMenuItem("MenuPrivateWindow");
        var privateWindow = browser.WaitForPrivateWindow();

        Assert.Contains("privée", privateWindow.Title, StringComparison.OrdinalIgnoreCase);
        browser.Capture(privateWindow, "05-fenetre-privee");
    }

    [Fact]
    public void ClearBrowsingData_DialogClearsSelectedCategories()
    {
        using var browser = BrowserSession.Launch();

        browser.ClickMenuItem("MenuClearData");
        var dialog = browser.WaitForDesktopElement("ClearDataWindow");
        Assert.True(dialog.FindFirstDescendant(cf => cf.ByAutomationId("ClearCookies"))!.AsCheckBox().IsChecked);
        Assert.True(dialog.FindFirstDescendant(cf => cf.ByAutomationId("ClearHistory"))!.AsCheckBox().IsChecked);
        Assert.False(dialog.FindFirstDescendant(cf => cf.ByAutomationId("ClearLocalStorage"))!.AsCheckBox().IsChecked);

        dialog.FindFirstDescendant(cf => cf.ByAutomationId("ClearDataConfirm"))!.AsButton().Invoke();

        var feedback = WaitForText(dialog, "ClearDataFeedback", "effacées");
        Assert.Contains("effacées", feedback);
        browser.Capture(dialog, "06-effacement-donnees");
    }

    [Fact]
    public void Download_ContentDispositionFileNameIsPreserved()
    {
        using var server = new LocalTestServer();
        var downloadDirectory = Path.Combine(Path.GetTempPath(), "madagascar-e2e-downloads", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(downloadDirectory);

        try
        {
            using var browser = BrowserSession.Launch(downloadDirectory: downloadDirectory);
            browser.Navigate(server.Url("/download"));

            var expected = Path.Combine(downloadDirectory, "recu-été.txt");
            WaitForFile(expected);
            Assert.Equal("contenu-e2e", File.ReadAllText(expected));

            // La capture montre aussi le nom réel dans la vue Téléchargements,
            // pas seulement le fichier sur disque.
            Thread.Sleep(700);
            browser.ClickMenuItem("MenuDownloads");
            browser.WaitForElementByName("recu-été.txt");
            browser.Capture("03-download-content-disposition");
        }
        finally
        {
            if (Directory.Exists(downloadDirectory))
                Directory.Delete(downloadDirectory, recursive: true);
        }
    }

    [Fact]
    public void AboutWindow_ReportsRealRuntimeInsteadOfPretending()
    {
        using var browser = BrowserSession.Launch();

        browser.ClickMenuItem("MenuAbout");
        var about = browser.WaitForDesktopElement("AboutWindow");

        var appVersion = about.FindFirstDescendant(cf => cf.ByAutomationId("AppVersion"))!.Name;
        var webViewVersion = about.FindFirstDescendant(cf => cf.ByAutomationId("WebViewVersion"))!.Name;
        var runtimeStatus = about.FindFirstDescendant(cf => cf.ByAutomationId("WebViewRuntimeStatus"))!.Name;

        Assert.False(string.IsNullOrWhiteSpace(appVersion));
        Assert.False(string.IsNullOrWhiteSpace(webViewVersion));
        Assert.Contains("État :", runtimeStatus);
        browser.Capture(about, "07-a-propos-runtime");

        about.FindFirstDescendant(cf => cf.ByAutomationId("AboutClose"))!.AsButton().Invoke();
    }

    [Fact]
    public void DevTools_AreAvailableInDebugBuild()
    {
        using var browser = BrowserSession.Launch();

        browser.OpenMenu();
        browser.WaitForDesktopElement("MenuDevTools");

        browser.Window.Focus();
        Keyboard.Press(VirtualKeyShort.F12);
        var devToolsTitle = browser.WaitForWindowWithTitle("DevTools", 15);
        Assert.Contains("DevTools", devToolsTitle, StringComparison.OrdinalIgnoreCase);

        browser.Capture("04-devtools-ouverts");
    }

    private static string WaitForText(AutomationElement root, string automationId, string expected, int timeoutSeconds = 20)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        string? last = null;
        while (DateTime.UtcNow < deadline)
        {
            last = root.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.Name;
            if (last is not null && last.Contains(expected, StringComparison.OrdinalIgnoreCase))
                return last;
            Thread.Sleep(150);
        }

        throw new TimeoutException($"Texte « {expected} » absent (dernier : « {last} »).");
    }

    private static void WaitForFile(string path, int timeoutSeconds = 20)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        while (DateTime.UtcNow < deadline)
        {
            if (File.Exists(path))
                return;
            Thread.Sleep(200);
        }

        throw new TimeoutException($"Téléchargement absent : {path}");
    }
}
