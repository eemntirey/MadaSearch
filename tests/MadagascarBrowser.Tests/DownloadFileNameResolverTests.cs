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

    [Fact]
    public void Resolve_InlineDisposition_UsesExtendedFileName()
    {
        var fileName = DownloadFileNameResolver.Resolve(
            "inline; filename*=UTF-8''facture-%E2%82%AC.txt",
            "https://example.com/telecharger");

        Assert.Equal("facture-€.txt", fileName);
    }

    [Fact]
    public void Resolve_QuotedExtendedValue_IsDecoded()
    {
        var fileName = DownloadFileNameResolver.Resolve(
            "attachment; filename=\"fallback.txt\"; filename*=\"UTF-8''caf%C3%A9.txt\"",
            "https://example.com/download");

        Assert.Equal("café.txt", fileName);
    }

    [Fact]
    public void Resolve_KeepsDoubleExtension()
    {
        var fileName = DownloadFileNameResolver.Resolve(
            "attachment; filename=archive.tar.gz",
            "https://example.com/download");

        Assert.Equal("archive.tar.gz", fileName);
    }

    [Fact]
    public void Resolve_ExtendedValue_CannotEscapeDirectory()
    {
        var fileName = DownloadFileNameResolver.Resolve(
            "attachment; filename*=UTF-8''..%2Fsecret.txt",
            "https://example.com/download");

        Assert.Equal("secret.txt", fileName);
    }

    [Fact]
    public void Resolve_HeaderWithoutFilename_FallsBackToUri()
    {
        Assert.Equal(
            "rapport.pdf",
            DownloadFileNameResolver.Resolve("attachment", "https://example.com/dossier/rapport.pdf?token=1"));
    }
}
