using MadagascarBrowser.Core.Data;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Threading;

namespace MadagascarBrowser.Tests;

public class SqliteRobustnessTests
{
    private static string CreateTemporaryDatabase()
    {
        var path = Path.Combine(Path.GetTempPath(), "mb-sqlite-" + Guid.NewGuid().ToString("N") + ".db");
        new Database(path);
        return path;
    }

    /// <summary>
    /// Supprime la base et ses fichiers annexes (journal WAL), après avoir vidé
    /// le groupe de connexions qui garderait le fichier ouvert.
    /// </summary>
    private static void DeleteDatabase(string path)
    {
        using (var connection = new SqliteConnection($"Data Source={path}"))
        {
            SqliteConnection.ClearPool(connection);
        }

        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            var file = path + suffix;
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (IOException)
            {
                // Le fichier reste ouvert : le systeme le supprimera a la fermeture.
            }
        }
    }

    [Fact]
    public void IsTransient_ReconaitLesVerrousSqlite()
    {
        Assert.True(SqliteRetryPolicy.IsTransient(new SqliteException("verrou", SqliteRetryPolicy.BusyErrorCode)));
        Assert.True(SqliteRetryPolicy.IsTransient(new SqliteException("verrou", SqliteRetryPolicy.LockedErrorCode)));
    }

    [Fact]
    public void IsTransient_IgnoreLesAutresErreurs()
    {
        Assert.False(SqliteRetryPolicy.IsTransient(new SqliteException("autre", 19)));
    }

    [Fact]
    public void Execute_ReessaieQuandLaBaseEstVerrouillee()
    {
        var attempts = 0;

        var result = SqliteRetryPolicy.Execute(
            () =>
            {
                attempts++;
                if (attempts < 3)
                    throw new SqliteException("verrou", SqliteRetryPolicy.BusyErrorCode);
                return "ok";
            },
            maxAttempts: 5,
            initialDelay: TimeSpan.FromMilliseconds(1));

        Assert.Equal("ok", result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public void Execute_AbandonneApresLeNombreDeTentatives()
    {
        var attempts = 0;

        Assert.Throws<SqliteException>(() => SqliteRetryPolicy.Execute(
            () =>
            {
                attempts++;
                throw new SqliteException("verrou", SqliteRetryPolicy.BusyErrorCode);
            },
            maxAttempts: 3,
            initialDelay: TimeSpan.FromMilliseconds(1)));

        Assert.Equal(3, attempts);
    }

    [Fact]
    public void Execute_UneErreurNonTransitoireNEstPasReessayee()
    {
        var attempts = 0;

        Assert.Throws<SqliteException>(() => SqliteRetryPolicy.Execute(
            () =>
            {
                attempts++;
                throw new SqliteException("autre", 19);
            },
            maxAttempts: 5,
            initialDelay: TimeSpan.FromMilliseconds(1)));

        Assert.Equal(1, attempts);
    }

    [Fact]
    public void Connexion_DefinitUnDelaiDAttenteDeVerrou()
    {
        var path = CreateTemporaryDatabase();
        try
        {
            using var connection = SqliteConnectionFactory.Open(path);
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA busy_timeout;";

            Assert.Equal(SqliteConnectionFactory.BusyTimeoutMilliseconds, Convert.ToInt64(command.ExecuteScalar()));
        }
        finally
        {
            DeleteDatabase(path);
        }
    }

    [Fact]
    public void EcrituresSimultanees_ReussissentSansErreurDeVerrou()
    {
        // Usage réel : l'historique, les métriques et les téléchargements écrivent
        // en même temps depuis plusieurs threads. Aucune « database is locked ».
        var path = CreateTemporaryDatabase();
        try
        {
            var history = new HistoryService(new Database(path));
            var errors = new List<Exception>();
            var threads = new List<Thread>();

            for (var writer = 0; writer < 8; writer++)
            {
                var index = writer;
                var thread = new Thread(() =>
                {
                    try
                    {
                        for (var visit = 0; visit < 20; visit++)
                            history.AddVisit($"https://site{index}.mg/{visit}", "Titre");
                    }
                    catch (Exception exception)
                    {
                        lock (errors) errors.Add(exception);
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads)
                Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "Une écriture a expiré.");

            Assert.Empty(errors);
            Assert.Equal(160, history.Recent(500).Count);
        }
        finally
        {
            DeleteDatabase(path);
        }
    }
}
