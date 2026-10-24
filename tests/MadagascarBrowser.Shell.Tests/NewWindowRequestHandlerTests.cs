using MadagascarBrowser.Core.Navigation;
using MadagascarBrowser.Shell;

namespace MadagascarBrowser.Shell.Tests;

/// <summary>
/// Hôte d'onglets de substitution : enregistre les ouvertures demandées
/// et permet de simuler un échec.
/// </summary>
internal sealed class RecordingTabHost : ITabHost
{
    private readonly bool _shouldFail;

    public RecordingTabHost(bool shouldFail = false)
    {
        _shouldFail = shouldFail;
    }

    public List<string> OpenedUrls { get; } = new();

    public int ClosedTabs { get; private set; }

    public List<int> LibraryTabs { get; } = new();

    public int DownloadsOpened { get; private set; }

    public int SettingsOpened { get; private set; }

    public int SwitchedTabs { get; private set; }

    public int ReopenedTabs { get; private set; }

    public int FavoritesAdded { get; private set; }

    public int PrivateWindowsOpened { get; private set; }

    public bool FullScreen { get; private set; }

    public List<AcceleratorCommand> ExecutedCommands { get; } = new();

    public TabViewModel NewTab(string? url)
    {
        if (_shouldFail)
            throw new InvalidOperationException("Ouverture d'onglet refusée par le test.");

        // L'interface autorise l'URL absente ; l'hôte de test la traite comme vide.
        url ??= string.Empty;
        OpenedUrls.Add(url);
        return new TabViewModel { Url = url, Title = "Nouvel onglet" };
    }

    public void CloseActiveTab() => ClosedTabs++;

    public TabViewModel OpenLibrary(int selectedTab)
    {
        LibraryTabs.Add(selectedTab);
        return new TabViewModel();
    }

    public TabViewModel OpenDownloads()
    {
        DownloadsOpened++;
        return new TabViewModel();
    }

    public TabViewModel OpenSettings()
    {
        SettingsOpened++;
        return new TabViewModel();
    }

    public void SwitchToNextTab() => SwitchedTabs++;

    public void SwitchToPreviousTab() => SwitchedTabs--;

    public void ReopenClosedTab() => ReopenedTabs++;

    public void AddFavoriteToActiveTab() => FavoritesAdded++;

    public void SetFullScreen(bool enabled) => FullScreen = enabled;

    public void Execute(AcceleratorCommand command) => ExecutedCommands.Add(command);

    public void OpenPrivateWindow() => PrivateWindowsOpened++;
}

public class NewWindowRequestHandlerTests
{
    [Fact]
    public void Handle_UneUriReelle_OuvreUnOngletSurCetteUri()
    {
        var host = new RecordingTabHost();
        var handler = new NewWindowRequestHandler(host);

        var result = handler.Handle("https://exemple.mg/page");

        Assert.Equal(NewWindowOutcome.OpenedInNewTab, result.Outcome);
        Assert.True(result.Succeeded);
        Assert.Equal("https://exemple.mg/page", Assert.Single(host.OpenedUrls));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("about:blank")]
    public void Handle_UneUriVide_DemandeUnOngletVide(string? uri)
    {
        var host = new RecordingTabHost();
        var handler = new NewWindowRequestHandler(host);

        var result = handler.Handle(uri);

        Assert.Equal(NewWindowOutcome.BlankTabOpened, result.Outcome);
        Assert.NotNull(result.Tab);
        Assert.Equal(NewWindowRequestResolver.BlankUrl, Assert.Single(host.OpenedUrls));
    }

    [Fact]
    public void Handle_UnEchecDLOuverture_EstSignaleEtNonAvale()
    {
        var host = new RecordingTabHost(shouldFail: true);
        var handler = new NewWindowRequestHandler(host);

        var result = handler.Handle("https://exemple.mg/page");

        Assert.Equal(NewWindowOutcome.Failed, result.Outcome);
        Assert.False(result.Succeeded);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
        Assert.Empty(host.OpenedUrls);
    }

    [Fact]
    public void Handle_UneUriVideEtUnEchec_EstSignale()
    {
        var host = new RecordingTabHost(shouldFail: true);
        var handler = new NewWindowRequestHandler(host);

        var result = handler.Handle("about:blank");

        Assert.Equal(NewWindowOutcome.Failed, result.Outcome);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public void Constructeur_SansHote_EstRefuse()
    {
        Assert.Throws<ArgumentNullException>(() => new NewWindowRequestHandler(null!));
    }
}
