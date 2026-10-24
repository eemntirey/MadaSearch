using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;

namespace MadagascarBrowser.Core.Navigation
{
    public class NavigationService
    {
        private readonly string _searchUrlTemplate;

        public NavigationService(string? searchProvider = "google")
        {
            _searchUrlTemplate = SearchProviderResolver.GetSearchUrlTemplate(searchProvider);
        }

        public string ResolveNavigation(string input)
        {
            return ResolveNavigation(input, _searchUrlTemplate);
        }

        public string ResolveNavigation(string input, string searchUrlTemplate)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            input = input.Trim();
            if (IsAbsoluteUrl(input))
            {
                return input;
            }

            // Un chemin local absolu (« C:\...\page.mhtml ») est une cible de fichier :
            // NavigationTargetResolver le convertit en URI file://. Un sélecteur de
            // lecteur sans séparateur (« c: documents ») n'en est pas un : c'est une
            // recherche, sinon la saisie deviendrait un fichier introuvable.
            if (LooksLikeRootedPath(input))
            {
                return input;
            }

            if (LooksLikeUrl(input))
            {
                return "https://" + input;
            }

            var encoded = UrlEncoder.Default.Encode(input);
            return (string.IsNullOrWhiteSpace(searchUrlTemplate) ? _searchUrlTemplate : searchUrlTemplate) + encoded;
        }

        public bool LooksLikeUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var s = input.Trim();

            // Une chaîne contenant un espace est une recherche, jamais une URL :
            // « python 3.11 » ne doit pas être interprété comme le domaine « python 3.11 ».
            if (s.Any(char.IsWhiteSpace))
                return false;

            if (IsAbsoluteUrl(s))
                return true;

            if (s.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
                return true;
            if (System.Net.IPAddress.TryParse(s, out _))
                return true;

            if (s.Contains('.'))
            {
                var parts = s.Split('.');
                if (parts.Length >= 2)
                {
                    var tld = parts[parts.Length - 1];
                    if (tld.Length >= 2 && tld.Length <= 6)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool LooksLikeRootedPath(string s)
        {
            // Un chemin absolu Windows (« C:\... » ou « C:/... ») est une page locale,
            // jamais une recherche. Un simple lecteur (« c: bonjour ») reste une
            // recherche : sans séparateur, il n'y a pas de chemin.
            return s.IndexOfAny(new[] { '\\', '/' }) >= 0
                && Path.IsPathRooted(s);
        }

        /// <summary>
        /// Indique si la saisie porte déjà un schéma d'URL qui doit être conservé tel quel.
        /// Les pages hors ligne (file://) et les onglets vides (about:) seraient
        /// sinon transformés en « https://file://… », donc inaccessibles.
        /// </summary>
        private static bool IsAbsoluteUrl(string s)
        {
            return s.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("file://", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("about:", StringComparison.OrdinalIgnoreCase);
        }
    }
}
