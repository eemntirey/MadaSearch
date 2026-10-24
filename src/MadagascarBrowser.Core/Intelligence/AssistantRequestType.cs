namespace MadagascarBrowser.Core.Intelligence
{
    /// <summary>
    /// Type d'assistance demandée à un service d'assistance IA.
    /// Aucune transmission automatique du contenu complet des pages.
    /// </summary>
    public enum AssistantRequestType
    {
        /// <summary>Résumer le contenu de la page active sur demande explicite.</summary>
        SummarizePage,

        /// <summary>Expliquer un passage sélectionné par l'utilisateur.</summary>
        ExplainSelection,

        /// <summary>Traduire un texte sélectionné.</summary>
        TranslateSelection,

        /// <summary>Rechercher une information dans la page active.</summary>
        SearchInPage,

        /// <summary>Comparer des informations issues d'onglets ouverts.</summary>
        CompareTabs,

        /// <summary>Synthétiser avec références vers passages pertinents.</summary>
        SynthesizeWithReferences,

        /// <summary>Rechercher dans toutes les pages, onglets et historique.</summary>
        UnifiedSearch
    }

    /// <summary>
    /// État d'une requête d'assistance. L'utilisateur doit toujours confirmer
    /// le contenu transmis avant tout envoi vers un service externe.
    /// </summary>
    public enum AssistantRequestState
    {
        /// <summary>Aucune requête en cours.</summary>
        Idle,

        /// <summary>En attente de confirmation utilisateur du contenu.</summary>
        AwaitingConfirmation,

        /// <summary>En cours de traitement par le service.</summary>
        Processing,

        /// <summary>Résultat disponible (local ou distant).</summary>
        Completed,

        /// <summary>Erreur ou service indisponible.</summary>
        Error
    }
}
