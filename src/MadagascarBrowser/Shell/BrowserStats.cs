using System.ComponentModel;

namespace MadagascarBrowser.Shell
{
    public class BrowserStats : INotifyPropertyChanged
    {
        public static BrowserStats Instance { get; } = new BrowserStats();

        private long _blockedCount;
        private long _replacedImagesCount;
        private long _blockedVideosCount;
        private string _savedDataText = "↓ 0 Mo économisés";
        private string _savedDataTooltip = "Économies cette session : 0 octet. Cumul : 0 octet.";

        public long BlockedCount
        {
            get => _blockedCount;
            set { _blockedCount = value; OnPropertyChanged(nameof(BlockedCount)); }
        }

        /// <summary>Nombre d'images remplacées par un placeholder cette session.</summary>
        public long ReplacedImagesCount
        {
            get => _replacedImagesCount;
            set { _replacedImagesCount = value; OnPropertyChanged(nameof(ReplacedImagesCount)); }
        }

        /// <summary>Nombre de vidéos bloquées cette session.</summary>
        public long BlockedVideosCount
        {
            get => _blockedVideosCount;
            set { _blockedVideosCount = value; OnPropertyChanged(nameof(BlockedVideosCount)); }
        }

        public string SavedDataText
        {
            get => _savedDataText;
            set { _savedDataText = value; OnPropertyChanged(nameof(SavedDataText)); }
        }

        public string SavedDataTooltip
        {
            get => _savedDataTooltip;
            set { _savedDataTooltip = value; OnPropertyChanged(nameof(SavedDataTooltip)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
