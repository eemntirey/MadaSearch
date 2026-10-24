using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MadagascarBrowser.Core.Intelligence
{
    /// <summary>
    /// Abstraction d'un service d'assistance IA. Aucune dépendance à un fournisseur
    /// unique. Par défaut, le navigateur ne transmet aucun contenu de page vers
    /// un service externe sans confirmation explicite de l'utilisateur.
    /// </summary>
    public interface IAssistantService
    {
        /// <summary>
        /// Vrai si un service d'assistance est configuré et utilisable.
        /// Si faux, toutes les requêtes retournent un message indiquant
        /// clairement que le service n'est pas configuré.
        /// </summary>
        bool IsConfigured { get; }

        /// <summary>
        /// Nom du service configuré (pour information), jamais un identifiant
        /// de compte ou un secret.
        /// </summary>
        string? ConfiguredProviderName { get; }

        /// <summary>
        /// Initie une requête d'assistance. L'utilisateur doit avoir confirmé
        /// explicitement le contenu à transmettre (ExplicitContent).
        /// </summary>
        /// <param name="requestType">Type de demande.</param>
        /// <param name="explicitContent">Contenu explicitement sélectionné (jamais le document complet par défaut).</param>
        /// <param name="relatedUrls">URLs des onglets concernés.</param>
        /// <param name="cancellationToken">Annulation.</param>
        /// <returns>Résultat avec message explicite sur la source.</returns>
        Task<AssistantState> RequestAssistanceAsync(
            AssistantRequestType requestType,
            string? explicitContent,
            List<string> relatedUrls,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Annule une requête en cours de manière sûre.
        /// </summary>
        void CancelPendingRequest();
    }
}
