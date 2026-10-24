using System.Diagnostics;
using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;

namespace MadagascarBrowser.E2E;

/// <summary>
/// Une instance réelle de Madagascar Browser, isolée par une racine de données
/// temporaire (--data-root) pour que la recette ne touche jamais le profil
/// de l'utilisateur.
/// </summary>
internal sealed class BrowserSession : IDisposable
{
    private readonly string _screenshotsDirectory;

    private BrowserSession(
        Application application,
        UIA3Automation automation,
        Window window,
        string dataRoot,
        string screenshotsDirectory)
    {
        App = application;
        Automation = automation;
        Window = window;
        DataRoot = dataRoot;
        _screenshotsDirectory = screenshotsDirectory;
    }

    public Application App { get; }

    public UIA3Automation Automation { get; }

    public Window Window { get; private set; }

    public string DataRoot { get; }

    public static BrowserSession Launch(bool startPrivate = false, string? downloadDirectory = null)
    {
        var repositoryRoot = FindRepositoryRoot();
        var configuration =
#if DEBUG
            "Debug";
#else
            "Release";
#endif
        var executable = Path.Combine(
            repositoryRoot, "src", "MadagascarBrowser", "bin", configuration, "net8.0-windows",
            "MadagascarBrowser.exe");
        if (!File.Exists(executable))
            throw new FileNotFoundException($"Application introuvable : {executable}");

        var dataRoot = Path.Combine(Path.GetTempPath(), "madagascar-e2e", Guid.NewGuid().ToString("N"));
        var screenshotsDirectory = Path.Combine(repositoryRoot, "docs", "captures", "e2e");
        Directory.CreateDirectory(screenshotsDirectory);

        var arguments = $"--data-root \"{dataRoot}\"";
        if (startPrivate)
            arguments += " --private";

        string? previousDownloadDirectory = null;
        if (downloadDirectory is not null)
        {
            previousDownloadDirectory = Environment.GetEnvironmentVariable("MADAGASCAR_E2E_DOWNLOAD_DIR");
            Environment.SetEnvironmentVariable("MADAGASCAR_E2E_DOWNLOAD_DIR", downloadDirectory);
        }

        var automation = new UIA3Automation();
        var application = Application.Launch(executable, arguments);
        try
        {
            application.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(20));
            var window = application.GetMainWindow(automation, TimeSpan.FromSeconds(20))
                         ?? throw new InvalidOperationException("Fenêtre principale absente.");
            var session = new BrowserSession(application, automation, window, dataRoot, screenshotsDirectory);
            session.WaitForElement("AddressBar");
            Thread.Sleep(1500);
            return session;
        }
        catch
        {
            application.Close();
            application.Dispose();
            automation.Dispose();
            throw;
        }
        finally
        {
            if (downloadDirectory is not null)
                Environment.SetEnvironmentVariable("MADAGASCAR_E2E_DOWNLOAD_DIR", previousDownloadDirectory);
        }
    }

    public AutomationElement Find(string automationId) =>
        Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId))
        ?? throw new InvalidOperationException($"Élément introuvable : {automationId}");

    public AutomationElement WaitForElement(string automationId, int timeoutSeconds = 15) =>
        WaitFor(() => Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)),
            $"Élément '{automationId}' absent après {timeoutSeconds} s.", timeoutSeconds)!;

    public AutomationElement WaitForDesktopElement(string automationId, int timeoutSeconds = 15) =>
        WaitFor(() => Automation.GetDesktop().FindFirstDescendant(cf => cf.ByAutomationId(automationId)),
            $"Élément '{automationId}' absent du bureau après {timeoutSeconds} s.", timeoutSeconds)!;

    public AutomationElement WaitForElementByName(string name, int timeoutSeconds = 15) =>
        WaitFor(() => Window.FindFirstDescendant(cf => cf.ByName(name)),
            $"Élément nommé « {name} » absent après {timeoutSeconds} s.", timeoutSeconds)!;

    public void Navigate(string url)
    {
        var address = WaitForElement("AddressBar").AsTextBox();
        address.Text = url;
        address.Focus();
        Keyboard.Press(VirtualKeyShort.RETURN);
    }

    public string WaitForStatus(string expectedSubstring, int timeoutSeconds = 20)
    {
        string? name = null;
        var found = WaitFor(() =>
        {
            name = Window.FindFirstDescendant(cf => cf.ByAutomationId("StatusText"))?.Name;
            return name is not null && name.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase)
                ? name
                : null;
        }, $"Le statut ne contient pas « {expectedSubstring} » (dernier : « {name} »).", timeoutSeconds);

        return found ?? string.Empty;
    }

    public void OpenMenu() => Find("MenuButton").AsButton().Invoke();

    public void ClickMenuItem(string automationId)
    {
        OpenMenu();
        WaitForDesktopElement(automationId).AsButton().Invoke();
    }

    public Window WaitForPrivateWindow(int timeoutSeconds = 15)
    {
        var badge = WaitForDesktopElement("PrivateBadge", timeoutSeconds);
        AutomationElement? current = badge;
        while (current is not null && current.ControlType != ControlType.Window)
            current = current.Parent;

        return current?.AsWindow()
               ?? throw new InvalidOperationException("La fenêtre privée est introuvable depuis son badge.");
    }

    public string WaitForWindowWithTitle(string titleSubstring, int timeoutSeconds = 15)
    {
        string? title = null;
        var found = WaitFor(() =>
        {
            title = Automation.GetDesktop()
                .FindAllChildren(cf => cf.ByControlType(ControlType.Window))
                .Select(window => window.Name)
                .FirstOrDefault(name => name.Contains(titleSubstring, StringComparison.OrdinalIgnoreCase));
            return title;
        }, $"Fenêtre contenant « {titleSubstring} » absente.", timeoutSeconds);

        return found ?? string.Empty;
    }

    public void Capture(string name) => Capture(Window, name);

    public void Capture(AutomationElement element, string name)
    {
        // Éloigne le curseur pour éviter qu'une infobulle de survol ne soit
        // figée dans la capture.
        Mouse.MoveTo(0, 0);
        Thread.Sleep(400);
        var path = Path.Combine(_screenshotsDirectory, $"{name}.png");
        element.CaptureToFile(path);
    }

    public void Dispose()
    {
        try
        {
            if (!App.HasExited)
            {
                App.Close();
                App.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(5));
            }
        }
        catch (Exception)
        {
        }

        App.Dispose();
        Automation.Dispose();

        if (Environment.GetEnvironmentVariable("MADAGASCAR_E2E_KEEP_DATA") == "1")
            return;

        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                if (Directory.Exists(DataRoot))
                    Directory.Delete(DataRoot, recursive: true);
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(250);
            }
            catch (UnauthorizedAccessException)
            {
                Thread.Sleep(250);
            }
        }
    }

    private static T? WaitFor<T>(Func<T?> probe, string failureMessage, int timeoutSeconds) where T : class
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        while (DateTime.UtcNow < deadline)
        {
            var value = probe();
            if (value is not null)
                return value;
            Thread.Sleep(150);
        }

        throw new TimeoutException(failureMessage);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MadagascarBrowser.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Racine du dépôt introuvable (MadagascarBrowser.sln).");
    }
}
