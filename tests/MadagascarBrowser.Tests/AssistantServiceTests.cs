using MadagascarBrowser.Core.Intelligence;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MadagascarBrowser.Tests;

public class AssistantServiceTests
{
    [Fact]
    public void NoOpAssistantService_IsConfigured_ReturnsFalse()
    {
        var service = new NoOpAssistantService();
        Assert.False(service.IsConfigured);
        Assert.Null(service.ConfiguredProviderName);
    }

    [Fact]
    public async Task NoOpAssistantService_RequestAssistanceAsync_ReturnsExplicitError()
    {
        var service = new NoOpAssistantService();
        var state = await service.RequestAssistanceAsync(
            AssistantRequestType.SummarizePage,
            "test content",
            new List<string> { "https://example.mg/page" });

        Assert.Equal(AssistantRequestState.Error, state.State);
        Assert.NotNull(state.ErrorMessage);
        // Le message d'erreur explicite indique clairement l'absence de service.
        Assert.Contains("Service d'assistance non configuré", state.ErrorMessage);
        Assert.True(state.ContentExplicitlyConfirmed || state.ExplicitContent != null);
    }

    [Fact]
    public void NoOpAssistantService_CancelPendingRequest_DoesNotThrow()
    {
        var service = new NoOpAssistantService();
        service.CancelPendingRequest();
        Assert.False(service.IsConfigured);
    }

    [Fact]
    public void AssistantState_DefaultValues_AreSafe()
    {
        var state = new AssistantState();
        Assert.Equal(AssistantRequestState.Idle, state.State);
        Assert.NotNull(state.RelatedTabUrls);
        Assert.Empty(state.RelatedTabUrls);
        Assert.NotNull(state.References);
        Assert.Empty(state.References);
    }

    [Fact]
    public void AssistantPanelViewModel_NoOpService_StatusMessage_ContainsExplicitWarning()
    {
        var viewModel = new AssistantPanelViewModel(new NoOpAssistantService());
        Assert.False(viewModel.IsConfigured);
        Assert.Contains("Aucun service d'assistance configuré", viewModel.StatusMessage);
    }

    [Fact]
    public void AssistantPanelViewModel_StateChange_UpdatesDisplay()
    {
        var viewModel = new AssistantPanelViewModel(new NoOpAssistantService());
        viewModel.CurrentState = new AssistantState
        {
            RequestType = AssistantRequestType.SearchInPage,
            State = AssistantRequestState.Processing
        };
        Assert.Contains("Traitement", viewModel.StateDisplay);
    }
}
