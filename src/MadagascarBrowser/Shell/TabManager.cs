using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Shell
{
    public class TabManager : INotifyPropertyChanged
    {
        public ObservableCollection<TabViewModel> Tabs { get; } = new();
        private TabViewModel _active;
        public TabViewModel Active
        {
            get => _active;
            set
            {
                if (ReferenceEquals(_active, value)) return;
                _active = value;
                ActiveChanged?.Invoke(this, EventArgs.Empty);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
            }
        }

        public event EventHandler ActiveChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsPrivate { get; }
        private PrivateProfileSession? PrivateProfile { get; }

        public TabManager(bool isPrivate = false)
        {
            IsPrivate = isPrivate;
            if (isPrivate)
                PrivateProfile = new PrivateProfileSession();
        }

        public TabViewModel NewTab(string url = "https://" + WebViewEnvironment.LocalHost + "/index.html")
        {
            BrowserServices.EnsureInitialized();
            var kind = NavigationTargetResolver.Resolve(url, out var target, out var title);
            switch (kind)
            {
                case NavigationTargetKind.None:
                    target = url;
                    title = "Nouvel onglet";
                    break;
                case NavigationTargetKind.Blank:
                    title = "Nouvel onglet";
                    break;
                case NavigationTargetKind.Web when target.Contains(WebViewEnvironment.LocalHost, StringComparison.OrdinalIgnoreCase):
                    title = "Nouvel onglet";
                    break;
            }

            var t = new TabViewModel { Title = title, Url = target, IsPrivate = IsPrivate, PrivateProfileFolder = PrivateProfile?.DirectoryPath };
            var view = new BrowserView { DataContext = t };
            t.View = view;
            Tabs.Add(t);
            Active = t;
            return t;
        }

        public TabViewModel OpenLibrary(int selectedTab = 0)
        {
            var existing = Tabs.FirstOrDefault(t => t.Kind == TabKind.Library);
            if (existing != null)
            {
                if (existing.View is LibraryView library)
                    library.SelectTab(selectedTab);
                Active = existing;
                return existing;
            }

            return EnsureKindTab(TabKind.Library, "Bibliothèque",
                () => new LibraryView(OpenUrlInNewTab, selectedTab));
        }

        public TabViewModel OpenDownloads()
        {
            return EnsureKindTab(TabKind.Downloads, "Téléchargements", () => new DownloadsView());
        }

        public TabViewModel OpenSettings()
        {
            var existing = Tabs.FirstOrDefault(t => t.Kind == TabKind.Settings);
            if (existing != null)
            {
                if (existing.View is SettingsView settings)
                    settings.RefreshSettings();
                Active = existing;
                return existing;
            }

            return EnsureKindTab(TabKind.Settings, "Paramètres", () => new SettingsView());
        }

        private TabViewModel EnsureKindTab(TabKind kind, string title, Func<object> viewFactory)
        {
            var existing = Tabs.FirstOrDefault(t => t.Kind == kind);
            if (existing != null)
            {
                Active = existing;
                return existing;
            }

            BrowserServices.EnsureInitialized();
            var t = new TabViewModel { Title = title, Kind = kind };
            t.View = viewFactory();
            Tabs.Add(t);
            Active = t;
            return t;
        }

        private void OpenUrlInNewTab(string url)
        {
            NewTab(url);
        }

        public void Dispose()
        {
            foreach (var tab in Tabs.ToList())
            {
                if (tab.View is BrowserView view)
                    view.Dispose();
            }

            Tabs.Clear();
            Active = null!;
            PrivateProfile?.Dispose();
        }

        public async Task ClearWebViewDataAsync(bool clearCookies, bool clearCache)
        {
            if (!clearCookies && !clearCache)
                return;

            var view = Tabs.Select(tab => tab.View).OfType<BrowserView>()
                .FirstOrDefault(browserView => browserView.IsCoreReady);
            if (view == null)
                throw new InvalidOperationException("Aucun onglet WebView2 initialisé ne permet d'effacer les données.");

            await view.ClearWebViewDataAsync(clearCookies, clearCache);
        }

        public void RestoreSession(MadagascarBrowser.Core.Data.BrowserSession session)
        {
            var urls = session.Urls
                .Where(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                              (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .ToList();
            if (urls.Count == 0)
                return;

            foreach (var url in urls)
                NewTab(url);

            Active = Tabs[Math.Clamp(session.ActiveTabIndex, 0, Tabs.Count - 1)];
        }

        public void Close(TabViewModel t)
        {
            if (t == null) return;
            var idx = Tabs.IndexOf(t);
            Tabs.Remove(t);
            if (t.View is BrowserView view)
            {
                view.Dispose();
            }
            if (Active == t)
            {
                if (Tabs.Any())
                {
                    Active = Tabs[Math.Max(0, idx - 1)];
                }
                else
                {
                    NewTab();
                }
            }
        }
    }
}