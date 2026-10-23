using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Creates one WebView2 environment per profile directory. Normal tabs share the
    /// persistent profile; a private window receives its own temporary profile path.
    /// </summary>
    internal static class WebViewEnvironment
    {
        /// <summary>
        /// Host for the browser-owned start and error pages.
        /// </summary>
        public const string LocalHost = "startpage.localhost";

        public static string UserDataFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MadagascarBrowser",
            "WebView2");

        private static readonly ConcurrentDictionary<string, Lazy<Task<CoreWebView2Environment>>> Environments =
            new(StringComparer.OrdinalIgnoreCase);

        public static Task<CoreWebView2Environment> GetAsync(string? userDataFolder = null)
        {
            var folder = string.IsNullOrWhiteSpace(userDataFolder) ? UserDataFolder : userDataFolder;
            return Environments.GetOrAdd(folder, path => new Lazy<Task<CoreWebView2Environment>>(
                () =>
                {
                    Directory.CreateDirectory(path);
                    return CoreWebView2Environment.CreateAsync(null, path);
                },
                LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        public static void Forget(string userDataFolder)
        {
            Environments.TryRemove(userDataFolder, out _);
        }
    }
}
