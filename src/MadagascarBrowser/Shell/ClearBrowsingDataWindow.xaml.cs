using System;
using System.Windows;
using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Effacement granulaire des données du profil local. Les cases sont
    /// explicites : rien n'est supprimé sans sélection de l'utilisateur.
    /// </summary>
    public partial class ClearBrowsingDataWindow : Window
    {
        private readonly BrowserView _view;

        public ClearBrowsingDataWindow(BrowserView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Clear_Click(object sender, RoutedEventArgs e)
        {
            var selection = BrowsingDataSelection.Compose(
                cookies: CookiesCheck.IsChecked == true,
                cache: CacheCheck.IsChecked == true,
                history: HistoryCheck.IsChecked == true,
                localStorage: LocalStorageCheck.IsChecked == true,
                passwords: PasswordsCheck.IsChecked == true);

            if (!BrowsingDataSelection.HasSelection(selection))
            {
                FeedbackText.Text = "Sélectionnez au moins une catégorie de données.";
                return;
            }

            ClearButton.IsEnabled = false;
            FeedbackText.Text = "Effacement en cours…";
            try
            {
                await _view.ClearBrowsingDataAsync(selection);
                FeedbackText.Text = "Données de navigation effacées.";
            }
            catch (Exception exception)
            {
                AppLog.Error("Effacement des données de navigation impossible.", exception);
                FeedbackText.Text = $"Échec de l'effacement : {exception.Message}";
            }
            finally
            {
                ClearButton.IsEnabled = true;
            }
        }
    }
}
