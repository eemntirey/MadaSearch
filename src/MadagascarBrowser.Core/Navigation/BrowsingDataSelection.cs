using System;

namespace MadagascarBrowser.Core.Navigation
{
    /// <summary>
    /// Catégories de données que l'utilisateur peut effacer.
    /// La traduction vers les API WebView2 reste dans la couche Shell :
    /// le Core ne dépend pas de WebView2.
    /// </summary>
    [Flags]
    public enum BrowsingDataKind
    {
        None = 0,
        Cookies = 1,
        Cache = 2,
        History = 4,
        LocalStorage = 8,
        Passwords = 16
    }

    public static class BrowsingDataSelection
    {
        /// <summary>Compose la sélection à partir des cases cochées.</summary>
        public static BrowsingDataKind Compose(
            bool cookies,
            bool cache,
            bool history,
            bool localStorage,
            bool passwords)
        {
            var selection = BrowsingDataKind.None;
            if (cookies) selection |= BrowsingDataKind.Cookies;
            if (cache) selection |= BrowsingDataKind.Cache;
            if (history) selection |= BrowsingDataKind.History;
            if (localStorage) selection |= BrowsingDataKind.LocalStorage;
            if (passwords) selection |= BrowsingDataKind.Passwords;
            return selection;
        }

        public static bool HasSelection(BrowsingDataKind selection)
        {
            return selection != BrowsingDataKind.None;
        }
    }
}
