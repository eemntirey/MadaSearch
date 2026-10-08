namespace MadagascarBrowser.Core.Data
{
    public class BrowserSettings
    {
        public string SearchProvider { get; set; } = "google";
        public bool AdBlockEnabled { get; set; } = true;
        public bool DataSaverEnabled { get; set; }
        public bool RestoreSession { get; set; }
    }
}