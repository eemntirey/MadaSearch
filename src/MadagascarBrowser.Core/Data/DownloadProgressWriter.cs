using System;

namespace MadagascarBrowser.Core.Data
{
    /// <summary>
    /// Détermine si la progression d'un téléchargement doit être écrite en base.
    /// L'écriture est limitée à un intervalle temporel et à un pas de pourcentage,
    /// afin de ne pas saturer SQLite pendant un téléchargement.
    /// </summary>
    public class DownloadProgressWriter
    {
        /// <summary>Intervalle minimal entre deux écritures.</summary>
        public static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(500);

        /// <summary>Variation minimale de pourcentage entre deux écritures.</summary>
        public const double DefaultMinimumPercentDelta = 5;

        private readonly object _lock = new();
        private readonly TimeSpan _interval;
        private readonly double _minimumPercentDelta;
        private DateTime _lastWriteUtc;
        private double _lastWrittenPercent = -1;

        public DownloadProgressWriter(TimeSpan? interval = null, double minimumPercentDelta = DefaultMinimumPercentDelta)
        {
            _interval = interval ?? DefaultInterval;
            _minimumPercentDelta = minimumPercentDelta;
        }

        /// <summary>
        /// Indique si la progression courante doit être persistée.
        /// </summary>
        /// <param name="bytesReceived">Octets déjà reçus.</param>
        /// <param name="totalBytes">Taille totale connue (0 si inconnue).</param>
        /// <param name="nowUtc">Instant courant (UTC).</param>
        public bool ShouldPersist(long bytesReceived, long totalBytes, DateTime nowUtc)
        {
            var percent = totalBytes > 0
                ? Math.Clamp(bytesReceived * 100.0 / totalBytes, 0, 100)
                : 0;

            lock (_lock)
            {
                var first = _lastWriteUtc == default;
                var elapsed = !first && nowUtc - _lastWriteUtc >= _interval;
                var stepped = _lastWrittenPercent >= 0 && percent - _lastWrittenPercent >= _minimumPercentDelta;

                if (!first && !elapsed && !stepped)
                    return false;

                _lastWriteUtc = nowUtc;
                _lastWrittenPercent = percent;
                return true;
            }
        }
    }
}
