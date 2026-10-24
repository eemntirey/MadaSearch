using MadagascarBrowser.Core.Navigation;

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
        /// <param name="url">URL à ouvrir. Absente, la page d'accueil est ouverte.</param>
        /// <returns>L'onglet créé.</returns>
        TabViewModel NewTab(string? url = null);

        /// <summary>Ferme l'onglet actif.</summary>
        void CloseActiveTab();

        /// <summary>Affiche la bibliothèque, sur l'onglet demandé (historique, favoris, hors ligne).</summary>
        TabViewModel OpenLibrary(int selectedTab);

        /// <summary>Affiche la vue des téléchargements.</summary>
        TabViewModel OpenDownloads();

        /// <summary>Affiche les paramètres.</summary>
        TabViewModel OpenSettings();

        /// <summary>Active l'onglet suivant (Ctrl+Tab).</summary>
        void SwitchToNextTab();

        /// <summary>Active l'onglet précédent (Ctrl+Maj+Tab).</summary>
        void SwitchToPreviousTab();

        /// <summary>Rouvre le dernier onglet fermé (Ctrl+Maj+T).</summary>
        void ReopenClosedTab();

        /// <summary>Ajoute l'onglet actif aux favoris (Ctrl+D).</summary>
        void AddFavoriteToActiveTab();

        /// <summary>
        /// Signale le passage en plein écran : la barre d'onglets et la barre d'état
        /// sont masquées pour laisser toute la place à la page.
        /// </summary>
        void SetFullScreen(bool enabled);

        /// <summary>
        /// Exécute une commande applicative issue d'un raccourci clavier.
        /// </summary>
        void Execute(AcceleratorCommand command);
    }
}
