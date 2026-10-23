using System;
using System.IO;
using FlaUI.Core;
using FlaUI.UIA3;
using Xunit;

namespace MadagascarBrowser.UiTests;

public sealed class ApplicationLaunchTests
{
    [Fact]
    public void BrowserStartsAndExposesNamedTopLevelWindow()
    {
        var executable = Environment.GetEnvironmentVariable("MADAGASCARBROWSER_EXE");
        Assert.False(string.IsNullOrWhiteSpace(executable), "MADAGASCARBROWSER_EXE must point to the Release WPF executable.");
        Assert.True(File.Exists(executable), $"Browser executable not found: {executable}");
        using var application = Application.Launch(executable!);
        using var automation = new UIA3Automation();
        var window = application.GetMainWindow(automation, TimeSpan.FromSeconds(45));
        Assert.NotNull(window);
        Assert.Contains("Madagascar Browser", window!.Title, StringComparison.OrdinalIgnoreCase);
    }
}
