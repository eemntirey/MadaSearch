using System;

namespace MadagascarBrowser.Core.Navigation
{
    /// <summary>
    /// Commande applicative déclenchée par un raccourci clavier.
    /// </summary>
    public enum AcceleratorCommand
    {
        None,
        NewTab,
        CloseTab,
        FocusAddressBar,
        Reload,
        NextTab,
        PreviousTab,
        AddFavorite,
        ReopenClosedTab,
        FindInPage,
        Print,
        ExitFullScreen,
        OpenDevTools
    }

    /// <summary>
    /// Traduit une combinaison de touches en commande applicative.
    /// La règle vit dans le Core : l'interface et la page ne font que la relayer.
    /// </summary>
    public static class AcceleratorCommandResolver
    {
        /// <summary>
        /// Résout la commande correspondant à la combinaison de touches.
        /// </summary>
        /// <param name="key">Touche pressée (par exemple « t », « w », « l », « f5 »).</param>
        /// <param name="ctrl">Vrai si Ctrl est enfoncé.</param>
        /// <param name="alt">Vrai si Alt est enfoncé.</param>
        /// <param name="shift">Vrai si Maj est enfoncé.</param>
        public static AcceleratorCommand Resolve(string? key, bool ctrl, bool alt, bool shift)
        {
            if (string.IsNullOrWhiteSpace(key) || alt)
                return AcceleratorCommand.None;

            var normalized = key.Trim().ToLowerInvariant();

            // F5 recharge la page : Ctrl n'est pas nécessaire.
            if (normalized == "f5")
                return AcceleratorCommand.Reload;

            // Échap sort du plein écran : Ctrl n'est pas nécessaire.
            if (normalized == "escape")
                return AcceleratorCommand.ExitFullScreen;

            // F12 ouvre les outils de développement : Ctrl n'est pas nécessaire.
            // La décision d'exposition (Debug/Release) reste dans la couche Shell.
            if (normalized == "f12" && !ctrl && !alt)
                return AcceleratorCommand.OpenDevTools;

            if (!ctrl)
                return AcceleratorCommand.None;

            switch (normalized)
            {
                case "t":
                    return shift ? AcceleratorCommand.ReopenClosedTab : AcceleratorCommand.NewTab;
                case "w":
                    return AcceleratorCommand.CloseTab;
                case "l":
                    return AcceleratorCommand.FocusAddressBar;
                case "d":
                    return AcceleratorCommand.AddFavorite;
                case "f":
                    return AcceleratorCommand.FindInPage;
                case "p":
                    return AcceleratorCommand.Print;
                case "tab":
                    return shift ? AcceleratorCommand.PreviousTab : AcceleratorCommand.NextTab;
                default:
                    return AcceleratorCommand.None;
            }
        }
    }
}
