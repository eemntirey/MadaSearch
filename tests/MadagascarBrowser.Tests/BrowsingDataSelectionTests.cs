using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class BrowsingDataSelectionTests
{
    [Fact]
    public void Compose_MapsEachCategoryToItsFlag()
    {
        var selection = BrowsingDataSelection.Compose(
            cookies: true,
            cache: false,
            history: true,
            localStorage: false,
            passwords: true);

        Assert.True(selection.HasFlag(BrowsingDataKind.Cookies));
        Assert.False(selection.HasFlag(BrowsingDataKind.Cache));
        Assert.True(selection.HasFlag(BrowsingDataKind.History));
        Assert.False(selection.HasFlag(BrowsingDataKind.LocalStorage));
        Assert.True(selection.HasFlag(BrowsingDataKind.Passwords));
    }

    [Fact]
    public void Compose_NoCheckbox_IsEmptySelection()
    {
        var selection = BrowsingDataSelection.Compose(false, false, false, false, false);

        Assert.Equal(BrowsingDataKind.None, selection);
        Assert.False(BrowsingDataSelection.HasSelection(selection));
    }

    [Fact]
    public void HasSelection_AnyCategory_IsTrue()
    {
        Assert.True(BrowsingDataSelection.HasSelection(BrowsingDataKind.Cookies));
        Assert.True(BrowsingDataSelection.HasSelection(BrowsingDataKind.Passwords));
    }
}
