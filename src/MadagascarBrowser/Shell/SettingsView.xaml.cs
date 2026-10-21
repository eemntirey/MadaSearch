using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MadagascarBrowser.Core.Data;

namespace MadagascarBrowser.Shell
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            BrowserServices.EnsureInitialized();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = BrowserServices.CurrentSettings;
            foreach (ComboBoxItem item in SearchProviderCombo.Items)
            {
                if (item.Tag?.ToString() == settings.SearchProvider)
                {
                    SearchProviderCombo.SelectedItem = item;
                    break;
                }
            }
            AdBlockCheck.IsChecked = settings.AdBlockEnabled;
            DataSaverCheck.IsChecked = settings.DataSaverEnabled;
            RestoreSessionCheck.IsChecked = settings.RestoreSession;
            AllowedDomainsList.ItemsSource = settings.AllowedDomains?.ToList() ?? new List<string>();
            AllowedDomainsFeedback.Text = string.Empty;
        }

        public void RefreshSettings()
        {
            LoadSettings();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var settings = new BrowserSettings
            {
                SearchProvider = SearchProviderCombo.SelectedValue?.ToString() ?? "google",
                AdBlockEnabled = AdBlockCheck.IsChecked == true,
                DataSaverEnabled = DataSaverCheck.IsChecked == true,
                RestoreSession = RestoreSessionCheck.IsChecked == true,
                AllowedDomains = BrowserServices.CurrentSettings.AllowedDomains?.ToList() ?? new List<string>()
            };

            try
            {
                await DatabaseAccess.WriteAsync(() => BrowserServices.UpdateSettings(settings));
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Impossible d'enregistrer les paramètres : {exception.Message}",
                    "Madagascar Browser", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private async void AllowDomain_Click(object sender, RoutedEventArgs e)
        {
            var domain = AllowedDomainBox.Text?.Trim() ?? string.Empty;
            if (domain.Length == 0)
            {
                AllowedDomainsFeedback.Text = "Saisissez un domaine à autoriser.";
                return;
            }

            var allowed = BrowserServices.CurrentSettings.AllowedDomains?.ToList() ?? new List<string>();
            if (allowed.Any(existing => string.Equals(existing, domain, StringComparison.OrdinalIgnoreCase)))
            {
                AllowedDomainsFeedback.Text = "Ce domaine est déjà autorisé.";
                AllowedDomainsList.ItemsSource = allowed;
                return;
            }

            allowed.Add(domain);

            var settings = new BrowserSettings
            {
                SearchProvider = BrowserServices.CurrentSettings.SearchProvider,
                AdBlockEnabled = AdBlockCheck.IsChecked == true,
                DataSaverEnabled = DataSaverCheck.IsChecked == true,
                RestoreSession = RestoreSessionCheck.IsChecked == true,
                AllowedDomains = allowed
            };

            try
            {
                await DatabaseAccess.WriteAsync(() => BrowserServices.UpdateSettings(settings));
                AllowedDomainBox.Text = string.Empty;
                AllowedDomainsList.ItemsSource = allowed;
                AllowedDomainsFeedback.Text = "Domaine autorisé.";
            }
            catch (Exception exception)
            {
                AllowedDomainsFeedback.Text = $"Impossible d'enregistrer le domaine : {exception.Message}";
            }
        }

        private async void RemoveDomain_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not string domain)
                return;

            var allowed = BrowserServices.CurrentSettings.AllowedDomains?.ToList() ?? new List<string>();
            allowed.RemoveAll(existing => string.Equals(existing, domain, StringComparison.OrdinalIgnoreCase));

            var settings = new BrowserSettings
            {
                SearchProvider = BrowserServices.CurrentSettings.SearchProvider,
                AdBlockEnabled = AdBlockCheck.IsChecked == true,
                DataSaverEnabled = DataSaverCheck.IsChecked == true,
                RestoreSession = RestoreSessionCheck.IsChecked == true,
                AllowedDomains = allowed
            };

            try
            {
                await DatabaseAccess.WriteAsync(() => BrowserServices.UpdateSettings(settings));
                AllowedDomainsList.ItemsSource = allowed;
                AllowedDomainsFeedback.Text = "Domaine retiré.";
            }
            catch (Exception exception)
            {
                AllowedDomainsFeedback.Text = $"Impossible de retirer le domaine : {exception.Message}";
            }
        }
    }
}
