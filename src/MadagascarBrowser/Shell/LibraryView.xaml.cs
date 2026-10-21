using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MadagascarBrowser.Core.Data;

namespace MadagascarBrowser.Shell
{
    public partial class LibraryView : UserControl
    {
        private readonly Action<string> _openUrl;

        public LibraryView(Action<string> openUrl, int selectedTab = 0)
        {
            InitializeComponent();
            BrowserServices.EnsureInitialized();
            _openUrl = openUrl;
            SelectTab(selectedTab);
        }

        public void SelectTab(int selectedTab)
        {
            LibraryTabs.SelectedIndex = Math.Clamp(selectedTab, 0, 2);
            LoadItems();
        }

        private void HistorySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded && LibraryTabs.SelectedIndex == 0)
                LoadItems();
        }

        private void LibraryTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && ReferenceEquals(e.Source, LibraryTabs))
                LoadItems();
        }

        private async void LoadItems()
        {
            var showHistory = LibraryTabs.SelectedIndex == 0;
            HistorySearch.IsEnabled = showHistory;
            ClearHistoryButton.IsEnabled = showHistory;

            if (showHistory)
            {
                var query = HistorySearch.Text.Trim();
                var items = await DatabaseAccess.ReadAsync(() => query.Length == 0
                    ? BrowserServices.History.Recent(1000)
                    : BrowserServices.History.Search(query, 1000));
                HistoryList.ItemsSource = items;
                HistoryEmptyState.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (LibraryTabs.SelectedIndex == 1)
            {
                var items = await DatabaseAccess.ReadAsync(() => BrowserServices.Favorites.GetAll());
                FavoritesList.ItemsSource = items;
                FavoritesEmptyState.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                var items = await DatabaseAccess.ReadAsync(() => BrowserServices.OfflinePages.GetAll());
                OfflinePagesList.ItemsSource = items;
                OfflineEmptyState.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OpenSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedItem();
            if (selected == null)
                return;

            if (selected is OfflinePage offlinePage)
            {
                var path = BrowserServices.OfflinePages.GetPath(offlinePage.Id);
                if (path == null || !File.Exists(path))
                {
                    MessageBox.Show("Le fichier hors ligne est introuvable.", "Pages hors ligne",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Schéma file:// : la page hors ligne est ouverte directement sur le fichier.
                _openUrl(new Uri(path).AbsoluteUri);
            }
            else if (selected is Visit visit)
            {
                _openUrl(visit.Url);
            }
            else if (selected is Favorite favorite)
            {
                _openUrl(favorite.Url);
            }
        }

        private async void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedItem();
            if (selected == null)
                return;

            switch (selected)
            {
                case Visit visit:
                    await DatabaseAccess.WriteAsync(() => BrowserServices.History.Remove(visit.Url));
                    break;
                case Favorite favorite:
                    await DatabaseAccess.WriteAsync(() => BrowserServices.Favorites.Remove(favorite.Url));
                    break;
                case OfflinePage page:
                    await DatabaseAccess.WriteAsync(() => BrowserServices.OfflinePages.Remove(page.Id));
                    break;
            }
            LoadItems();
        }

        private async void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Effacer tout l'historique de navigation ?", "Historique",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            await DatabaseAccess.WriteAsync(() => BrowserServices.History.Clear());
            LoadItems();
        }

        private void OpenSelected_Click(object sender, MouseButtonEventArgs e)
        {
            OpenSelected_Click(sender, (RoutedEventArgs)e);
        }

        private object? SelectedItem()
        {
            return LibraryTabs.SelectedIndex switch
            {
                0 => HistoryList.SelectedItem,
                1 => FavoritesList.SelectedItem,
                _ => OfflinePagesList.SelectedItem
            };
        }
    }
}
