using System;
using System.Globalization;

namespace MadagascarBrowser.Core.Data
{
    /// <summary>
    /// Format de date commun à tous les services de données : lecture et écriture
    /// utilisent la même culture et les mêmes options, afin que l'historique,
    /// les favoris, les téléchargements et les pages hors ligne se relisent
    /// de la même façon.
    /// </summary>
    public static class DateTimeStorage
    {
        /// <summary>Format ISO 8601 arrondi, utilisé pour l'écriture en base.</summary>
        public const string Format = "o";

        /// <summary>
        /// Convertit une date en sa représentation stockée en base.
        /// </summary>
        public static string ToStorage(DateTime value)
        {
            return value.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Relit une date stockée en base.
        /// </summary>
        public static DateTime Parse(string value)
        {
            return DateTime.Parse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }
    }
}
