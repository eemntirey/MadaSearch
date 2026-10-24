using MadagascarBrowser.Core.Startup;

namespace MadagascarBrowser.Tests;

public class CommandLineOptionsTests
{
    [Fact]
    public void Parse_NoArguments_ReturnsDefaults()
    {
        var options = CommandLineOptions.Parse(Array.Empty<string>());

        Assert.False(options.IsPrivate);
        Assert.Null(options.DataRoot);
    }

    [Fact]
    public void Parse_PrivateFlag_EnablesPrivateWindow()
    {
        var options = CommandLineOptions.Parse(new[] { "--private" });

        Assert.True(options.IsPrivate);
    }

    [Fact]
    public void Parse_DataRootWithSeparateValue_IsRead()
    {
        var options = CommandLineOptions.Parse(new[] { "--data-root", @"C:\Temp\e2e" });

        Assert.Equal(@"C:\Temp\e2e", options.DataRoot);
    }

    [Fact]
    public void Parse_DataRootWithEquals_IsRead()
    {
        var options = CommandLineOptions.Parse(new[] { "--data-root=C:\\Temp\\e2e" });

        Assert.Equal(@"C:\Temp\e2e", options.DataRoot);
    }

    [Fact]
    public void Parse_DataRootWithoutValue_IsIgnored()
    {
        var options = CommandLineOptions.Parse(new[] { "--data-root" });

        Assert.Null(options.DataRoot);
    }
}
