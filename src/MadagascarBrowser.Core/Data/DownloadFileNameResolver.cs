using System.Net.Http.Headers;
using System.Text;

namespace MadagascarBrowser.Core.Data;

public static class DownloadFileNameResolver
{
    public static string Resolve(string? contentDisposition, string? downloadUri)
    {
        var extendedValue = ReadExtendedFileName(contentDisposition);
        var extendedName = Sanitize(DecodeExtendedValue(extendedValue));
        if (extendedName is not null)
            return extendedName;

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

    /// <summary>
    /// Extrait la valeur brute du paramètre RFC 5987 « filename* ».
    /// Le parseur de .NET ne décode pas toujours le pourcentage : on lit
    /// l'en-tête nous-mêmes pour ne pas dépendre de ce détail.
    /// </summary>
    private static string? ReadExtendedFileName(string? contentDisposition)
    {
        if (string.IsNullOrWhiteSpace(contentDisposition))
            return null;

        const string parameterName = "filename*=";
        var index = contentDisposition.IndexOf(parameterName, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            return null;

        var start = index + parameterName.Length;
        var end = start;
        var quoted = start < contentDisposition.Length && contentDisposition[start] == '"';
        if (quoted)
        {
            start++;
            end = contentDisposition.IndexOf('"', start);
            if (end < 0)
                return null;
        }
        else
        {
            end = contentDisposition.IndexOf(';', start);
            if (end < 0)
                end = contentDisposition.Length;
        }

        var value = contentDisposition[start..end].Trim();
        return value.Length == 0 ? null : value;
    }

    /// <summary>
    /// Décode une valeur RFC 5987 de la forme « charset'lang'valeur-pourcentée ».
    /// </summary>
    private static string? DecodeExtendedValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var encoding = Encoding.UTF8;
        var encoded = value;
        var parts = value.Split('\'', 3);
        if (parts.Length == 3)
        {
            encoded = parts[2];
            try
            {
                encoding = Encoding.GetEncoding(parts[0]);
            }
            catch (ArgumentException)
            {
                encoding = Encoding.UTF8;
            }
        }

        if (!encoded.Contains('%'))
            return encoded;

        var bytes = new List<byte>(encoded.Length);
        for (var index = 0; index < encoded.Length; index++)
        {
            var character = encoded[index];
            if (character == '%' && index + 2 < encoded.Length &&
                byte.TryParse(encoded.AsSpan(index + 1, 2), System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture, out var decodedByte))
            {
                bytes.Add(decodedByte);
                index += 2;
                continue;
            }

            bytes.AddRange(encoding.GetBytes(character.ToString()));
        }

        return encoding.GetString(bytes.ToArray());
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
