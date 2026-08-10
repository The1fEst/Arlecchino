using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Arlecchino.Tests.Hosting;

/// <summary>
/// Lending the terminal to another program: what is given back before it runs, what is taken again
/// afterward, and what the thread reading keys is told while it lasts.
/// </summary>
public sealed class HandoverTests : IDisposable
{
    private readonly ArlecchinoTestHost _host = new(40, 10);

    public void Dispose() => _host.Dispose();

    private Handover Lending => _host.Services.GetRequiredService<Handover>();

    private TerminalModes Modes => _host.Services.GetRequiredService<TerminalModes>();

    [Fact]
    public void TheTerminalIsGivenBackWhileTheWorkRuns()
    {
        Modes.Enter();

        var wasFullScreen = true;
        var wasMouse = true;

        Lending.Give(() =>
        {
            wasFullScreen = _host.Terminal.IsFullScreen;
            wasMouse = _host.Terminal.IsMouseEnabled;
        });

        Assert.False(wasFullScreen);
        Assert.False(wasMouse);
        Assert.True(_host.Terminal.IsFullScreen);
    }

    /// <summary>
    /// What is taken back is what was in force. Nothing here ever took the terminal over — no loop is
    /// running — so nothing is switched on behind the work either.
    /// </summary>
    [Fact]
    public void ATerminalThatWasNeverTakenOverIsNotTakenOverAfterward()
    {
        Lending.Give(static () => { });

        Assert.False(_host.Terminal.IsFullScreen);
        Assert.False(_host.Terminal.IsMouseEnabled);
    }

    /// <summary>
    /// The surface only knows what it drew itself, so the frame after the terminal comes back has to be
    /// drawn whole over whatever the other program left on the screen.
    /// </summary>
    [Fact]
    public void TheNextFrameIsAskedForWhole()
    {
        _host.Frame();
        _host.Repaint.TakeRequested();

        Lending.Give(static () => { });

        Assert.True(_host.Repaint.TakeRequested());
    }

    /// <summary>
    /// The reader is told to stop before the work starts and told it may read again afterward. A key
    /// read while an editor is on the screen is a key the editor never receives.
    /// </summary>
    [Fact]
    public void TheReaderIsParkedWhileTheTerminalIsSomebodyElses()
    {
        var readWhileAway = true;

        Assert.True(Lending.MayRead());

        Lending.Give(() => readWhileAway = Lending.MayRead());

        Assert.False(readWhileAway);
        Assert.False(Lending.IsAway);
        Assert.True(Lending.MayRead());
    }

    /// <summary>
    /// Type-ahead belongs to the program that has just ended, not to the screen coming back. Anything
    /// waiting when the terminal is handed back is thrown away rather than replayed as keys nobody
    /// pressed at this screen.
    /// </summary>
    [Fact]
    public void WhatWasWaitingWhenItEndedIsThrownAway()
    {
        Lending.Give(() => _host.Terminal.Enqueue(new(ConsoleKey.Q, KeyModifiers.None, 'q')));

        Assert.False(_host.Terminal.KeyAvailable);
    }

    /// <summary>
    /// An error in the work is the caller's to answer for, but the terminal is not: it comes back before
    /// the error does, or the message about it would be printed to a screen nobody can see.
    /// </summary>
    [Fact]
    public void TheTerminalComesBackEvenWhenTheWorkThrows()
    {
        Modes.Enter();

        Assert.Throws<InvalidOperationException>(() => Lending.Give(static () => throw new InvalidOperationException()));

        Assert.True(_host.Terminal.IsFullScreen);
        Assert.False(Lending.IsAway);
    }

    /// <summary>Nothing can be started from an empty name, and the terminal survives finding that out.</summary>
    [Fact]
    public void AProgramThatCannotBeStartedLeavesTheScreenUsable()
    {
        Modes.Enter();

        Assert.ThrowsAny<Exception>(() => Lending.Run(new("arlecchino-no-such-program")));

        Assert.True(_host.Terminal.IsFullScreen);
        Assert.False(Lending.IsAway);
    }
}
