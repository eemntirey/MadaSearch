using MadagascarBrowser.Core.Data;

namespace MadagascarBrowser.Tests;

public class AsyncDebouncerTests
{
    [Fact]
    public async Task Debounce_RapidCalls_ExecutesOnlyOnce()
    {
        var counter = 0;
        using var debouncer = new AsyncDebouncer(_ =>
        {
            Interlocked.Increment(ref counter);
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(60));

        debouncer.Debounce();
        debouncer.Debounce();
        debouncer.Debounce();

        await Task.Delay(250);

        Assert.Equal(1, counter);
    }

    [Fact]
    public async Task Debounce_SingleCall_ExecutesAfterDelay()
    {
        var executed = false;
        using var debouncer = new AsyncDebouncer(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(50));

        debouncer.Debounce();

        Assert.False(executed);
        await Task.Delay(200);
        Assert.True(executed);
    }

    [Fact]
    public async Task Debounce_SeparatedCalls_ExecuteTwice()
    {
        var counter = 0;
        using var debouncer = new AsyncDebouncer(_ =>
        {
            Interlocked.Increment(ref counter);
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(40));

        debouncer.Debounce();
        await Task.Delay(180);
        debouncer.Debounce();
        await Task.Delay(180);

        Assert.Equal(2, counter);
    }
}
