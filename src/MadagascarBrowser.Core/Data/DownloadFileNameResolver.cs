using System.Net.Http.Headers;

namespace MadagascarBrowser.Core.Data;

public static class DownloadFileNameResolver
{
    public static string Resolve(string? contentDisposition, string? downloadUri)
    {
        if (ContentDispositionHeaderValue.TryParse(contentDisposition, out var disposition))
        {
            var fileName = Sanitize(disposition.FileNameStar) ?? Sanitize(disposition.FileName);
            if (fileName is not null)
                return fileName;
        }

        if (Uri.TryCreate(downloadUri, UriKind.Absolute, out var uri))
            return Sanitize(uri.LocalPath) ?? "download";

        return "download";
    }

    private static string? Sanitize(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
            return null;

        var fileName = Path.GetFileName(candidate.Trim().Trim('"'));
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var invalidCharacters = Path.GetInvalidFileNameChars();
        fileName = new string(fileName
                .Select(character => invalidCharacters.Contains(character) ? '_' : character)
                .ToArray())
            .TrimEnd(' ', '.');

        return string.IsNullOrWhiteSpace(fileName) || fileName is "." or ".."
            ? null
            : fileName;
    }
}
