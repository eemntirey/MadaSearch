using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Shell
{
    public class TabManager : INotifyPropertyChanged, ITabHost
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

        public TabViewModel NewTab(string? url = null)
        {
            BrowserServices.EnsureInitialized();
            url ??= WebViewEnvironment.StartPageUrl;
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

            var t = new TabViewModel { Title = title, Url = target };
            var view = new BrowserView(this) { DataContext = t };
            t.View = view;
            Tabs.Add(t);
            Active = t;
            return t;
        }

        public void CloseActiveTab()
        {
            if (Active != null)
                Close(Active);
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
            var index = Tabs.IndexOf(t);
            if (index < 0) return;
            var wasActive = ReferenceEquals(Active, t);

            Tabs.Remove(t);
            if (t.View is BrowserView view)
            {
                view.Dispose();
            }

            if (!wasActive)
                return;

            // Le retrait de l'élément sélectionné vide SelectedItem, ce qui efface
            // Active par la liaison bidirectionnelle de la barre d'onglets : sans
            // réaffectation explicite, la fenêtre resterait entièrement vide.
            if (Tabs.Count == 0)
            {
                NewTab();
                return;
            }

            Active = Tabs[TabSelection.NextIndex(index, Tabs.Count)];
        }
    }
}