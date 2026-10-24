using System.Windows;
using System.Windows.Controls;
using MadagascarBrowser.Core.Intelligence;

namespace MadagascarBrowser.Shell
{
    public partial class AssistantPanel : UserControl
    {
        private AssistantPanelViewModel? _viewModel;

        public AssistantPanel()
        {
            InitializeComponent();
            DataContextChanged += AssistantPanel_DataContextChanged;
        }

        private void AssistantPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = DataContext as AssistantPanelViewModel;
            if (_viewModel == null && BrowserServices.Assistant != null)
            {
                _viewModel = new AssistantPanelViewModel(BrowserServices.Assistant);
                DataContext = _viewModel;
            }
        }

        private async void Summarize_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.CurrentState = new AssistantState
            {
                RequestType = AssistantRequestType.SummarizePage,
                State = AssistantRequestState.AwaitingConfirmation,
                ExplicitContent = GetExplicitSelectionOrEmpty(),
                ContentExplicitlyConfirmed = false
            };
            var result = await BrowserServices.Assistant.RequestAssistanceAsync(
                AssistantRequestType.SummarizePage,
                _viewModel.CurrentState.ExplicitContent,
                new System.Collections.Generic.List<string>());
            _viewModel.CurrentState = result;
            ResultText.Text = result.Result ?? result.ErrorMessage ?? "Aucun résultat.";
        }

        private async void Explain_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.CurrentState = new AssistantState
            {
                RequestType = AssistantRequestType.ExplainSelection,
                State = AssistantRequestState.AwaitingConfirmation,
                ExplicitContent = GetExplicitSelectionOrEmpty(),
                ContentExplicitlyConfirmed = false
            };
            var result = await BrowserServices.Assistant.RequestAssistanceAsync(
                AssistantRequestType.ExplainSelection,
                _viewModel.CurrentState.ExplicitContent,
                new System.Collections.Generic.List<string>());
            _viewModel.CurrentState = result;
            ResultText.Text = result.Result ?? result.ErrorMessage ?? "Aucun résultat.";
        }

        private async void SearchInPage_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.CurrentState = new AssistantState
            {
                RequestType = AssistantRequestType.SearchInPage,
                State = AssistantRequestState.AwaitingConfirmation,
                ExplicitContent = GetExplicitSelectionOrEmpty(),
                ContentExplicitlyConfirmed = false
            };
            var result = await BrowserServices.Assistant.RequestAssistanceAsync(
                AssistantRequestType.SearchInPage,
                _viewModel.CurrentState.ExplicitContent,
                new System.Collections.Generic.List<string>());
            _viewModel.CurrentState = result;
            ResultText.Text = result.Result ?? result.ErrorMessage ?? "Aucun résultat.";
        }

        private async void Synthesize_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.CurrentState = new AssistantState
            {
                RequestType = AssistantRequestType.SynthesizeWithReferences,
                State = AssistantRequestState.AwaitingConfirmation,
                ExplicitContent = GetExplicitSelectionOrEmpty(),
                ContentExplicitlyConfirmed = false
            };
            var result = await BrowserServices.Assistant.RequestAssistanceAsync(
                AssistantRequestType.SynthesizeWithReferences,
                _viewModel.CurrentState.ExplicitContent,
                new System.Collections.Generic.List<string>());
            _viewModel.CurrentState = result;
            ResultText.Text = result.Result ?? result.ErrorMessage ?? "Aucun résultat.";
        }

        /// <summary>
        /// Récupère la sélection explicite de l'utilisateur, jamais le document complet
        /// par défaut. Dans cette version, la sélection réelle nécessiterait l'intégration
        /// du script WebView2 ; ici, le contenu est explicitement vide si non confirmé.
        /// </summary>
        private string? GetExplicitSelectionOrEmpty()
        {
            // Aucune transmission automatique : le contenu doit être confirmé explicitement.
            // Dans une intégration réelle, cela proviendrait d'une sélection utilisateur.
            return null;
        }
    }
}
