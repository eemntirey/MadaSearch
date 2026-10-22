using System;
using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Shell
{
    /// <summary>Issue du traitement d'une demande d'ouverture de fenêtre.</summary>
    public enum NewWindowOutcome
    {
        /// <summary>Un nouvel onglet a été ouvert sur l'URL demandée.</summary>
        OpenedInNewTab,

        /// <summary>Un onglet vide a été ouvert, à raccrocher à la page appelante.</summary>
        BlankTabOpened,

        /// <summary>La demande n'a pas pu être traitée.</summary>
        Failed
    }

    /// <summary>
    /// Résultat du traitement d'une demande d'ouverture de fenêtre.
    /// </summary>
    /// <param name="Outcome">Issue de la demande.</param>
    /// <param name="Tab">Onglet créé, lorsqu'il y en a un.</param>
    /// <param name="Error">Message d'erreur en cas d'échec.</param>
    public sealed record NewWindowRequestResult(NewWindowOutcome Outcome, TabViewModel? Tab, string? Error)
    {
        /// <summary>La demande a-t-elle abouti ?</summary>
        public bool Succeeded => Outcome != NewWindowOutcome.Failed;
    }

    /// <summary>
    /// Traite une demande d'ouverture de fenêtre (window.open, target="_blank").
    /// La règle est ici, sans WebView2 : elle est donc vérifiable par des tests.
    /// </summary>
    public sealed class NewWindowRequestHandler
    {
        private readonly ITabHost _tabHost;

        public NewWindowRequestHandler(ITabHost tabHost)
        {
            _tabHost = tabHost ?? throw new ArgumentNullException(nameof(tabHost));
        }

        /// <summary>
        /// Ouvre l'URI demandée dans un nouvel onglet.
        /// Une URI vide (« about:blank ») donne un onglet vide que la page appelante
        /// pourra piloter via l'API WebView2.
        /// </summary>
        /// <param name="uri">URI demandée par la page.</param>
        public NewWindowRequestResult Handle(string? uri)
        {
            try
            {
                if (NewWindowRequestResolver.IsBlank(uri))
                {
                    var blankTab = _tabHost.NewTab(NewWindowRequestResolver.BlankUrl);
                    return new NewWindowRequestResult(NewWindowOutcome.BlankTabOpened, blankTab, null);
                }

                var target = NewWindowRequestResolver.Resolve(uri);
                var tab = _tabHost.NewTab(target);
                return new NewWindowRequestResult(NewWindowOutcome.OpenedInNewTab, tab, null);
            }
            catch (Exception exception)
            {
                // L'échec est signalé, jamais avalé : la page appelante perd la main
                // mais l'utilisateur et le journal savent ce qui s'est passé.
                return new NewWindowRequestResult(NewWindowOutcome.Failed, null, exception.Message);
            }
        }
    }
}
