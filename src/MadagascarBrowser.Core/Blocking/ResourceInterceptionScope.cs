using System;

namespace MadagascarBrowser.Core.Blocking
{
    /// <summary>
    /// Périmètre d'interception des requêtes réseau.
    /// Seules les ressources http(s) sont observées par le bloqueur de publicités et
    /// l'économiseur de données : les pages enregistrées hors ligne (file://) et les
    /// fichiers locaux ne sont jamais réécrits ni bloqués.
    /// </summary>
    public static class ResourceInterceptionScope
    {
        /// <summary>Motifs de filtre utilisés par WebView2 pour les ressources interceptées.</summary>
        public static readonly string[] UrlPatterns = { "http://*", "https://*" };

        /// <summary>
        /// Motifs de filtre utilisés par le protocole DevTools (Fetch) pour observer
        /// les réponses http(s) avant leur remise à la page.
        /// </summary>
        public static readonly string[] FetchPatterns = { "http://*/*", "https://*/*" };

        /// <summary>
        /// Indique si une ressource entre dans le périmètre d'interception.
        /// </summary>
        public static bool IsInterceptable(Uri? uri)
        {
            if (uri == null)
                return false;
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        /// <summary>
        /// Indique si une URL entre dans le périmètre d'interception.
        /// Une URL illisible ou hors http(s) (file://, about:blank) est ignorée.
        /// </summary>
        public static bool IsInterceptable(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
                return false;
            return IsInterceptable(uri);
        }
    }
}
