using MadagascarBrowser.Core.Data;
using MadagascarBrowser.Shell;
using Xunit;

namespace MadagascarBrowser.Shell.Tests;

public class ThemeManagerTests
{
    [Fact]
    public void Constants_AreDefined()
    {
        Assert.Equal("dark", ThemeManager.ThemeDark);
        Assert.Equal("light", ThemeManager.ThemeLight);
        Assert.Equal("#FF4438", ThemeManager.MalagasyAccent);
        Assert.Equal("#19B36B", ThemeManager.GreenAccent);
        Assert.Equal("#F0A63C", ThemeManager.OrangeAccent);
    }

    [Fact]
    public void Apply_DoesNotCrashWhenApplicationIsNull()
    {
        // Application.Current est null hors d'un contexte WPF : Apply doit rester silencieux.
        var settings = new BrowserSettings { Theme = "light", Accent = "vert", Density = "compact" };
        ThemeManager.Apply(settings);
    }

    [Fact]
    public void ApplyTheme_DoesNotCrashWhenApplicationIsNull()
    {
        ThemeManager.ApplyTheme("light");
        ThemeManager.ApplyTheme("dark");
        ThemeManager.ApplyTheme(null);
    }

    [Fact]
    public void ApplyAccent_DoesNotCrashWhenApplicationIsNull()
    {
        ThemeManager.ApplyAccent("vert");
        ThemeManager.ApplyAccent("orange");
        ThemeManager.ApplyAccent("malagasy");
        ThemeManager.ApplyAccent(null);
    }

    [Fact]
    public void ApplyDensity_DoesNotCrashWhenApplicationIsNull()
    {
        ThemeManager.ApplyDensity("comfortable");
        ThemeManager.ApplyDensity("compact");
        ThemeManager.ApplyDensity(null);
    }
}