namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Points d'entrée du navigateur utilisés par les vues.
    /// Une vue ne cherche plus le gestionnaire d'onglets dans le DataContext de la
    /// fenêtre : elle reçoit son hôte, ce qui la rend testable et permet d'y
    /// injecter un comportement différent (navigation privée, tests).
    /// </summary>
    public interface ITabHost
    {
        /// <summary>
        /// Ouvre un nouvel onglet sur l'URL indiquée et l'active.
        /// </summary>
        /// <param name="url">URL à ouvrir.</param>
        /// <returns>L'onglet créé.</returns>
        TabViewModel NewTab(string url);

        /// <summary>Ferme l'onglet actif.</summary>
        void CloseActiveTab();

        /// <summary>Affiche la bibliothèque, sur l'onglet demandé (historique, favoris, hors ligne).</summary>
        TabViewModel OpenLibrary(int selectedTab);

        /// <summary>Affiche la vue des téléchargements.</summary>
        TabViewModel OpenDownloads();

        /// <summary>Affiche les paramètres.</summary>
        TabViewModel OpenSettings();
    }
}
