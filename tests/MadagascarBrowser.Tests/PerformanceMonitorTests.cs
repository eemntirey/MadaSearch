using MadagascarBrowser.Core.Performance;
using System;

namespace MadagascarBrowser.Tests;

public class PerformanceMonitorTests
{
    [Fact]
    public void PerformanceMonitor_StartupMeasured_InitiallyFalse()
    {
        var monitor = new PerformanceMonitor();
        Assert.False(monitor.StartupMeasured);
        Assert.Equal(0, monitor.StartupTimeMs);
    }

    [Fact]
    public void PerformanceMonitor_StartTabOpenMeasurement_RecordsTime()
    {
        var monitor = new PerformanceMonitor();
        monitor.StartTabOpenMeasurement();
        System.Threading.Thread.Sleep(10);
        monitor.CompleteTabOpenMeasurement();

        Assert.True(monitor.AverageTabOpenTimeMs >= 0);
        Assert.True(monitor.MeasuredTabOpenCount > 0);
        Assert.True(monitor.TotalTabsOpened > 0);
    }

    [Fact]
    public void PerformanceMonitor_RecordStartupComplete_MeasuresTime()
    {
        var monitor = new PerformanceMonitor();
        System.Threading.Thread.Sleep(5);
        monitor.RecordStartupComplete();

        Assert.True(monitor.StartupMeasured);
        Assert.True(monitor.StartupTimeMs >= 0);
    }

    [Fact]
    public void PerformanceMonitor_AverageTabOpenTimeMs_NoMeasurements_ReturnsZero()
    {
        var monitor = new PerformanceMonitor();
        Assert.Equal(0.0, monitor.AverageTabOpenTimeMs);
    }
}
