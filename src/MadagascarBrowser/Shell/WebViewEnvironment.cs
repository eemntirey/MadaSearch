using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MadagascarBrowser.Core.Paths;
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

        /// <summary>Dossier de profil WebView2 de la navigation normale.</summary>
        public static string UserDataFolder { get; } = BrowserPaths.Default.WebViewProfilePath;

        private static Task<CoreWebView2Environment>? _environment;

        /// <summary>
        /// Environnement unique, partagé par tous les onglets : un seul processus navigateur
        /// et un seul dossier de profil (WebView2 refuse deux environnements sur le même dossier).
        /// </summary>
        public static Task<CoreWebView2Environment> GetAsync()
        {
            if (_environment == null)
            {
                Directory.CreateDirectory(UserDataFolder);
                WarnAboutDefaultProfileLeftovers();
                _environment = CoreWebView2Environment.CreateAsync(null, UserDataFolder);
            }
            return _environment;
        }

        /// <summary>
        /// Signale les profils WebView2 par défaut restés à côté de l'exécutable.
        /// Leur présence est sans danger pour l'utilisateur, mais elle indique que
        /// l'application tourne encore avec un dossier de profil non imposé.
        /// </summary>
        private static void WarnAboutDefaultProfileLeftovers()
        {
            try
            {
                var applicationName = Path.GetFileNameWithoutExtension(Environment.ProcessPath) ?? "MadagascarBrowser";
                var leftovers = BrowserPaths.FindDefaultProfileLeftovers(
                    AppContext.BaseDirectory,
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    applicationName);

                foreach (var leftover in leftovers)
                {
                    var message = $"Un profil WebView2 par défaut subsiste à l'emplacement « {leftover} ». " +
                                  "Le profil utilisé est « " + UserDataFolder + " ».";
                    Debug.WriteLine(message);
                    Trace.TraceWarning(message);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Impossible de vérifier les profils WebView2 par défaut : {exception}");
                Trace.TraceWarning($"Impossible de vérifier les profils WebView2 par défaut : {exception}");
            }
        }
    }
}
