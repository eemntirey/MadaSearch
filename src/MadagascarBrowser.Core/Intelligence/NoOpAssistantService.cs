using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MadagascarBrowser.Core.Intelligence
{
    /// <summary>
    /// Implémentation par défaut : aucun service d'assistance externe configuré.
    /// Ne produit jamais de résultats simulés comme s'ils provenaient d'une IA réelle.
    /// Toute demande retourne un message explicite indiquant l'absence de configuration.
    /// </summary>
    public class NoOpAssistantService : IAssistantService
    {
        public bool IsConfigured => false;
        public string? ConfiguredProviderName => null;

        private CancellationTokenSource? _pendingCts;

        public void CancelPendingRequest()
        {
            _pendingCts?.Cancel();
            _pendingCts?.Dispose();
            _pendingCts = null;
        }

        public async Task<AssistantState> RequestAssistanceAsync(
            AssistantRequestType requestType,
            string? explicitContent,
            List<string> relatedUrls,
            CancellationToken cancellationToken = default)
        {
            _pendingCts?.Cancel();
            _pendingCts?.Dispose();
            _pendingCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _pendingCts.Token);

            try
            {
                // Aucune transmission vers un service externe : pas de service configuré.
                await Task.Delay(50, linkedCts.Token); // simule un court délai d'annulation sûre
            }
            catch (TaskCanceledException)
            {
                // Annulation attendue.
            }
            finally
            {
                linkedCts.Dispose();
            }

            return new AssistantState
            {
                RequestType = requestType,
                State = AssistantRequestState.Error,
                Result = "Aucun service d'assistance configuré. " +
                         "Pour activer cette fonction, configurez un service dans les paramètres. " +
                         "Le navigateur ne transmet jamais automatiquement le contenu complet des pages.",
                ErrorMessage = "Service d'assistance non configuré. Aucune donnée n'a été transmise.",
                ContentExplicitlyConfirmed = explicitContent != null
            };
        }
    }
}
