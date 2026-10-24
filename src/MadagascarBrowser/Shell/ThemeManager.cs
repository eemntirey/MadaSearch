using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using MadagascarBrowser.Core.Data;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Applique à l'exécutant les préférences d'apparence (thème clair/sombre,
    /// accent, densité). L'accent est appliqué en modifiant l'instance de brush
    /// partagée par les jetons : toutes les vues qui la consomment en
    /// DynamicResource se repeignent immédiatement, sans rechargement de la
    /// fenêtre.
    /// </summary>
    public static class ThemeManager
    {
        public const string MalagasyAccent = "#FF4438";
        public const string GreenAccent = "#19B36B";
        public const string OrangeAccent = "#F0A63C";

        /// <summary>Thème clair.</summary>
        public const string ThemeLight = "light";

        /// <summary>Thème sombre (défaut).</summary>
        public const string ThemeDark = "dark";

        /// <summary>
        /// Applique l'ensemble des préférences d'apparence.
        /// </summary>
        public static void Apply(BrowserSettings settings)
        {
            if (settings == null) return;
            ApplyTheme(settings.Theme);
            ApplyAccent(settings.Accent);
            ApplyDensity(settings.Density);
        }

        /// <summary>
        /// Change le thème clair/sombre de l'application en remplaçant le
        /// dictionnaire de ressources fusionné dans Application.Current.
        /// </summary>
        public static void ApplyTheme(string? theme)
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;

            var isLight = string.Equals(theme, ThemeLight, StringComparison.OrdinalIgnoreCase);

            var toRemove = new List<ResourceDictionary>();
            foreach (var rd in resources.MergedDictionaries)
            {
                var source = rd.Source?.ToString();
                if (source != null &&
                    (source.Contains("LightTokens", StringComparison.OrdinalIgnoreCase) ||
                     source.Contains("DarkTokens", StringComparison.OrdinalIgnoreCase)))
                {
                    toRemove.Add(rd);
                }
            }
            foreach (var rd in toRemove)
                resources.MergedDictionaries.Remove(rd);

            var themeSource = isLight
                ? "Themes/LightTokens.xaml"
                : "Themes/DarkTokens.xaml";
            resources.MergedDictionaries.Add(
                new ResourceDictionary { Source = new Uri(themeSource, UriKind.Relative) });
        }

        /// <summary>
        /// Change la couleur d'accent de l'interface.
        /// </summary>
        public static void ApplyAccent(string? accent)
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;

            var color = accent switch
            {
                "vert" => GreenAccent,
                "orange" => OrangeAccent,
                _ => MalagasyAccent
            };

            if (resources["AccentBrush"] is SolidColorBrush brush && !brush.IsFrozen)
            {
                brush.Color = (Color)ColorConverter.ConvertFromString(color);
                return;
            }

            resources["AccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        /// <summary>
        /// Change la densité de l'interface : « compact » resserre les marges des vues internes.
        /// </summary>
        public static void ApplyDensity(string? density)
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;

            var compact = density == "compact";
            resources["ViewPadding"] = compact ? new Thickness(18, 16, 18, 16) : new Thickness(28, 24, 28, 24);
            resources["CardPadding"] = compact ? new Thickness(12) : new Thickness(20);
            resources["CardMargin"] = compact ? new Thickness(0, 0, 0, 10) : new Thickness(0, 0, 0, 16);
        }
    }
}
