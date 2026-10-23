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
        private SecurityIndicator _securityIndicator = SecurityIndicator.None;
        private bool _hasNavigationError;
        private string _navigationErrorTitle = string.Empty;
        private string _navigationErrorMessage = string.Empty;

        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Url { get => _url; set { _url = value; OnPropertyChanged(); } }
        public string Favicon { get => _favicon; set { _favicon = value; OnPropertyChanged(); } }
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        public bool HasNavigationError { get => _hasNavigationError; set { _hasNavigationError = value; OnPropertyChanged(); } }
        public string NavigationErrorTitle { get => _navigationErrorTitle; set { _navigationErrorTitle = value; OnPropertyChanged(); } }
        public string NavigationErrorMessage { get => _navigationErrorMessage; set { _navigationErrorMessage = value; OnPropertyChanged(); } }

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
            set
            {
                if (_securityState == value)
                    return;
                _securityState = value;
                _securityIndicator = SecurityIndicatorResolver.Resolve(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SecurityGlyph));
                OnPropertyChanged(nameof(SecurityLabel));
                OnPropertyChanged(nameof(SecurityBrushKey));
            }
        }

        /// <summary>Glyphe du cadenas affiché pour l'état de sécurité courant.</summary>
        public string SecurityGlyph => _securityIndicator.Glyph;

        /// <summary>Libellé accessible de l'état de sécurité courant.</summary>
        public string SecurityLabel => _securityIndicator.Label;

        /// <summary>Clé du jeton de couleur à appliquer au cadenas.</summary>
        public string SecurityBrushKey => _securityIndicator.BrushKey;

        public TabKind Kind { get; set; } = TabKind.Page;
        public object View { get; set; } = null!;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
