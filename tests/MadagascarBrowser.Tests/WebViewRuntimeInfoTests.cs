using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Tests;

public class WebViewRuntimeInfoTests
{
    [Fact]
    public void Detect_EvergreenRecent_IsUpToDate_AndShowsNoNotice()
    {
        var info = WebViewRuntimeInfo.Detect("141.0.3537.85", fixedVersionDeployment: false);

        Assert.Equal(WebViewRuntimeChannel.Evergreen, info.Channel);
        Assert.Equal(WebViewRuntimeStatus.UpToDate, info.Status);
        Assert.False(info.RequiresSecurityNotice);
    }

    [Fact]
    public void Detect_FixedOldVersion_IsOutdated_AndShowsNotice()
    {
        var info = WebViewRuntimeInfo.Detect("120.0.2210.91", fixedVersionDeployment: true);

        Assert.Equal(WebViewRuntimeChannel.Fixed, info.Channel);
        Assert.Equal(WebViewRuntimeStatus.Outdated, info.Status);
        Assert.True(info.RequiresSecurityNotice);
    }

    [Fact]
    public void Detect_FixedRecentVersion_IsUpToDate()
    {
        var info = WebViewRuntimeInfo.Detect("141.0.3537.85", fixedVersionDeployment: true);

        Assert.Equal(WebViewRuntimeStatus.UpToDate, info.Status);
        Assert.False(info.RequiresSecurityNotice);
    }

    [Fact]
    public void Detect_EvergreenOldVersion_IsNeverFlagged_ByTheApplication()
    {
        // Evergreen est mis à jour par Microsoft : l'application ne se substitue
        // pas au mécanisme de mise à jour.
        var info = WebViewRuntimeInfo.Detect("120.0.2210.91", fixedVersionDeployment: false);

        Assert.Equal(WebViewRuntimeStatus.UpToDate, info.Status);
        Assert.False(info.RequiresSecurityNotice);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("version inconnue")]
    public void Detect_NoUsableVersion_IsUnknown(string? version)
    {
        var info = WebViewRuntimeInfo.Detect(version, fixedVersionDeployment: true);

        Assert.Equal(WebViewRuntimeStatus.Unknown, info.Status);
        Assert.False(info.RequiresSecurityNotice);
    }

    [Theory]
    [InlineData("141.0.3537.85", "141.0.3537.85")]
    [InlineData("141.0.3537.85 (Official build)", "141.0.3537.85")]
    [InlineData("141.0.3537.85 beta", "141.0.3537.85")]
    [InlineData("141", "141.0")]
    public void ParseVersion_IgnoresSuffixes(string raw, string expected)
    {
        Assert.Equal(Version.Parse(expected), WebViewRuntimeInfo.ParseVersion(raw));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abcd")]
    [InlineData(".")]
    public void ParseVersion_InvalidInput_ReturnsNull(string? raw)
    {
        Assert.Null(WebViewRuntimeInfo.ParseVersion(raw));
    }
}
