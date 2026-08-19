using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Arlecchino.Diagnostics;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Testing;
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

        app.Press(ConsoleKey.L, KeyModifiers.Control);
        Assert.Contains("hello from the log", app.Frame(), StringComparison.Ordinal);

        app.Press(ConsoleKey.L, KeyModifiers.Control);
        Assert.DoesNotContain("hello from the log", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void FrameworkFailuresEndUpInTheLogInsteadOfOnTheScreen()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Breaking);
        app.Frame();

        app.Press(ConsoleKey.L, KeyModifiers.Control);
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

        app.Press(ConsoleKey.L, KeyModifiers.Control);
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

        app.Press(ConsoleKey.L, KeyModifiers.Control);
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

        var overlay = buffer.Snapshot();
        Assert.Equal(3, overlay.Count);
        Assert.Equal("line 9", overlay[^1].Message);
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

    /// <summary>
    /// The overlay shows what a provider writes to the console. Told there is no provider, it says so
    /// rather than sitting empty for the life of the application.
    /// </summary>
    [Fact]
    public void WithNoProviderTheOverlaySaysHowToAddOne()
    {
        var strings = new ArlecchinoStrings();
        var terminal = new FakeTerminal(100, 20);
        var surface = new Surface(terminal);

        using var repaint = new Repaint();
        var overlay = new LogOverlay(new(repaint), repaint, providers: false);

        surface.StartFrame();
        new LogPaint(surface, strings).Draw(overlay);
        surface.Build();

        Assert.Contains(strings.LogWithoutProviders(), terminal.WrittenText, StringComparison.Ordinal);
        Assert.DoesNotContain(strings.LogEmpty(), terminal.WrittenText, StringComparison.Ordinal);
    }

    private static void Log(TestApplication app, LogLevel level, string message) =>
        app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Tests.Probe").Log(level, "{Text}", message);
}
