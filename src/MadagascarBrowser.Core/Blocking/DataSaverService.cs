using System;
using System.IO;

namespace MadagascarBrowser.Core.Blocking
{
    public enum DataSaverDecision { Pass, Block, Placeholder }

    public class DataSaverService
    {
        public const long DefaultImageThresholdBytes = 200 * 1024;

        public bool Enabled { get; set; }
        private long _imageThresholdBytes = DefaultImageThresholdBytes;

        public long ImageThresholdBytes
        {
            get => _imageThresholdBytes;
            set => _imageThresholdBytes = value > 0
                ? value
                : throw new ArgumentOutOfRangeException(nameof(value), "Le seuil d'image doit être supérieur à zéro.");
        }

        public DataSaverService()
        {
            Enabled = false;
        }

        public DataSaverDecision Evaluate(
            Uri resourceUrl,
            string mimeType,
            long? contentLength = null,
            bool domainExempt = false)
        {
            if (!Enabled || domainExempt)
                return DataSaverDecision.Pass;

            if (!string.IsNullOrWhiteSpace(mimeType) &&
                mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                return DataSaverDecision.Block;
            }

            if (string.IsNullOrWhiteSpace(mimeType) && IsKnownVideoExtension(resourceUrl))
                return DataSaverDecision.Block;

            if (!string.IsNullOrWhiteSpace(mimeType) && mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                if (contentLength > ImageThresholdBytes)
                {
                    return DataSaverDecision.Placeholder;
                }
            }

            return DataSaverDecision.Pass;
        }

        private static bool IsKnownVideoExtension(Uri resourceUrl)
        {
            return Path.GetExtension(resourceUrl.AbsolutePath).ToLowerInvariant() is
                ".mp4" or ".m4v" or ".webm" or ".ogv" or ".avi" or ".mkv" or ".mov";
        }
    }
}