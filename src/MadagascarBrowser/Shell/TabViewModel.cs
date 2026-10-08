using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MadagascarBrowser.Shell
{
    public enum TabKind
    {
        Page,
        Library,
        Downloads,
        Settings
    }

    public class TabViewModel : INotifyPropertyChanged
    {
        private string _title = "Nouvel onglet";
        private string _url = "about:blank";
        private string _favicon = string.Empty;
        private bool _isLoading;

        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Url { get => _url; set { _url = value; OnPropertyChanged(); } }
        public string Favicon { get => _favicon; set { _favicon = value; OnPropertyChanged(); } }
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        public TabKind Kind { get; set; } = TabKind.Page;
        public object View { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}