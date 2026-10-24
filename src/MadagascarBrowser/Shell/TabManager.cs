using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MadagascarBrowser.Core.Data;
using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Shell
{
    public class TabManager : INotifyPropertyChanged, ITabHost
    {
        private readonly bool _isPrivate;

        public TabManager(bool isPrivate = false)
        {
            _isPrivate = isPrivate;
        }

        /// <summary>Vrai si cette fenêtre ne doit laisser aucune trace locale.</summary>
        public bool IsPrivate => _isPrivate;

        public ObservableCollection<TabViewModel> Tabs { get; } = new();
        private TabViewModel _active = null!;

        /// <summary>Groupes d'onglets nommés et colorés.</summary>
        public ObservableCollection<TabGroup> Groups { get; } = new();
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

        public event EventHandler? ActiveChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Signalé quand une page passe en plein écran.</summary>
        public event EventHandler<bool>? FullScreenChanged;

        /// <summary>Signalé quand l'utilisateur demande une fenêtre privée.</summary>
        public event EventHandler? PrivateWindowRequested;

        public void OpenPrivateWindow()
        {
            PrivateWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        private readonly List<string> _closedTabUrls = new();

        /// <summary>
        /// Active l'onglet suivant, avec passage à l'onglet suivant en fin de liste.
        /// </summary>
        public void SwitchToNextTab() => SwitchTab(1);

        /// <summary>Active l'onglet précédent, avec passage au dernier en début de liste.</summary>
        public void SwitchToPreviousTab() => SwitchTab(-1);

        private void SwitchTab(int offset)
        {
            if (Tabs.Count < 2)
                return;

            var index = Tabs.IndexOf(Active);
            if (index < 0)
                return;

            Active = Tabs[(index + offset + Tabs.Count) % Tabs.Count];
        }

        /// <summary>Rouvre le dernier onglet fermé (Ctrl+Maj+T).</summary>
        public void ReopenClosedTab()
        {
            if (_closedTabUrls.Count == 0)
                return;

            var url = _closedTabUrls[0];
            _closedTabUrls.RemoveAt(0);
            NewTab(url);
        }

        /// <summary>Ajoute l'onglet actif aux favoris (Ctrl+D).</summary>
        public void AddFavoriteToActiveTab()
        {
            var tab = Active;
            if (tab == null)
                return;

            if (!Uri.TryCreate(tab.Url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                AppLog.Info($"Ajout aux favoris ignoré : « {tab.Url} » n'est pas une page Web.");
                return;
            }

            var url = uri.AbsoluteUri;
            var title = tab.Title;
            _ = DatabaseAccess.WriteAsync(() => BrowserServices.Favorites.Add(url, title));
        }

        /// <summary>Masque ou réaffiche les barres de la fenêtre selon le plein écran.</summary>
        public void SetFullScreen(bool enabled)
        {
            FullScreenChanged?.Invoke(this, enabled);
        }

        /// <summary>Exécute une commande applicative issue d'un raccourci clavier.</summary>
        public void Execute(AcceleratorCommand command)
        {
            var view = Active?.View as BrowserView;
            switch (command)
            {
                case AcceleratorCommand.NewTab:
                    NewTab();
                    break;
                case AcceleratorCommand.CloseTab:
                    CloseActiveTab();
                    break;
                case AcceleratorCommand.FocusAddressBar:
                    view?.FocusAddressBar();
                    break;
                case AcceleratorCommand.Reload:
                    view?.Reload();
                    break;
                case AcceleratorCommand.NextTab:
                    SwitchToNextTab();
                    break;
                case AcceleratorCommand.PreviousTab:
                    SwitchToPreviousTab();
                    break;
                case AcceleratorCommand.AddFavorite:
                    AddFavoriteToActiveTab();
                    break;
                case AcceleratorCommand.ReopenClosedTab:
                    ReopenClosedTab();
                    break;
                case AcceleratorCommand.FindInPage:
                    view?.OpenFindBar();
                    break;
                case AcceleratorCommand.Print:
                    view?.Print();
                    break;
                case AcceleratorCommand.ExitFullScreen:
                    view?.ExitFullScreen();
                    break;
                case AcceleratorCommand.OpenDevTools:
                    view?.OpenDevTools();
                    break;
            }
        }

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

            var t = new TabViewModel { Title = title, Url = target, IsPrivate = _isPrivate };
            var view = new BrowserView(this, _isPrivate) { DataContext = t };
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

        public async Task ClearWebViewDataAsync(bool clearHistory, bool clearCookies, bool clearCache)
        {
            if (IsPrivate) throw new InvalidOperationException("L'effacement sélectif du profil normal est désactivé en navigation privée.");
            if (!clearHistory && !clearCookies && !clearCache) return;
            var views = Tabs.Select(tab => tab.View).OfType<BrowserView>().ToList();
            var view = views.FirstOrDefault(browserView => browserView.IsCoreReady);
            if (view == null)
            {
                var pending = views.FirstOrDefault();
                if (pending == null) throw new InvalidOperationException("Aucun onglet WebView2 ne permet d'effacer les données.");
                await pending.CoreReady;
                if (!pending.IsCoreReady) throw new InvalidOperationException("WebView2 n'est pas prêt pour effacer les données.");
                view = pending;
            }
            await view.ClearWebViewDataAsync(clearHistory, clearCookies, clearCache);
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

        public void Dispose()
        {
            foreach (var tab in Tabs.ToList())
                if (tab.View is BrowserView view) view.Dispose();
            Tabs.Clear();
            _active = null!;
        }

        /// <summary>Réduit ou restaure un groupe.</summary>
        public void ToggleGroupCollapse(int groupId)
        {
            var group = Groups.FirstOrDefault(g => g.Id == groupId);
            if (group != null)
            {
                group.IsCollapsed = !group.IsCollapsed;
                // Met à jour tous les onglets liés au groupe
                foreach (var tab in Tabs)
                {
                    if (tab.GroupId == groupId)
                        tab.GroupCollapsed = group.IsCollapsed;
                }
            }
        }

        /// <summary>Recherche rapide dans les onglets ouverts (titre ou URL).</summary>
        /// <param name="query">Texte de recherche.</param>
        /// <returns>Onglets correspondants.</returns>
        public System.Collections.Generic.List<TabViewModel> SearchTabs(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new();
            var q = query.ToLowerInvariant();
            return Tabs
                .Where(t => t.Title.ToLowerInvariant().Contains(q) ||
                            (t.Url ?? string.Empty).ToLowerInvariant().Contains(q))
                .ToList();
        }

        /// <summary>Détecte les onglets potentiellement dupliqués (même URL) et propose une action.</summary>
        /// <returns>Paires d'onglets avec la même URL, sans fermer automatiquement.</returns>
        public System.Collections.Generic.List<(TabViewModel first, TabViewModel second, string url)> DetectDuplicateTabs()
        {
            var duplicates = new System.Collections.Generic.List<(TabViewModel, TabViewModel, string)>();
            var seen = new System.Collections.Generic.Dictionary<string, TabViewModel>();
            foreach (var tab in Tabs)
            {
                if (string.IsNullOrWhiteSpace(tab.Url)) continue;
                var url = tab.Url;
                if (seen.TryGetValue(url, out var existing))
                {
                    duplicates.Add((existing, tab, url));
                }
                else
                {
                    seen[url] = tab;
                }
            }
            return duplicates;
        }

        /// <summary>Crée un nouveau groupe d'onglets nommé et coloré.</summary>
        /// <param name="name">Nom du groupe.</param>
        /// <param name="colorHex">Couleur hexadécimale du groupe.</param>
        /// <returns>Le groupe créé.</returns>
        public TabGroup CreateGroup(string name, string colorHex)
        {
            var group = new TabGroup
            {
                Id = Groups.Count > 0 ? Groups.Max(g => g.Id) + 1 : 1,
                Name = string.IsNullOrWhiteSpace(name) ? "Nouveau groupe" : name.Trim(),
                ColorHex = string.IsNullOrWhiteSpace(colorHex) ? "#FF4438" : colorHex.Trim(),
                OrderIndex = Groups.Count
            };
            Groups.Add(group);
            return group;
        }

        /// <summary>Supprime un groupe d'onglets et en retire tous les onglets sans les fermer.</summary>
        public void DeleteGroup(int groupId)
        {
            var group = Groups.FirstOrDefault(g => g.Id == groupId);
            if (group == null) return;

            // Retire l'association de groupe pour tous les onglets concernés sans les fermer.
            foreach (var tab in Tabs.Where(t => t.GroupId == groupId).ToList())
            {
                RemoveTabFromGroup(tab);
            }

            Groups.Remove(group);
        }

        /// <summary>Renomme un groupe d'onglets existant.</summary>
        public void RenameGroup(int groupId, string name)
        {
            var group = Groups.FirstOrDefault(g => g.Id == groupId);
            if (group == null || string.IsNullOrWhiteSpace(name)) return;

            group.Name = name.Trim();
            foreach (var tab in Tabs.Where(t => t.GroupId == groupId))
                tab.GroupName = group.Name;
        }

        /// <summary>Retourne les onglets appartenant à un groupe.</summary>
        public System.Collections.Generic.List<TabViewModel> GetGroupTabs(int groupId)
        {
            return Tabs.Where(t => t.GroupId == groupId).ToList();
        }

        /// <summary>Associe un onglet à un groupe (le retire de son groupe précédent le cas échéant).</summary>
        public void AssignTabToGroup(TabViewModel tab, int groupId)
        {
            var group = Groups.FirstOrDefault(g => g.Id == groupId);
            if (group == null) return;

            // Retire l'onglet de son groupe précédent sans le fermer.
            if (tab.GroupId != 0 && tab.GroupId != groupId)
                RemoveTabFromGroup(tab);

            tab.GroupId = groupId;
            tab.GroupName = group.Name;
            tab.GroupColorHex = group.ColorHex;
            if (!group.TabUrls.Contains(tab.Url ?? string.Empty))
                group.TabUrls.Add(tab.Url ?? string.Empty);
        }

        /// <summary>Retire un onglet de son groupe.</summary>
        public void RemoveTabFromGroup(TabViewModel tab)
        {
            var group = Groups.FirstOrDefault(g => g.Id == tab.GroupId);
            if (group != null && !string.IsNullOrWhiteSpace(tab.Url))
                group.TabUrls.Remove(tab.Url);
            tab.GroupId = 0;
            tab.GroupName = string.Empty;
            tab.GroupCollapsed = false;
        }

        public void Close(TabViewModel t)
        {
            if (t == null) return;
            var index = Tabs.IndexOf(t);
            if (index < 0) return;
            if (index < 0) return;
            var wasActive = ReferenceEquals(Active, t);

            // L'URL est mémorisée pour permettre la réouverture (Ctrl+Maj+T).
            if (!IsPrivate && !string.IsNullOrWhiteSpace(t.Url))
                _closedTabUrls.Insert(0, t.Url);

            // Retire l'onglet de son groupe sans supprimer le groupe.
            if (t.GroupId != 0)
                RemoveTabFromGroup(t);

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