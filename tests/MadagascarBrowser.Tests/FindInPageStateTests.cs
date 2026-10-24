using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class FindInPageStateTests
{
    [Fact]
    public void Start_AucuneRechercheEnCours()
    {
        var state = new FindInPageState();

        state.Start("  ");

        Assert.False(state.IsActive);
        Assert.Equal(string.Empty, state.Query);
        Assert.Equal(0, state.MatchCount);
        Assert.Equal(0, state.MatchIndex);
    }

    [Fact]
    public void Start_LaRechercheEstActivee()
    {
        var state = new FindInPageState();

        state.Start(" Madagascar ");

        Assert.True(state.IsActive);
        Assert.Equal("Madagascar", state.Query);
    }

    [Fact]
    public void ReportMatches_CompteLesCorrespondances()
    {
        var state = new FindInPageState();
        state.Start("a");

        state.ReportMatches(3, 1);

        Assert.Equal(3, state.MatchCount);
        Assert.Equal(1, state.MatchIndex);
    }

    [Fact]
    public void ReportMatches_AucuneCorrespondance_IndexAZero()
    {
        var state = new FindInPageState();
        state.Start("a");

        state.ReportMatches(0, 2);

        Assert.Equal(0, state.MatchCount);
        Assert.Equal(0, state.MatchIndex);
    }

    [Fact]
    public void NextIndex_SuivantFaitUnTour()
    {
        var state = new FindInPageState();
        state.Start("a");
        state.ReportMatches(3, 3);

        Assert.Equal(1, state.NextIndex(forward: true));
    }

    [Fact]
    public void NextIndex_PrecedentRevientAuDernier()
    {
        var state = new FindInPageState();
        state.Start("a");
        state.ReportMatches(3, 1);

        Assert.Equal(3, state.NextIndex(forward: false));
    }

    [Fact]
    public void NextIndex_Suivantincremente()
    {
        var state = new FindInPageState();
        state.Start("a");
        state.ReportMatches(3, 1);

        Assert.Equal(2, state.NextIndex(forward: true));
    }

    [Fact]
    public void NextIndex_SansCorrespondance_RetourneZero()
    {
        var state = new FindInPageState();
        state.Start("a");

        Assert.Equal(0, state.NextIndex(forward: true));
        Assert.Equal(0, state.NextIndex(forward: false));
    }

    [Fact]
    public void Clear_FermeLaRecherche()
    {
        var state = new FindInPageState();
        state.Start("a");
        state.ReportMatches(3, 2);

        state.Clear();

        Assert.False(state.IsActive);
        Assert.Equal(0, state.MatchCount);
    }
}
