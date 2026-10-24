using MadagascarBrowser.Shell;

namespace MadagascarBrowser.Shell.Tests;

public class TabManagerPrivateTests
{
    [Fact]
    public void OpenPrivateWindow_RaisesRequestWithoutCreatingTrace()
    {
        var manager = new TabManager();
        var raised = 0;
        manager.PrivateWindowRequested += (_, _) => raised++;

        manager.OpenPrivateWindow();

        Assert.Equal(1, raised);
        Assert.False(manager.IsPrivate);
    }

    [Fact]
    public void PrivateManager_ExposesItsIsolationMode()
    {
        Assert.True(new TabManager(isPrivate: true).IsPrivate);
    }
}
