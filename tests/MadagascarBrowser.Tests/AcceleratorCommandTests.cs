using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class AcceleratorCommandTests
{
    [Theory]
    [InlineData("t", AcceleratorCommand.NewTab)]
    [InlineData("T", AcceleratorCommand.NewTab)]
    [InlineData("w", AcceleratorCommand.CloseTab)]
    [InlineData("l", AcceleratorCommand.FocusAddressBar)]
    public void Resolve_CtrlShortcut_ReturnsCommand(string key, AcceleratorCommand expected)
    {
        Assert.Equal(expected, AcceleratorCommandResolver.Resolve(key, ctrl: true, alt: false, shift: false));
    }

    [Fact]
    public void Resolve_WithoutCtrl_ReturnsNone()
    {
        Assert.Equal(AcceleratorCommand.None, AcceleratorCommandResolver.Resolve("t", ctrl: false, alt: false, shift: false));
    }

    [Fact]
    public void Resolve_WithAlt_ReturnsNone()
    {
        Assert.Equal(AcceleratorCommand.None, AcceleratorCommandResolver.Resolve("t", ctrl: true, alt: true, shift: false));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("n")]
    [InlineData("")]
    [InlineData(null)]
    public void Resolve_UnknownKey_ReturnsNone(string? key)
    {
        Assert.Equal(AcceleratorCommand.None, AcceleratorCommandResolver.Resolve(key, ctrl: true, alt: false, shift: false));
    }
}
