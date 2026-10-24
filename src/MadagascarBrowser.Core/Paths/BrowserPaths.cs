using System;
using System.Collections.Generic;
using System.IO;

namespace MadagascarBrowser.Core.Paths
{
    /// <summary>
    /// Emplacement de toutes les données écrites par Madagascar Browser.
    /// Tout est regroupé sous %LocalAppData%\MadagascarBrowser : l'application
    /// reste ainsi utilisable depuis un dossier d'installation non inscriptible
    /// (Program Files) et ne laisse aucun profil WebView2 par défaut à côté
    /// de l'exécutable.
    /// </summary>
    public sealed class BrowserPaths
    {
        /// <summary>Nom du dossier applicatif sous %LocalAppData%.</summary>
        public const string ApplicationFolderName = "MadagascarBrowser";

        /// <summary>Nom du dossier de profil WebView2 (navigation normale).</summary>
        public const string WebViewProfileFolderName = "WebView2";

        /// <summary>Nom du dossier de profil WebView2 (navigation privée).</summary>
        public const string PrivateWebViewProfileFolderName = "WebView2-Private";

        /// <summary>Nom du dossier des journaux.</summary>
        public const string LogsFolderName = "logs";

        /// <summary>Nom du fichier journal principal.</summary>
        public const string LogFileName = "app.log";

        /// <summary>Nom du fichier de base de données.</summary>
        public const string DatabaseFileName = "browser.db";

        /// <summary>Nom du dossier des pages hors ligne.</summary>
        public const string OfflineFolderName = "offline";

        private BrowserPaths(string root)
        {
            Root = Path.GetFullPath(root);
            DatabasePath = Path.Combine(Root, DatabaseFileName);
            WebViewProfilePath = Path.Combine(Root, WebViewProfileFolderName);
            PrivateWebViewProfilePath = Path.Combine(Root, PrivateWebViewProfileFolderName);
            LogsDirectory = Path.Combine(Root, LogsFolderName);
            LogFilePath = Path.Combine(LogsDirectory, LogFileName);
            OfflineDirectory = Path.Combine(Root, OfflineFolderName);
        }

        /// <summary>Racine de toutes les données de l'application.</summary>
        public string Root { get; }

        /// <summary>Base SQLite de l'utilisateur.</summary>
        public string DatabasePath { get; }

        /// <summary>Dossier de profil WebView2 de la navigation normale.</summary>
        public string WebViewProfilePath { get; }

        /// <summary>Dossier de profil WebView2 de la navigation privée.</summary>
        public string PrivateWebViewProfilePath { get; }

        /// <summary>Dossier des journaux.</summary>
        public string LogsDirectory { get; }

        /// <summary>Fichier journal principal.</summary>
        public string LogFilePath { get; }

        /// <summary>Dossier des pages enregistrées hors ligne.</summary>
        public string OfflineDirectory { get; }

        /// <summary>Emplacements utilisés par l'utilisateur courant.</summary>
        public static BrowserPaths Default { get; } =
            new(ComposeRoot(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)));

        /// <summary>Construit un ensemble de chemins à partir d'une racine donnée.</summary>
        public static BrowserPaths ForRoot(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
                throw new ArgumentException("La racine des données ne peut pas être vide.", nameof(root));
            return new BrowserPaths(root);
        }

        /// <summary>Compose la racine applicative sous un dossier %LocalAppData%.</summary>
        public static string ComposeRoot(string localApplicationData)
        {
            return Path.Combine(localApplicationData, ApplicationFolderName);
        }

        /// <summary>
        /// Dossiers de profil WebView2 par défaut que WebView2 créerait si l'application
        /// n'imposait pas de dossier utilisateur. Ils ne doivent pas subsister : leur
        /// présence signale une version antérieure de Madagascar Browser ou une
        /// installation qui n'utilisait pas l'emplacement imposé.
        /// </summary>
        /// <param name="executableDirectory">Dossier contenant l'exécutable.</param>
        /// <param name="localApplicationData">Dossier %LocalAppData% de l'utilisateur.</param>
        /// <param name="applicationName">Nom de l'application (nom du fichier .exe).</param>
        public static IReadOnlyList<string> FindDefaultProfileLeftovers(
            string executableDirectory,
            string localApplicationData,
            string applicationName)
        {
            var leftovers = new List<string>();
            if (string.IsNullOrWhiteSpace(executableDirectory) || string.IsNullOrWhiteSpace(applicationName))
                return leftovers;

            var candidates = new[]
            {
                Path.Combine(executableDirectory, applicationName + ".WebView2"),
                Path.Combine(executableDirectory, "EBWebView"),
                Path.Combine(localApplicationData, applicationName + ".WebView2")
            };

            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate))
                    leftovers.Add(candidate);
            }

            return leftovers;
        }
    }
}
