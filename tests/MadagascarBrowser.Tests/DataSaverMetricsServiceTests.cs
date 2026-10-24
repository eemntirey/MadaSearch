using MadagascarBrowser.Core.Data;

namespace MadagascarBrowser.Tests;

public class DataSaverMetricsServiceTests
{
    [Fact]
    public void RecordImageReplacement_PersistsResponseContentLengthAndSessionMetrics()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"madasearch-metrics-{Guid.NewGuid():N}.db");
        try
        {
            var service = new DataSaverMetricsService(new Database(databasePath));
            service.RecordImageReplaced(256_000);
            service.RecordVideoBlocked(1_024_000);
            service.RecordAdBlocked();

            var snapshot = service.GetSnapshot();

            Assert.Equal(1_280_000, snapshot.SessionSavedBytes);
            Assert.Equal(1_280_000, snapshot.TotalSavedBytes);
            Assert.Equal(1, snapshot.SessionBlockedAds);
            Assert.Equal(1, snapshot.TotalBlockedAds);
            Assert.Equal(1, snapshot.SessionReplacedImages);
            Assert.Equal(1, snapshot.TotalReplacedImages);
            Assert.Equal(1, snapshot.SessionBlockedVideos);
            Assert.Equal(1, snapshot.TotalBlockedVideos);

            var nextSession = new DataSaverMetricsService(new Database(databasePath)).GetSnapshot();
            Assert.Equal(0, nextSession.SessionSavedBytes);
            Assert.Equal(1_280_000, nextSession.TotalSavedBytes);
            Assert.Equal(0, nextSession.SessionBlockedAds);
            Assert.Equal(1, nextSession.TotalBlockedAds);
        }
        finally
        {
            // Connexion SQLite poolée : le fichier peut rester verrouillé quelques
            // instants après la fermeture. Conformément aux autres tests, on ignore
            // l'échec du nettoyage d'un fichier temporaire.
            try { File.Delete(databasePath); } catch (IOException) { }
        }
    }
}
