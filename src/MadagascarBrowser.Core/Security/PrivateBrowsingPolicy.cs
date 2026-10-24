namespace MadagascarBrowser.Core.Security;

/// <summary>
/// Central policy for deciding whether private-window activity may enter persistent browser data.
/// </summary>
public static class PrivateBrowsingPolicy
{
    public static bool ShouldPersistHistory(bool isPrivate, bool historyWorthy) =>
        !isPrivate && historyWorthy;

    public static bool ShouldPersistDownloadMetadata(bool isPrivate) => !isPrivate;
    public static bool ShouldPersistFavorites(bool isPrivate) => !isPrivate;
    public static bool ShouldPersistOfflinePages(bool isPrivate) => !isPrivate;
    public static bool ShouldPersistSession(bool isPrivate) => !isPrivate;
}
