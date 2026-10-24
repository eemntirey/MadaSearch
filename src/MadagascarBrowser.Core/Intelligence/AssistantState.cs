namespace MadagascarBrowser.Core.Intelligence
{
    /// <summary>
    /// Représente l'état actuel d'une interaction d'assistance.
    /// Ne contient jamais automatiquement l'historique, les cookies ou le contenu
    /// de pages privées : tout contenu transmis doit être explicite et limité.
    /// </summary>
    public class AssistantState
    {
        public AssistantRequestType RequestType { get; set; }
        public AssistantRequestState State { get; set; } = AssistantRequestState.Idle;

        /// <summary>Contenu explicite sélectionné par l'utilisateur (jamais le document complet).</summary>
        public string? ExplicitContent { get; set; }

        /// <summary>Liste des URLs des onglets ouverts concernés par la demande.</summary>
        public System.Collections.Generic.List<string> RelatedTabUrls { get; } = new();

        /// <summary>Résultat produit par le service (si configuré) ou message d'information.</summary>
        public string? Result { get; set; }

        /// <summary>Références vers passages pertinents lorsque disponibles.</summary>
        public System.Collections.Generic.List<string> References { get; } = new();

        /// <summary>Message d'erreur clair lorsque le service est indisponible.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Message explicite signalant que le contenu n'a pas été transmis.</summary>
        public bool ContentExplicitlyConfirmed { get; set; }
    }
}
