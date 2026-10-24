using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
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

        /// <summary>URL de la page d'accueil intégrée.</summary>
        public const string StartPageUrl = "https://" + LocalHost + "/index.html";

        /// <summary>URL de la page d'erreur du navigateur.</summary>
        public const string ErrorPageUrl = "https://" + LocalHost + "/error.html";

        /// <summary>
        /// Dossier de profil unique : jamais à côté de l'exécutable, jamais dans le
        /// profil par défaut de WebView2 (%LocalAppData%\Microsoft\EdgeWebView2\Application).
        /// L'emplacement est celui de BrowserPaths : une seule source de vérité
        /// pour les données écrites par le navigateur.
        /// </summary>
        public static string UserDataFolder { get; } = BrowserPaths.Default.WebViewProfilePath;

        private static readonly ConcurrentDictionary<string, Lazy<Task<CoreWebView2Environment>>> Environments =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Environnement unique, partagé par tous les onglets : un seul processus navigateur
        /// et un seul dossier de profil (WebView2 refuse deux environnements sur le même dossier).
        /// Un échec n'est pas mis en cache : la prochaine demande retente la création.
        /// </summary>
        /// <summary>
        /// Options du contrôleur. En navigation privée, WebView2 crée un profil
        /// en mémoire pour ce contrôleur uniquement : cookies et cache ne
        /// touchent pas le profil normal et disparaissent à la fermeture.
        /// </summary>
        public static CoreWebView2ControllerOptions CreateControllerOptions(
            CoreWebView2Environment environment,
            bool isPrivate)
        {
            var options = environment.CreateCoreWebView2ControllerOptions();
            options.IsInPrivateModeEnabled = isPrivate;
            return options;
        }

        public static Task<CoreWebView2Environment> GetAsync(string? userDataFolder = null)
        {
            var folder = string.IsNullOrWhiteSpace(userDataFolder) ? UserDataFolder : Path.GetFullPath(userDataFolder);
            return Environments.GetOrAdd(folder, path => new Lazy<Task<CoreWebView2Environment>>(
                () => CreateAsync(path), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        private static async Task<CoreWebView2Environment> CreateAsync(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
                if (string.Equals(folder, UserDataFolder, StringComparison.OrdinalIgnoreCase))
                    WarnAboutDefaultProfileLeftovers();
                return await CoreWebView2Environment.CreateAsync(null, folder).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Environments.TryRemove(folder, out _); // failed creation is retriable
                Trace.TraceError($"Création impossible de l'environnement WebView2 (profil '{folder}') : {exception.GetType().Name}");
                throw;
            }
        }

        public static void Forget(string userDataFolder)
        {
            Environments.TryRemove(userDataFolder, out _);
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
