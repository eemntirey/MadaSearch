using MadagascarBrowser.Core.Data;

namespace MadagascarBrowser.Tests;

public class TabGroupTests
{
    [Fact]
    public void TabGroup_DefaultProperties_HaveSafeDefaults()
    {
        var group = new TabGroup();
        Assert.Equal(0, group.Id);
        Assert.Equal("Nouveau groupe", group.Name);
        Assert.Equal("#FF4438", group.ColorHex);
        Assert.False(group.IsCollapsed);
        Assert.NotNull(group.TabUrls);
        Assert.Empty(group.TabUrls);
    }

    [Fact]
    public void TabGroup_AddTabUrl_PreservesUrl()
    {
        var group = new TabGroup { Name = "Études", ColorHex = "#007E3A" };
        group.TabUrls.Add("https://wikipedia.org/madagascar");
        Assert.Single(group.TabUrls);
        Assert.Contains("https://wikipedia.org/madagascar", group.TabUrls);
    }

    [Fact]
    public void WorkspaceSession_DefaultProperties_AreSafe()
    {
        var session = new WorkspaceSession();
        Assert.Equal("Travail", session.WorkspaceName);
        Assert.Equal(0, session.ActiveTabIndex);
        Assert.NotNull(session.Urls);
        Assert.NotNull(session.Groups);
    }

}
