using MadagascarBrowser.Shell;
using Xunit;

namespace MadagascarBrowser.Shell.Tests;

public class TabManagerGroupTests
{
    [Fact]
    public void CreateGroup_AddsGroupWithUniqueId()
    {
        var manager = new TabManager();
        var group1 = manager.CreateGroup("Travail", "#FF4438");
        var group2 = manager.CreateGroup("Personnel", "#19B36B");

        Assert.Equal(1, group1.Id);
        Assert.Equal(2, group2.Id);
        Assert.Equal("Travail", group1.Name);
        Assert.Equal("Personnel", group2.Name);
        Assert.Equal("#FF4438", group1.ColorHex);
        Assert.Equal("#19B36B", group2.ColorHex);
        Assert.False(group1.IsCollapsed);
        Assert.Equal(2, manager.Groups.Count);
    }

    [Fact]
    public void CreateGroup_DefaultValuesWhenEmpty()
    {
        var manager = new TabManager();
        var group = manager.CreateGroup(null!, null!);

        Assert.Equal("Nouveau groupe", group.Name);
        Assert.Equal("#FF4438", group.ColorHex);
    }

    [Fact]
    public void DeleteGroup_RemovesGroupAndUnassignsTabs()
    {
        var manager = new TabManager();
        var group = manager.CreateGroup("Test", "#FF4438");

        var tab = new TabViewModel { Title = "Tab", Url = "https://example.com" };
        manager.Tabs.Add(tab);
        manager.AssignTabToGroup(tab, group.Id);

        Assert.Equal(group.Id, tab.GroupId);
        Assert.Equal(1, manager.GetGroupTabs(group.Id).Count);

        manager.DeleteGroup(group.Id);

        Assert.DoesNotContain(group, manager.Groups);
        Assert.Equal(0, tab.GroupId);
        Assert.Empty(manager.GetGroupTabs(group.Id));
    }

    [Fact]
    public void DeleteGroup_UnknownId_DoesNothing()
    {
        var manager = new TabManager();
        manager.CreateGroup("Test", "#FF4438");

        manager.DeleteGroup(999);

        Assert.Single(manager.Groups);
    }

    [Fact]
    public void RenameGroup_UpdatesNameAndTabs()
    {
        var manager = new TabManager();
        var group = manager.CreateGroup("Ancien", "#FF4438");

        var tab = new TabViewModel { Title = "Tab", Url = "https://example.com" };
        manager.Tabs.Add(tab);
        manager.AssignTabToGroup(tab, group.Id);

        manager.RenameGroup(group.Id, "Nouveau");

        Assert.Equal("Nouveau", group.Name);
        Assert.Equal("Nouveau", tab.GroupName);
    }

    [Fact]
    public void RenameGroup_UnknownId_DoesNothing()
    {
        var manager = new TabManager();
        manager.CreateGroup("Test", "#FF4438");

        manager.RenameGroup(999, "Nouveau");

        Assert.Equal("Test", manager.Groups[0].Name);
    }

    [Fact]
    public void ToggleGroupCollapse_ChangesCollapseState()
    {
        var manager = new TabManager();
        var group = manager.CreateGroup("Test", "#FF4438");

        Assert.False(group.IsCollapsed);
        manager.ToggleGroupCollapse(group.Id);
        Assert.True(group.IsCollapsed);
        manager.ToggleGroupCollapse(group.Id);
        Assert.False(group.IsCollapsed);
    }

    [Fact]
    public void GetGroupTabs_ReturnsTabsInGroup()
    {
        var manager = new TabManager();
        var group = manager.CreateGroup("Test", "#FF4438");

        var tab1 = new TabViewModel { Title = "T1", Url = "https://one.example" };
        var tab2 = new TabViewModel { Title = "T2", Url = "https://two.example" };
        var tab3 = new TabViewModel { Title = "T3", Url = "https://three.example" };
        manager.Tabs.Add(tab1);
        manager.Tabs.Add(tab2);
        manager.Tabs.Add(tab3);

        manager.AssignTabToGroup(tab1, group.Id);
        manager.AssignTabToGroup(tab2, group.Id);

        var groupTabs = manager.GetGroupTabs(group.Id);
        Assert.Equal(2, groupTabs.Count);
        Assert.Contains(tab1, groupTabs);
        Assert.Contains(tab2, groupTabs);
        Assert.DoesNotContain(tab3, groupTabs);
    }

    [Fact]
    public void CloseTab_RemovesFromGroup()
    {
        var manager = new TabManager();
        var group = manager.CreateGroup("Test", "#FF4438");

        var tab = new TabViewModel { Title = "Tab", Url = "https://example.com" };
        manager.Tabs.Add(tab);
        manager.AssignTabToGroup(tab, group.Id);

        manager.Close(tab);

        Assert.Equal(0, tab.GroupId);
        Assert.Empty(manager.GetGroupTabs(group.Id));
        Assert.DoesNotContain(tab, manager.Tabs);
    }

    [Fact]
    public void AssignTabToGroup_RemovesFromPreviousGroup()
    {
        var manager = new TabManager();
        var group1 = manager.CreateGroup("G1", "#FF4438");
        var group2 = manager.CreateGroup("G2", "#19B36B");

        var tab = new TabViewModel { Title = "Tab", Url = "https://example.com" };
        manager.Tabs.Add(tab);
        manager.AssignTabToGroup(tab, group1.Id);
        Assert.Equal(group1.Id, tab.GroupId);
        Assert.Single(manager.GetGroupTabs(group1.Id));

        manager.AssignTabToGroup(tab, group2.Id);
        Assert.Equal(group2.Id, tab.GroupId);
        Assert.Empty(manager.GetGroupTabs(group1.Id));
        Assert.Single(manager.GetGroupTabs(group2.Id));
    }
}