using System;
using System.IO;
using MadagascarBrowser.Core.Navigation;
using Microsoft.Web.WebView2.Core;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Lit l'état réel du runtime WebView2. Aucun remplacement du moteur :
    /// on interroge l'API Microsoft et on signale ce qu'elle renvoie.
    /// </summary>
    internal static class WebViewRuntimeService
    {
        private const string FixedRuntimeFolderPrefix = "Microsoft.WebView2.FixedVersionRuntime";

        public static WebViewRuntimeInfo GetRuntimeInfo()
        {
            var isFixed = IsFixedVersionDeployment();
            string? version = null;
            try
            {
                // Paramètre null : runtime installé (Evergreen) ou dossier figé
                // découvert par le loader WebView2 à côté de l'exécutable.
                version = CoreWebView2Environment.GetAvailableBrowserVersionString(null!);
            }
            catch (Exception exception)
            {
                AppLog.Warning("Version du runtime WebView2 indisponible.", exception);
            }

            return WebViewRuntimeInfo.Detect(version, isFixed);
        }

        /// <summary>
        /// Détecte un déploiement à version figée : dossier fixe fourni par
        /// l'utilisateur ou runtime copié à côté de l'exécutable.
        /// </summary>
        public static bool IsFixedVersionDeployment()
        {
            var configuredFolder = Environment.GetEnvironmentVariable("WEBVIEW2_BROWSER_EXECUTABLE_FOLDER");
            if (!string.IsNullOrWhiteSpace(configuredFolder))
                return true;

            foreach (var unused in EnumerateFixedRuntimeFolders())
                return true;

            return false;
        }

        private static string[] EnumerateFixedRuntimeFolders()
        {
            try
            {
                return Directory.GetDirectories(
                    AppContext.BaseDirectory,
                    FixedRuntimeFolderPrefix + "*",
                    SearchOption.TopDirectoryOnly);
            }
            catch (Exception exception)
            {
                AppLog.Warning("Recherche d'un runtime WebView2 figé impossible.", exception);
                return Array.Empty<string>();
            }
        }
    }
}
