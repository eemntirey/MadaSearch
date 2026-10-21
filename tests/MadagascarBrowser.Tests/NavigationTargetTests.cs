using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class NavigationTargetTests
{
    [Fact]
    public void Resolve_HttpUrl_IsWebTarget()
    {
        var kind = NavigationTargetResolver.Resolve("https://exemple.mg/page", out var target, out var title);

        Assert.Equal(NavigationTargetKind.Web, kind);
        Assert.Equal("https://exemple.mg/page", target);
        Assert.Equal("exemple.mg", title);
    }

    [Fact]
    public void Resolve_FileUri_IsLocalFileTarget()
    {
        var kind = NavigationTargetResolver.Resolve("file:///C:/offline/page.mhtml", out var target, out var title);

        Assert.Equal(NavigationTargetKind.LocalFile, kind);
        Assert.StartsWith("file:///", target);
        Assert.Equal("page.mhtml", title);
    }

    [Fact]
    public void Resolve_LocalPath_IsLocalFileTarget()
    {
        var path = Path.Combine(Path.GetTempPath(), "offline-page.mhtml");

        var kind = NavigationTargetResolver.Resolve(path, out var target, out var title);

        Assert.Equal(NavigationTargetKind.LocalFile, kind);
        Assert.StartsWith("file:///", target);
        Assert.Equal("offline-page.mhtml", title);
    }

    [Fact]
    public void Resolve_AboutBlank_IsBlankTarget()
    {
        var kind = NavigationTargetResolver.Resolve("about:blank", out var target, out var title);

        Assert.Equal(NavigationTargetKind.Blank, kind);
        Assert.Equal("about:blank", target);
        Assert.Equal("Nouvel onglet", title);
    }

    [Fact]
    public void Resolve_Empty_IsNone()
    {
        var kind = NavigationTargetResolver.Resolve("   ", out var target, out var title);

        Assert.Equal(NavigationTargetKind.None, kind);
        Assert.Equal(string.Empty, target);
        Assert.Equal(string.Empty, title);
    }

    [Fact]
    public void Resolve_UnknownScheme_IsNone()
    {
        var kind = NavigationTargetResolver.Resolve("mailto:contact@exemple.mg", out _, out _);

        Assert.Equal(NavigationTargetKind.None, kind);
    }
}
