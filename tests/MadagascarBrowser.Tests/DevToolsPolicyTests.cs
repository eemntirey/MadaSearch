using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class DevToolsPolicyTests
{
    [Theory]
    [InlineData("Debug")]
    [InlineData("debug")]
    [InlineData("DEBUG")]
    public void IsEnabledFor_Debug_IsTrue(string configuration)
    {
        Assert.True(DevToolsPolicy.IsEnabledFor(configuration));
    }

    [Theory]
    [InlineData("Release")]
    [InlineData("release")]
    [InlineData("Public")]
    [InlineData("")]
    [InlineData(null)]
    public void IsEnabledFor_OtherConfigurations_IsFalse(string? configuration)
    {
        Assert.False(DevToolsPolicy.IsEnabledFor(configuration));
    }
}
