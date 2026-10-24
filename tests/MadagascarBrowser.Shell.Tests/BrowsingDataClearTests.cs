using MadagascarBrowser.Shell;
using Microsoft.Web.WebView2.Core;

namespace MadagascarBrowser.Tests;

public sealed class BrowsingDataClearTests
{
    [Fact]
    public void EmptySelectionDoesNotClearAnyWebViewData()
    {
        Assert.Equal((CoreWebView2BrowsingDataKinds)0,
            BrowserView.ResolveBrowsingDataKinds(clearHistory: false, clearCookies: false, clearCache: false));
    }

    [Fact]
    public void HistorySelectionClearsOnlyWebViewBrowsingHistory()
    {
        Assert.Equal(CoreWebView2BrowsingDataKinds.BrowsingHistory,
            BrowserView.ResolveBrowsingDataKinds(clearHistory: true, clearCookies: false, clearCache: false));
    }

    [Fact]
    public void SiteDataSelectionClearsSiteDataWithoutSelectingHistoryOrDiskCache()
    {
        Assert.Equal(CoreWebView2BrowsingDataKinds.AllSite,
            BrowserView.ResolveBrowsingDataKinds(clearHistory: false, clearCookies: true, clearCache: false));
    }

    [Fact]
    public void CacheSelectionClearsOnlyDiskCache()
    {
        Assert.Equal(CoreWebView2BrowsingDataKinds.DiskCache,
            BrowserView.ResolveBrowsingDataKinds(clearHistory: false, clearCookies: false, clearCache: true));
    }

    [Fact]
    public void CombinedSelectionClearsExactlyTheRequestedCategories()
    {
        var expected = CoreWebView2BrowsingDataKinds.BrowsingHistory
            | CoreWebView2BrowsingDataKinds.AllSite
            | CoreWebView2BrowsingDataKinds.DiskCache;

        Assert.Equal(expected,
            BrowserView.ResolveBrowsingDataKinds(clearHistory: true, clearCookies: true, clearCache: true));
    }
}
