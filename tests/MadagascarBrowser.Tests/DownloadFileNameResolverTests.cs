using MadagascarBrowser.Core.Data;

namespace MadagascarBrowser.Tests;

public class DownloadFileNameResolverTests
{
    [Fact]
    public void Resolve_PrefersUtf8FilenameFromContentDisposition()
    {
        var fileName = DownloadFileNameResolver.Resolve(
            "attachment; filename=\"fallback.txt\"; filename*=UTF-8''rapport-%C3%A9t%C3%A9.txt",
            "https://example.com/download");

        Assert.Equal("rapport-été.txt", fileName);
    }

    [Fact]
    public void Resolve_UsesQuotedFilenameFromContentDisposition()
    {
        var fileName = DownloadFileNameResolver.Resolve(
            "attachment; filename=\"rapport test.txt\"",
            "https://example.com/download");

        Assert.Equal("rapport test.txt", fileName);
    }

    [Fact]
    public void Resolve_RemovesPathComponentsFromHeaderFilename()
    {
        var fileName = DownloadFileNameResolver.Resolve(
            "attachment; filename=\"..\\\\dossier\\\\rapport.txt\"",
            "https://example.com/download");

        Assert.Equal("rapport.txt", fileName);
    }

    [Fact]
    public void Resolve_FallsBackToUriPathWhenHeaderIsMissingOrInvalid()
    {
        Assert.Equal("download-named.txt", DownloadFileNameResolver.Resolve(null, "https://example.com/download-named.txt"));
        Assert.Equal("download", DownloadFileNameResolver.Resolve("invalid header", null));
    }
}
