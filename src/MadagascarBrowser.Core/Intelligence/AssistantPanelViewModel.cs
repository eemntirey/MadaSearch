using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MadagascarBrowser.Core.Intelligence
{
    /// <summary>
    /// Modèle de vue pour le panneau d'assistance. Gère l'état, la confirmation
    /// explicite du contenu et les actions utilisateur.
    /// </summary>
    public class AssistantPanelViewModel : INotifyPropertyChanged
    {
        private readonly IAssistantService _service;

        public AssistantPanelViewModel(IAssistantService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public bool IsConfigured => _service.IsConfigured;
        public string? ProviderName => _service.ConfiguredProviderName;

        private AssistantState _currentState = new();
        public AssistantState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value ?? new AssistantState();
                OnPropertyChanged();
                OnPropertyChanged(nameof(StateDisplay));
                OnPropertyChanged(nameof(HasResult));
                OnPropertyChanged(nameof(HasError));
            }
        }

        public string StateDisplay => _currentState.State switch
        {
            AssistantRequestState.Idle => "Prêt",
            AssistantRequestState.AwaitingConfirmation => "En attente de confirmation",
            AssistantRequestState.Processing => "Traitement en cours…",
            AssistantRequestState.Completed => "Résultat disponible",
            AssistantRequestState.Error => "Erreur — aucune transmission automatique",
            _ => "Inconnu"
        };

        public bool HasResult => !string.IsNullOrWhiteSpace(_currentState.Result)
            && _currentState.State == AssistantRequestState.Completed;

        public bool HasError => _currentState.State == AssistantRequestState.Error
            && !string.IsNullOrWhiteSpace(_currentState.ErrorMessage);

        /// <summary>Message explicite pour l'utilisateur.</summary>
        public string StatusMessage => _service.IsConfigured
            ? (HasError ? (_currentState.ErrorMessage ?? "Erreur inconnue.") : "Service configuré.")
            : (_currentState.ErrorMessage ?? "Aucun service d'assistance configuré. Aucune donnée transmise automatiquement.");

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
