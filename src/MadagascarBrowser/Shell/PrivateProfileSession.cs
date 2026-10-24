using System;
using System.IO;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Owns one temporary WebView2 profile for a private window. The exclusive lock
    /// lets a later process distinguish abandoned profiles from profiles still in use.
    /// </summary>
    internal sealed class PrivateProfileSession : IDisposable
    {
        private readonly FileStream _activeLock;
        private bool _disposed;

        public string DirectoryPath { get; }

        public PrivateProfileSession()
        {
            var root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MadagascarBrowser",
                "PrivateProfiles");
            Directory.CreateDirectory(root);
            CleanupOrphanedProfiles();

            DirectoryPath = Path.Combine(root, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirectoryPath);
            try
            {
                _activeLock = new FileStream(
                    Path.Combine(DirectoryPath, ".active.lock"),
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.None);
            }
            catch
            {
                try { Directory.Delete(DirectoryPath, recursive: true); } catch (IOException) { }
                throw;
            }
        }

        /// <summary>
        /// Removes abandoned profiles when their exclusive lock can be acquired.
        /// Profiles created moments ago are left alone to avoid racing another startup.
        /// </summary>
        public static void CleanupOrphanedProfiles()
        {
            var root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MadagascarBrowser",
                "PrivateProfiles");
            if (!Directory.Exists(root))
                return;

            foreach (var directory in Directory.GetDirectories(root))
            {
                try
                {
                    var lockPath = Path.Combine(directory, ".active.lock");
                    if (!File.Exists(lockPath))
                    {
                        if (DateTime.UtcNow - Directory.GetCreationTimeUtc(directory) < TimeSpan.FromMinutes(5))
                            continue;
                        Directory.Delete(directory, recursive: true);
                        continue;
                    }

                    using (new FileStream(lockPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        // Successfully acquiring the lock proves that no live private window owns it.
                    }
                    Directory.Delete(directory, recursive: true);
                }
                catch (IOException)
                {
                    // The profile is active or a WebView2 process still holds a file.
                }
                catch (UnauthorizedAccessException)
                {
                    // Leave inaccessible data untouched; a later startup may clean it.
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            WebViewEnvironment.Forget(DirectoryPath);
            _activeLock.Dispose();
            try
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
            catch (IOException)
            {
                // WebView2 may release files asynchronously; next startup retries cleanup.
            }
            catch (UnauthorizedAccessException)
            {
                // Never escalate cleanup into deletion outside this exact profile directory.
            }
        }
    }
}
