using Microsoft.Data.Sqlite;

namespace MadagascarBrowser.Core.Data
{
    /// <summary>
    /// Ouverture centralisée des connexions SQLite.
    /// Le délai d'attente par défaut évite les « database is locked » : quand une
    /// autre écriture est en cours, SQLite patiente jusqu'à ce délai au lieu
    /// d'échouer aussitôt. La chaîne de connexion reste inchangée afin de conserver
    /// le regroupement des connexions (pool) utilisé par les tests et l'application.
    /// </summary>
    public static class SqliteConnectionFactory
    {
        /// <summary>Délai d'attente maximal d'un verrou, en millisecondes.</summary>
        public const int BusyTimeoutMilliseconds = 5000;

        /// <summary>
        /// Ouvre une connexion sur le fichier de base indiqué.
        /// </summary>
        /// <param name="path">Chemin du fichier SQLite.</param>
        public static SqliteConnection Open(string path)
        {
            var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA busy_timeout = {BusyTimeoutMilliseconds};";
            command.ExecuteScalar();
            return connection;
        }
    }
}
