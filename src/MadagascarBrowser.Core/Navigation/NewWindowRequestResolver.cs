using System;

namespace MadagascarBrowser.Core.Navigation
{
    /// <summary>
    /// Détermine l'URL à ouvrir dans un nouvel onglet lorsqu'une page demande
    /// l'ouverture d'une fenêtre (window.open).
    /// </summary>
    public static class NewWindowRequestResolver
    {
        /// <summary>URL d'un onglet vide.</summary>
        public const string BlankUrl = "about:blank";

        /// <summary>
        /// Indique si la demande correspond à une fenêtre vide (window.open sans URL).
        /// WebView2 signale ce cas avec une URI vide ou « about:blank ».
        /// </summary>
        /// <param name="uri">URI demandée par la page.</param>
        public static bool IsBlank(string? uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return true;
            return uri.Trim().Equals(BlankUrl, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Résout l'URL cible d'une demande de nouvelle fenêtre.
        /// Une URI vide (window.open sans URL) donne un onglet vide : la page
        /// appelante reste ensuite responsable de son contenu via l'API WebView2.
        /// </summary>
        /// <param name="uri">URI demandée par la page, éventuellement vide.</param>
        public static string Resolve(string? uri)
        {
            if (IsBlank(uri))
                return BlankUrl;

            return uri!.Trim();
        }
    }
}
