using System;
using System.Threading.Tasks;
using MadagascarBrowser.Core.Data;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Exécute les accès SQLite hors du thread UI.
    /// Chaque service ouvre sa propre connexion, les opérations peuvent donc
    /// se dérouler en parallèle sans partager d'état. Une écriture refusée
    /// parce que la base est verrouillée est réessayée : l'utilisateur ne
    /// doit jamais voir « database is locked ».
    /// </summary>
    public static class DatabaseAccess
    {
        /// <summary>
        /// Exécute une lecture SQLite sur un thread de fond.
        /// </summary>
        public static Task<T> ReadAsync<T>(Func<T> read)
        {
            return Task.Run(() => SqliteRetryPolicy.Execute(read));
        }

        /// <summary>
        /// Exécute une écriture SQLite sur un thread de fond.
        /// </summary>
        public static Task WriteAsync(Action write)
        {
            return Task.Run(() => SqliteRetryPolicy.Execute(write));
        }

        /// <summary>
        /// Exécute une écriture SQLite sur un thread de fond et renvoie le résultat.
        /// </summary>
        public static Task<T> WriteAsync<T>(Func<T> write)
        {
            return Task.Run(() => SqliteRetryPolicy.Execute(write));
        }
    }
}
