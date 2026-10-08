using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MadagascarBrowser.Core.Data;
using MadagascarBrowser.Shell;

namespace MadagascarBrowser
{
    public partial class LibraryWindow : Window
    {
        private readonly Action<string> _openUrl;

        public LibraryWindow(Action<string> openUrl, int selectedTab = 0)
        {
            InitializeComponent();
            BrowserServices.EnsureInitialized();
            _openUrl = openUrl;
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

        private void LoadItems()
        {
            var showHistory = LibraryTabs.SelectedIndex == 0;
            HistorySearch.IsEnabled = showHistory;
            ClearHistoryButton.IsEnabled = showHistory;
            if (showHistory)
            {
                var query = HistorySearch.Text.Trim();
                HistoryList.ItemsSource = query.Length == 0
                    ? BrowserServices.History.Recent(1000)
                    : BrowserServices.History.Search(query, 1000);
            }
            else if (LibraryTabs.SelectedIndex == 1)
            {
                FavoritesList.ItemsSource = BrowserServices.Favorites.GetAll();
            }
            else
            {
                OfflinePagesList.ItemsSource = BrowserServices.OfflinePages.GetAll();
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
                    MessageBox.Show(this, "Le fichier hors ligne est introuvable.", "Pages hors ligne",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _openUrl(path);
            }
            else if (selected is Visit visit)
            {
                _openUrl(visit.Url);
            }
            else if (selected is Favorite favorite)
            {
                _openUrl(favorite.Url);
            }
            Close();
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedItem();
            if (selected == null)
                return;

            switch (selected)
            {
                case Visit visit:
                    BrowserServices.History.Remove(visit.Url);
                    break;
                case Favorite favorite:
                    BrowserServices.Favorites.Remove(favorite.Url);
                    break;
                case OfflinePage page:
                    BrowserServices.OfflinePages.Remove(page.Id);
                    break;
            }
            LoadItems();
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "Effacer tout l'historique de navigation ?", "Historique",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            BrowserServices.History.Clear();
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