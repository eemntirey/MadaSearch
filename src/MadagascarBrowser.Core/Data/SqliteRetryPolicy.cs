using Microsoft.Data.Sqlite;
using System;
using System.Threading;

namespace MadagascarBrowser.Core.Data
{
    /// <summary>
    /// Nouvelle tentative d'écriture SQLite lorsque la base est momentanément verrouillée.
    /// L'écriture reste hors du thread de l'interface ; cette politique ne fait que
    /// absorber les conflits brefs entre écritures concurrentes.
    /// </summary>
    public static class SqliteRetryPolicy
    {
        /// <summary>Code SQLite « SQLITE_BUSY ».</summary>
        public const int BusyErrorCode = 5;

        /// <summary>Code SQLite « SQLITE_LOCKED ».</summary>
        public const int LockedErrorCode = 6;

        /// <summary>Délai de la première reprise.</summary>
        public static readonly TimeSpan DefaultInitialDelay = TimeSpan.FromMilliseconds(40);

        /// <summary>
        /// Indique si l'erreur mérite une nouvelle tentative.
        /// </summary>
        public static bool IsTransient(SqliteException exception)
        {
            return exception.SqliteErrorCode == BusyErrorCode
                || exception.SqliteErrorCode == LockedErrorCode;
        }

        /// <summary>
        /// Exécute une action et réessaie en cas de verrou temporaire.
        /// </summary>
        /// <param name="action">Action à exécuter.</param>
        /// <param name="maxAttempts">Nombre maximum de tentatives (1 = aucun nouvel essai).</param>
        /// <param name="initialDelay">Délai avant la première reprise.</param>
        public static void Execute(Action action, int maxAttempts = 4, TimeSpan? initialDelay = null)
        {
            Execute<object?>(() =>
            {
                action();
                return null;
            }, maxAttempts, initialDelay);
        }

        /// <summary>
        /// Exécute une action et renvoie son résultat, en réessayant en cas de verrou temporaire.
        /// </summary>
        /// <typeparam name="T">Type du résultat.</typeparam>
        /// <param name="action">Action à exécuter.</param>
        /// <param name="maxAttempts">Nombre maximum de tentatives (1 = aucun nouvel essai).</param>
        /// <param name="initialDelay">Délai avant la première reprise.</param>
        public static T Execute<T>(Func<T> action, int maxAttempts = 4, TimeSpan? initialDelay = null)
        {
            ArgumentNullException.ThrowIfNull(action);
            var attempts = Math.Max(1, maxAttempts);
            var delay = initialDelay ?? DefaultInitialDelay;

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return action();
                }
                catch (SqliteException exception) when (IsTransient(exception) && attempt < attempts)
                {
                    Thread.Sleep(delay);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                }
            }
        }
    }
}
