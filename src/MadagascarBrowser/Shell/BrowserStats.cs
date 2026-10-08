using System.ComponentModel;

namespace MadagascarBrowser.Shell
{
    public class BrowserStats : INotifyPropertyChanged
    {
        public static BrowserStats Instance { get; } = new BrowserStats();

        private int _blockedCount;

        public int BlockedCount
        {
            get => _blockedCount;
            set { _blockedCount = value; OnPropertyChanged(nameof(BlockedCount)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
