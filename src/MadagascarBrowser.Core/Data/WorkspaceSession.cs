namespace MadagascarBrowser.Core.Data
{
    /// <summary>
    /// Session améliorée avec espaces de travail nommés, permettant la
    /// sauvegarde et la restauration explicites des groupes d'onglets.
    /// </summary>
    public class WorkspaceSession
    {
        /// <summary>Nom de l'espace de travail.</summary>
        public string WorkspaceName { get; set; } = "Travail";

        /// <summary>Index de l'onglet actif lors de la sauvegarde.</summary>
        public int ActiveTabIndex { get; set; }

        /// <summary>URL de chaque onglet ouvert.</summary>
        public System.Collections.Generic.List<string> Urls { get; } = new();

        /// <summary>Info des groupes d'onglets au moment de la sauvegarde.</summary>
        public System.Collections.Generic.List<TabGroup> Groups { get; } = new();
    }
}
