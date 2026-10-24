namespace MadagascarBrowser.Core.Data
{
    public enum DownloadState
    {
        InProgress,
        Completed,
        Interrupted,
        Cancelled,
        Failed
    }

    public record BrowserDownload(
        long Id,
        string Url,
        string Path,
        long BytesReceived,
        long TotalBytes,
        DownloadState State,
        System.DateTime StartedAt)
    {
        public double ProgressPercentage => TotalBytes > 0
            ? Math.Min(100, BytesReceived * 100d / TotalBytes)
            : 0;

        /// <summary>
        /// Nom du fichier téléchargé, sans le chemin complet.
        /// </summary>
        public string FileName => System.IO.Path.GetFileName(Path);
    }
}