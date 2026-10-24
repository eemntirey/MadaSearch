namespace MadagascarBrowser.Core.Navigation;

public static class SearchProviderResolver
{
    public static string Normalize(string? provider)
    {
        return provider?.Trim().ToLowerInvariant() switch
        {
            "duckduckgo" => "duckduckgo",
            "bing" => "bing",
            _ => "google"
        };
    }

    public static string GetSearchUrlTemplate(string? provider)
    {
        return Normalize(provider) switch
        {
            "duckduckgo" => "https://duckduckgo.com/?q=",
            "bing" => "https://www.bing.com/search?q=",
            _ => "https://www.google.mg/search?q="
        };
    }
}
