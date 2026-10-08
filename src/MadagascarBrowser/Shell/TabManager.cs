using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

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

        public TabViewModel NewTab(string url = "https://startpage.local/index.html")
        {
            BrowserServices.EnsureInitialized();
            var title = Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Host != "startpage.local"
                ? uri.Host
                : "Nouvel onglet";
            var t = new TabViewModel { Title = title, Url = url };
            var view = new BrowserView { DataContext = t };
            t.View = view;
            Tabs.Add(t);
            Active = t;
            return t;
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