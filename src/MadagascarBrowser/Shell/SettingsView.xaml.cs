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
            SelectByTag(SearchProviderCombo, settings.SearchProvider);
            SelectByTag(ThemeCombo, settings.Theme);
            SelectByTag(DensityCombo, settings.Density);
            SelectByTag(AccentCombo, settings.Accent);
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

        /// <summary>
        /// Sélectionne l'élément d'une liste dont la balise correspond à la valeur enregistrée.
        /// </summary>
        private static void SelectByTag(ComboBox combo, string? tag)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Tag?.ToString() == tag)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        /// <summary>
        /// Construit les paramètres à partir de l'état des contrôles, en conservant
        /// les domaines autorisés et les préférences d'apparence courantes.
        /// </summary>
        private BrowserSettings BuildSettings(List<string>? allowedDomains = null)
        {
            var current = BrowserServices.CurrentSettings;
            return new BrowserSettings
            {
                SearchProvider = SearchProviderCombo.SelectedValue?.ToString() ?? "google",
                AdBlockEnabled = AdBlockCheck.IsChecked == true,
                DataSaverEnabled = DataSaverCheck.IsChecked == true,
                RestoreSession = RestoreSessionCheck.IsChecked == true,
                AllowedDomains = allowedDomains ?? current.AllowedDomains?.ToList() ?? new List<string>(),
                Theme = ThemeCombo.SelectedValue?.ToString() ?? current.Theme,
                Density = DensityCombo.SelectedValue?.ToString() ?? current.Density,
                Accent = AccentCombo.SelectedValue?.ToString() ?? current.Accent
            };
        }

        private async void ClearData_Click(object sender, RoutedEventArgs e)
        {
            var clearHistory = ClearHistoryCheck.IsChecked == true;
            var clearCookies = ClearCookiesCheck.IsChecked == true;
            var clearCache = ClearCacheCheck.IsChecked == true;
            var selected = new List<string>();
            if (clearHistory) selected.Add("historique");
            if (clearCookies) selected.Add("cookies et données de session");
            if (clearCache) selected.Add("cache WebView2");

            if (selected.Count == 0)
            {
                ClearDataFeedback.Text = "Sélectionnez au moins une catégorie.";
                return;
            }

            var message = "Cette action supprimera uniquement : " + string.Join(", ", selected) +
                ". Les favoris, les téléchargements et les paramètres ne seront pas supprimés. Continuer ?";
            if (MessageBox.Show(Window.GetWindow(this), message, "Confirmer l'effacement",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                ClearDataFeedback.Text = "Effacement annulé.";
                return;
            }

            try
            {
                if (clearCookies || clearCache)
                {
                    if (Window.GetWindow(this)?.DataContext is not TabManager tabManager)
                        throw new InvalidOperationException("Gestionnaire de navigation indisponible.");
                    await tabManager.ClearWebViewDataAsync(clearCookies, clearCache);
                }

                if (clearHistory)
                {
                    await DatabaseAccess.WriteAsync(() => BrowserServices.History.Clear());
                    var historyIsEmpty = await DatabaseAccess.ReadAsync(() => BrowserServices.History.Recent(1).Count == 0);
                    if (!historyIsEmpty)
                        throw new InvalidOperationException("La vérification de l'historique a échoué.");
                }

                ClearDataFeedback.Text = "Effacement terminé : " + string.Join(", ", selected) +
                    ". L'historique a été vérifié si cette catégorie était sélectionnée.";
            }
            catch (Exception exception)
            {
                ClearDataFeedback.Text = "Effacement incomplet : " + exception.Message;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await DatabaseAccess.WriteAsync(() => BrowserServices.UpdateSettings(BuildSettings()));
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
            ThemeManager.Apply(BrowserServices.CurrentSettings);
        }

        /// <summary>
        /// Prévisualise immédiatement l'accent choisi, avant l'enregistrement.
        /// </summary>
        private void AccentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ThemeManager.ApplyAccent(AccentCombo.SelectedValue?.ToString());
        }

        /// <summary>
        /// Prévisualise immédiatement la densité choisie, avant l'enregistrement.
        /// </summary>
        private void DensityCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ThemeManager.ApplyDensity(DensityCombo.SelectedValue?.ToString());
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

            try
            {
                await DatabaseAccess.WriteAsync(() => BrowserServices.UpdateSettings(BuildSettings(allowed)));
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

            try
            {
                await DatabaseAccess.WriteAsync(() => BrowserServices.UpdateSettings(BuildSettings(allowed)));
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
