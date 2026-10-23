using System.ComponentModel;
using System.Runtime.CompilerServices;
using MadagascarBrowser.Core.Navigation;

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
        private bool _isFullScreen;
        private ConnectionSecurity _securityState = ConnectionSecurity.None;

        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Url { get => _url; set { _url = value; OnPropertyChanged(); } }
        public string Favicon { get => _favicon; set { _favicon = value; OnPropertyChanged(); } }
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }

        /// <summary>Vrai quand la page affichée occupe tout l'espace (plein écran).</summary>
        public bool IsFullScreen
        {
            get => _isFullScreen;
            set { _isFullScreen = value; OnPropertyChanged(); }
        }

        /// <summary>État de sécurité de la page affichée, pilote l'icône du cadenas.</summary>
        public ConnectionSecurity SecurityState
        {
            get => _securityState;
            set { _securityState = value; OnPropertyChanged(); }
        }

        /// <summary>Indicates that this tab belongs to an isolated private window.</summary>
        public bool IsPrivate { get; set; }

        /// <summary>Temporary WebView2 profile folder for a private tab.</summary>
        public string? PrivateProfileFolder { get; set; }

        public TabKind Kind { get; set; } = TabKind.Page;
        public object View { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
