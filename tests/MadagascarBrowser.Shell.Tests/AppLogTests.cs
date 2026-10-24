using System;
using System.IO;
using MadagascarBrowser.Shell;

namespace MadagascarBrowser.Shell.Tests;

public class AppLogTests
{
    [Fact]
    public void EnableFileSink_EcritDansLeFichierLocal()
    {
        var directory = Path.Combine(Path.GetTempPath(), "madagascar-browser-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, "app.log");

        try
        {
            AppLog.EnableFileSink(filePath);
            AppLog.Info("message de test");

            Assert.True(File.Exists(filePath));
            var content = File.ReadAllText(filePath);
            Assert.Contains("message de test", content);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}