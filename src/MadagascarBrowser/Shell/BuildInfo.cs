using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Informations de compilation. Les DevTools ne sont exposés qu'en Debug :
    /// le build public ne doit pas offrir un outil interne.
    /// </summary>
    internal static class BuildInfo
    {
#if DEBUG
        public const string Configuration = DevToolsPolicy.DebugConfiguration;
#else
        public const string Configuration = "Release";
#endif

        public static bool DevToolsEnabled => DevToolsPolicy.IsEnabledFor(Configuration);
    }
}
