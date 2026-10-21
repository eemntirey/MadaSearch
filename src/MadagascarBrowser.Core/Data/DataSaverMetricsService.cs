using Microsoft.Data.Sqlite;

namespace MadagascarBrowser.Core.Data;

public sealed record DataSaverMetricsSnapshot(
    long SessionSavedBytes,
    long TotalSavedBytes,
    long SessionBlockedAds,
    long TotalBlockedAds,
    long SessionReplacedImages,
    long TotalReplacedImages,
    long SessionBlockedVideos,
    long TotalBlockedVideos);

public sealed class DataSaverMetricsService
{
    private readonly string _path;
    private long _sessionSavedBytes;
    private long _sessionBlockedAds;
    private long _sessionReplacedImages;
    private long _sessionBlockedVideos;

    public DataSaverMetricsService(Database database)
    {
        _path = database.Path;
    }

    public DataSaverMetricsSnapshot GetSnapshot()
    {
        using var connection = SqliteConnectionFactory.Open(_path);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT saved_bytes, blocked_ads, replaced_images, blocked_videos FROM data_saver_metrics WHERE id = 1";
        using var reader = command.ExecuteReader();
        if (!reader.Read())
            throw new InvalidOperationException("Les métriques de l'économiseur de données sont absentes.");

        return new DataSaverMetricsSnapshot(
            _sessionSavedBytes,
            reader.GetInt64(0),
            _sessionBlockedAds,
            reader.GetInt64(1),
            _sessionReplacedImages,
            reader.GetInt64(2),
            _sessionBlockedVideos,
            reader.GetInt64(3));
    }

    public void RecordAdBlocked()
    {
        Record(0, 1, 0, 0);
        _sessionBlockedAds++;
    }

    public void RecordImageReplaced(long contentLength)
    {
        Record(contentLength, 0, 1, 0);
        _sessionSavedBytes += contentLength;
        _sessionReplacedImages++;
    }

    public void RecordVideoBlocked(long contentLength)
    {
        Record(contentLength, 0, 0, 1);
        _sessionSavedBytes += contentLength;
        _sessionBlockedVideos++;
    }

    private void Record(long savedBytes, long ads, long images, long videos)
    {
        if (savedBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(savedBytes));

        using var connection = SqliteConnectionFactory.Open(_path);
        using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE data_saver_metrics
SET saved_bytes = saved_bytes + $savedBytes,
    blocked_ads = blocked_ads + $ads,
    replaced_images = replaced_images + $images,
    blocked_videos = blocked_videos + $videos
WHERE id = 1";
        command.Parameters.AddWithValue("$savedBytes", savedBytes);
        command.Parameters.AddWithValue("$ads", ads);
        command.Parameters.AddWithValue("$images", images);
        command.Parameters.AddWithValue("$videos", videos);
        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("Impossible d'enregistrer les métriques de l'économiseur de données.");
    }
}
