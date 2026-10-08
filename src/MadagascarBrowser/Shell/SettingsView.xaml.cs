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
        }

        public void RefreshSettings()
        {
            LoadSettings();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var settings = new BrowserSettings
            {
                SearchProvider = SearchProviderCombo.SelectedValue?.ToString() ?? "google",
                AdBlockEnabled = AdBlockCheck.IsChecked == true,
                DataSaverEnabled = DataSaverCheck.IsChecked == true,
                RestoreSession = RestoreSessionCheck.IsChecked == true
            };

            try
            {
                BrowserServices.UpdateSettings(settings);
            }
            catch (System.Exception exception)
            {
                MessageBox.Show($"Impossible d'enregistrer les paramètres : {exception.Message}",
                    "Madagascar Browser", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }
    }
}
