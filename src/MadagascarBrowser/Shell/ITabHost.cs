namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Contrat minimal que la vue d'onglet utilise pour piloter l'hôte de navigation.
    /// Permet d'injecter un hôte factice dans les tests sans instancier WebView2.
    /// </summary>
    public interface ITabHost
    {
        TabViewModel? Active { get; }
        TabViewModel NewTab(string url = "https://" + WebViewEnvironment.LocalHost + "/index.html");
        void Close(TabViewModel tab);
        TabViewModel OpenLibrary(int selectedTab = 0);
        TabViewModel OpenDownloads();
        TabViewModel OpenSettings();
    }
}
