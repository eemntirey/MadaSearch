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
        FocusAddressBar
    }

    /// <summary>
    /// Traduit une combinaison de touches en commande applicative.
    /// </summary>
    public static class AcceleratorCommandResolver
    {
        /// <summary>
        /// Résout la commande correspondant à la combinaison de touches.
        /// </summary>
        /// <param name="key">Touche pressée (par exemple « t », « w », « l »).</param>
        /// <param name="ctrl">Vrai si Ctrl est enfoncé.</param>
        /// <param name="alt">Vrai si Alt est enfoncé.</param>
        /// <param name="shift">Vrai si Maj est enfoncé.</param>
        public static AcceleratorCommand Resolve(string? key, bool ctrl, bool alt, bool shift)
        {
            if (string.IsNullOrWhiteSpace(key) || !ctrl || alt)
                return AcceleratorCommand.None;

            switch (key.Trim().ToLowerInvariant())
            {
                case "t":
                    return AcceleratorCommand.NewTab;
                case "w":
                    return AcceleratorCommand.CloseTab;
                case "l":
                    return AcceleratorCommand.FocusAddressBar;
                default:
                    return AcceleratorCommand.None;
            }
        }
    }
}
