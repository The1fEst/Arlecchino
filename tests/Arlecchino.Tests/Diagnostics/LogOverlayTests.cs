using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Arlecchino.Diagnostics;
using Arlecchino.Tests.Views;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Diagnostics;

public sealed class LogOverlayTests
{
    [Fact]
    public void TheOverlayIsHiddenUntilItsKeyIsPressed()
    {
        using var app = new TestApplication();
        Log(app, LogLevel.Information, "hello from the log");

        Assert.DoesNotContain("hello from the log", app.Frame(), StringComparison.Ordinal);

        app.Press(ConsoleKey.L, control: true);
        Assert.Contains("hello from the log", app.Frame(), StringComparison.Ordinal);

        app.Press(ConsoleKey.L, control: true);
        Assert.DoesNotContain("hello from the log", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void FrameworkFailuresEndUpInTheLogInsteadOfOnTheScreen()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Breaking);
        app.Frame();

        app.Press(ConsoleKey.L, control: true);
        Assert.Contains("fail", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void ScrollingBackShowsOlderLinesAndEndReturnsToTheNewest()
    {
        using var app = new TestApplication();

        for (var i = 0; i < 60; i++)
        {
            Log(app, LogLevel.Information, $"line {i}");
        }

        app.Press(ConsoleKey.L, control: true);
        Assert.Contains("line 59", app.Frame(), StringComparison.Ordinal);

        for (var press = 0; press < 30; press++)
        {
            app.Press(ConsoleKey.UpArrow);
        }

        Assert.DoesNotContain("line 59", app.Frame(), StringComparison.Ordinal);

        app.Press(ConsoleKey.End);
        Assert.Contains("line 59", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void BackspaceEmptiesTheBuffer()
    {
        using var app = new TestApplication();
        Log(app, LogLevel.Warning, "something odd");

        app.Press(ConsoleKey.L, control: true);
        app.Press(ConsoleKey.Backspace);

        Assert.Empty(app.Services.GetRequiredService<LogBuffer>().Snapshot());
        Assert.Contains(app.Options.Strings.LogEmpty(), app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheBufferKeepsOnlyItsMostRecentLines()
    {
        using var app = new TestApplication();
        var buffer = app.Services.GetRequiredService<LogBuffer>();
        buffer.Capacity = 3;

        for (var i = 0; i < 10; i++)
        {
            Log(app, LogLevel.Information, $"line {i}");
        }

        var held = buffer.Snapshot();
        Assert.Equal(3, held.Count);
        Assert.Equal("line 9", held[^1].Message);
    }

    [Fact]
    public void LinesLoggedFromManyThreadsAreAllKept()
    {
        using var app = new TestApplication();
        var buffer = app.Services.GetRequiredService<LogBuffer>();
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Tests.Threads");

        Parallel.For(0, 500, index => logger.LogInformation("{Text}", $"line {index}"));

        Assert.Equal(200, buffer.Snapshot().Count);
    }

    private static void Log(TestApplication app, LogLevel level, string message) =>
        app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Tests.Probe").Log(level, "{Text}", message);
}
