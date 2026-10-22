using System;
using System.Globalization;

namespace MadagascarBrowser.Core.Navigation
{
    /// <summary>
    /// Mise en forme du niveau de zoom affiché à l'utilisateur.
    /// </summary>
    public static class ZoomLevel
    {
        /// <summary>
        /// Convertit un facteur de zoom WebView2 en pourcentage lisible (« 110 % »).
        /// </summary>
        /// <param name="factor">Facteur de zoom (1 = 100 %).</param>
        public static string Format(double factor)
        {
            if (double.IsNaN(factor) || double.IsInfinity(factor) || factor <= 0)
                return "100 %";

            var percent = (int)Math.Round(factor * 100, MidpointRounding.AwayFromZero);
            return percent.ToString(CultureInfo.InvariantCulture) + " %";
        }

        /// <summary>
        /// Ajuste un facteur de zoom en l'écartant de 100 % (clic sur l'indicateur).
        /// </summary>
        /// <param name="factor">Facteur de zoom courant.</param>
        public static double Reset(double factor)
        {
            return 1.0;
        }
    }
}
