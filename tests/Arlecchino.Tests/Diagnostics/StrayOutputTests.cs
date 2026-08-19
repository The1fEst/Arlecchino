using System;
using System.IO;
using Arlecchino.Diagnostics;
using Arlecchino.Hosting;
using Arlecchino.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arlecchino.Tests.Diagnostics;

/// <summary>
/// What the console is taken over for. The writers are built here rather than read off the console, so a
/// test can say where text went without the process losing its own output while it runs.
/// </summary>
public sealed class StrayOutputTests
{
    [Fact]
    public void TextWrittenBeforeAFrameGoesToTheConsole()
    {
        using var log = new LogScope();
        var console = new StringWriter();
        var writer = Writer(log.Strays, console);

        writer.Write("nothing has the screen yet\n");

        Assert.Equal("nothing has the screen yet\n", console.ToString());
        Assert.Empty(log.Buffer.Snapshot());
    }

    [Fact]
    public void TextWrittenWhileAFrameIsUpIsCaughtInstead()
    {
        using var log = new LogScope();
        var console = new StringWriter();
        var writer = Writer(log.Strays, console);

        log.Strays.Hold();
        writer.Write("Application started.\n");
        log.Strays.Release();

        Assert.Equal(string.Empty, console.ToString());

        var entry = Assert.Single(log.Buffer.Snapshot());

        Assert.Equal("stdout", entry.Category);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Equal("Application started.", entry.Message);
    }

    /// <summary>
    /// A logging provider colors what it writes. Drawn into the overlay as it stands, the sequences would
    /// go back out to the terminal and move the frame — which is the very thing being caught.
    /// </summary>
    [Fact]
    public void EscapeSequencesAreTakenOutOfWhatIsCaught()
    {
        using var log = new LogScope();
        var console = new StringWriter();
        var writer = Writer(log.Strays, console);

        log.Strays.Hold();
        writer.Write("\e[40m\e[32minfo\e[39m\e[22m\e[49m: Hosting\e]0;a title\a\n");
        log.Strays.Release();

        Assert.Equal("info: Hosting", Assert.Single(log.Buffer.Snapshot()).Message);
    }

    [Fact]
    public void EachLineIsCaughtOnItsOwn()
    {
        using var log = new LogScope();
        var console = new StringWriter();
        var writer = Writer(log.Strays, console);

        log.Strays.Hold();
        writer.Write("info: Lifetime[0]\r\n      Hosting environment: Production\r\n");
        log.Strays.Release();

        var entries = log.Buffer.Snapshot();

        Assert.Equal(2, entries.Count);
        Assert.Equal("info: Lifetime[0]", entries[0].Message);
        Assert.Equal("      Hosting environment: Production", entries[1].Message);
    }

    [Fact]
    public void ALineLeftWithoutItsNewlineIsCaughtWhenTheConsoleIsGivenBack()
    {
        using var log = new LogScope();
        var console = new StringWriter();
        var writer = Writer(log.Strays, console);

        log.Strays.Hold();
        writer.Write("halfway through");
        log.Strays.Release();

        Assert.Equal("halfway through", Assert.Single(log.Buffer.Snapshot()).Message);
    }

    [Fact]
    public void TheConsoleIsWrittenToAgainOnceItIsGivenBack()
    {
        using var log = new LogScope();
        var console = new StringWriter();
        var writer = Writer(log.Strays, console);

        log.Strays.Hold();
        log.Strays.Release();
        writer.Write("Application is shutting down...\n");

        Assert.Equal("Application is shutting down...\n", console.ToString());
        Assert.Empty(log.Buffer.Snapshot());
    }

    [Fact]
    public void WhatTheErrorStreamSaysIsWorthAWarning()
    {
        using var log = new LogScope();
        var console = new StringWriter();
        var writer = new StrayWriter(log.Strays, console, "stderr", LogLevel.Warning);

        log.Strays.Hold();
        writer.Write("something went wrong\n");
        log.Strays.Release();

        var entry = Assert.Single(log.Buffer.Snapshot());

        Assert.Equal("stderr", entry.Category);
        Assert.Equal(LogLevel.Warning, entry.Level);
    }

    [Fact]
    public void TheConsoleIsTakenOverOnlyOnce() =>
        Assert.Same(StrayOutput.TakeOverTheConsole(), StrayOutput.TakeOverTheConsole());

    [Fact]
    public void CatchingLastsExactlyAsLongAsTheTerminalIsHeld()
    {
        using var log = new LogScope();
        var modes = new TerminalModes(new FakeTerminal(20, 5), new(), log.Strays);

        Assert.False(log.Strays.Holding);

        modes.Enter();
        Assert.True(log.Strays.Holding);

        modes.Leave();
        Assert.False(log.Strays.Holding);
    }

    private static StrayWriter Writer(StrayOutput strays, TextWriter console) =>
        new(strays, console, "stdout", LogLevel.Information);

    /// <summary>
    /// A log of its own for one test, pointed at by whatever holds the console, and the console given
    /// back afterward, so the next test starts with text passing through again.
    /// </summary>
    private sealed class LogScope : IDisposable
    {
        private readonly Repaint _repaint = new();

        public LogScope()
        {
            Strays = StrayOutput.TakeOverTheConsole();
            Buffer = new(_repaint);

            Strays.SendTo(Buffer, TimeProvider.System);
        }

        public StrayOutput Strays { get; }

        public LogBuffer Buffer { get; }

        public void Dispose()
        {
            Strays.Release();
            _repaint.Dispose();
        }
    }
}
