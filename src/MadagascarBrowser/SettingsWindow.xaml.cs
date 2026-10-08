using System.Windows;
using System.Windows.Controls;
using MadagascarBrowser.Core.Data;
using MadagascarBrowser.Shell;

namespace MadagascarBrowser
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            BrowserServices.EnsureInitialized();
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
                DialogResult = true;
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(this, $"Impossible d'enregistrer les paramètres : {exception.Message}",
                    "Madagascar Browser", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}