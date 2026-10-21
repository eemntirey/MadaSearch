using System;
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
        public static string UserDataFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MadagascarBrowser",
            "WebView2");

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
                _environment = CoreWebView2Environment.CreateAsync(null, UserDataFolder);
            }
            return _environment;
        }
    }
}
