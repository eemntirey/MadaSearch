using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Environnement WebView2 partagé par tous les onglets.
    /// Le dossier de profil utilisateur est forcé dans %LocalAppData%\MadagascarBrowser\WebView2
    /// (même racine que la base SQLite) : sans cela, WebView2 créait son profil par défaut
    /// à côté de l'exécutable, ce qui échouait en installation dans Program Files.
    /// </summary>
    internal static class WebViewEnvironment
    {
        /// <summary>
        /// Hôte virtuel de la page de démarrage.
        /// « .local » est un TLD réservé à mDNS : Chromium y tentait une résolution
        /// multicast qui expirait après ≈ 2 s avant de servir la page, ce qui rendait
        /// la page de démarrage lente à chaque rechargement. « .localhost » est
        /// également réservé (RFC 6761) et n'appelle aucune résolution : la page
        /// s'affiche en quelques dizaines de millisecondes.
        /// </summary>
        public const string LocalHost = "startpage.localhost";

        /// <summary>URL de la page d'accueil intégrée.</summary>
        public const string StartPageUrl = "https://" + LocalHost + "/index.html";

        /// <summary>URL de la page d'erreur du navigateur.</summary>
        public const string ErrorPageUrl = "https://" + LocalHost + "/error.html";

        /// <summary>
        /// Dossier de profil unique : jamais à côté de l'exécutable, jamais dans le
        /// profil par défaut de WebView2 (%LocalAppData%\Microsoft\EdgeWebView2\Application).
        /// </summary>
        public static string UserDataFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MadagascarBrowser",
            "WebView2");

        private static readonly object Sync = new();
        private static Task<CoreWebView2Environment>? _environment;

        /// <summary>
        /// Environnement unique, partagé par tous les onglets : un seul processus navigateur
        /// et un seul dossier de profil (WebView2 refuse deux environnements sur le même dossier).
        /// Un échec n'est pas mis en cache : la prochaine demande retente la création.
        /// </summary>
        public static Task<CoreWebView2Environment> GetAsync()
        {
            lock (Sync)
            {
                _environment ??= CreateAsync();
                return _environment;
            }
        }

        private static async Task<CoreWebView2Environment> CreateAsync()
        {
            try
            {
                if (!Directory.Exists(UserDataFolder))
                    Directory.CreateDirectory(UserDataFolder);
            }
            catch (Exception exception)
            {
                Trace.TraceError(
                    $"Impossible de créer le dossier de profil WebView2 '{UserDataFolder}' : {exception}");
            }

            try
            {
                return await CoreWebView2Environment.CreateAsync(null, UserDataFolder).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Trace.TraceError(
                    $"Création impossible de l'environnement WebView2 (profil '{UserDataFolder}') : {exception}");
                lock (Sync)
                {
                    // L'échec n'est pas conservé : onglets suivants et redémarrages retentent.
                    _environment = null;
                }
                throw;
            }
        }
    }
}
