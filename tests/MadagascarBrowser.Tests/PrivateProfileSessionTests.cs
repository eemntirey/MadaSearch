using System;
using System.IO;
using MadagascarBrowser.Shell;

namespace MadagascarBrowser.Tests;

public sealed class PrivateProfileSessionTests
{
    [Fact]
    public void SessionsUseDistinctProfileFoldersAndNormalCloseDeletesTheProfile()
    {
        var first = new PrivateProfileSession();
        var second = new PrivateProfileSession();
        var firstPath = first.DirectoryPath;
        var secondPath = second.DirectoryPath;

        try
        {
            Assert.NotEqual(firstPath, secondPath);
            Assert.True(Directory.Exists(firstPath));
            Assert.True(Directory.Exists(secondPath));

            first.Dispose();

            Assert.False(Directory.Exists(firstPath));
            Assert.True(Directory.Exists(secondPath));
        }
        finally
        {
            first.Dispose();
            second.Dispose();
        }
    }

    [Fact]
    public void CleanupOrphanedProfiles_RemovesUnlockedProfileAndKeepsActiveProfile()
    {
        using var active = new PrivateProfileSession();
        var root = Path.GetDirectoryName(active.DirectoryPath)!;
        var orphanPath = Path.Combine(root, "orphan-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(orphanPath);
        using (File.Create(Path.Combine(orphanPath, ".active.lock"))) { }
        Directory.SetCreationTimeUtc(orphanPath, DateTime.UtcNow.AddMinutes(-10));

        try
        {
            PrivateProfileSession.CleanupOrphanedProfiles();

            Assert.False(Directory.Exists(orphanPath));
            Assert.True(Directory.Exists(active.DirectoryPath));
        }
        finally
        {
            if (Directory.Exists(orphanPath))
                Directory.Delete(orphanPath, recursive: true);
        }
    }
}
