using System;

namespace MadagascarBrowser.Core.Blocking
{
    public enum DataSaverDecision { Pass, Block, Placeholder }

    public class DataSaverService
    {
        public bool Enabled { get; set; }

        public DataSaverService()
        {
            Enabled = false;
        }

        public DataSaverDecision Evaluate(Uri resourceUrl, string mimeType, long contentLength = 0)
        {
            if (!Enabled)
                return DataSaverDecision.Pass;

            if (!string.IsNullOrWhiteSpace(mimeType) && mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                return DataSaverDecision.Block;
            }

            if (!string.IsNullOrWhiteSpace(mimeType) && mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                if (contentLength > 200000)
                {
                    return DataSaverDecision.Placeholder;
                }
            }

            return DataSaverDecision.Pass;
        }
    }
}