using System;
using System.IO;

namespace MadagascarBrowser.Core.Navigation
{
    /// <summary>
    /// Nature de la cible d'une navigation.
    /// </summary>
    public enum NavigationTargetKind
    {
        /// <summary>Cible vide ou inconnue.</summary>
        None,

        /// <summary>Onglet vide (about:blank).</summary>
        Blank,

        /// <summary>Page Web (http ou https).</summary>
        Web,

        /// <summary>Fichier local (page hors ligne au format MHTML).</summary>
        LocalFile
    }

    /// <summary>
    /// Détermine la cible d'une navigation (page Web, fichier local ou onglet vide) ainsi que
    /// le titre à afficher pour l'onglet correspondant.
    /// </summary>
    public static class NavigationTargetResolver
    {
        /// <summary>
        /// Résout la cible d'une navigation.
        /// </summary>
        /// <param name="url">URL ou chemin local demandé.</param>
        /// <param name="target">URL absolue à navigated (vide si <paramref name="kind"/> vaut None).</param>
        /// <param name="title">Titre à afficher pour l'onglet (jamais vide si la cible est résolue).</param>
        /// <param name="kind">Nature de la cible.</param>
        public static NavigationTargetKind Resolve(string? url, out string target, out string title)
        {
            target = string.Empty;
            title = string.Empty;

            if (string.IsNullOrWhiteSpace(url))
                return NavigationTargetKind.None;

            var trimmed = url.Trim();

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                if (uri.IsFile)
                {
                    target = uri.AbsoluteUri;
                    title = FileNameOf(uri.LocalPath);
                    return NavigationTargetKind.LocalFile;
                }

                if (uri.Scheme.Equals("about", StringComparison.OrdinalIgnoreCase))
                {
                    target = NewWindowRequestResolver.BlankUrl;
                    title = "Nouvel onglet";
                    return NavigationTargetKind.Blank;
                }

                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                    return NavigationTargetKind.None;

                target = uri.AbsoluteUri;
                title = string.IsNullOrEmpty(uri.Host) ? trimmed : uri.Host;
                return NavigationTargetKind.Web;
            }

            // Chemin local brut (par exemple le fichier MHTML d'une page hors ligne).
            if (Path.IsPathRooted(trimmed))
            {
                var full = Path.GetFullPath(trimmed);
                target = new Uri(full).AbsoluteUri;
                title = FileNameOf(full);
                return NavigationTargetKind.LocalFile;
            }

            return NavigationTargetKind.None;
        }

        private static string FileNameOf(string path)
        {
            var name = Path.GetFileName(path);
            return string.IsNullOrWhiteSpace(name) ? "Fichier local" : name;
        }
    }
}
