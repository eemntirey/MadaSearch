using System;

namespace MadagascarBrowser.Core.Navigation
{
    /// <summary>
    /// Les DevTools sont un outil interne : visibles dans les builds de
    /// développement, absents du build public. WebView2 fournit l'API
    /// (AreDevToolsEnabled / OpenDevToolsWindow) ; cette règle ne fait que
    /// décider de l'exposition.
    /// </summary>
    public static class DevToolsPolicy
    {
        public const string DebugConfiguration = "Debug";

        public static bool IsEnabledFor(string? configuration)
        {
            return string.Equals(configuration, DebugConfiguration, StringComparison.OrdinalIgnoreCase);
        }
    }
}
