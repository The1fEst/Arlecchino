using System;
using System.Collections.Generic;
using Arlecchino.Input;
using Arlecchino.Testing;
using Arlecchino.Tests.Views;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Rendering;

/// <summary>
/// A tape is only worth having if it plays the same way twice, so that is what these check first, and
/// then that the text it writes down is the text it reads back — the two claims everything else about
/// a tape rests on.
/// </summary>
public sealed class SessionTapeTests
{
    [Fact]
    public void TheSameTapePlayedTwiceDrawsTheSameFrames()
    {
        var tape = Session();

        var first = Play(tape);
        var second = Play(tape);

        Assert.Equal(6, first.Count);
        Assert.Equal(first, second);
    }

    [Fact]
    public void ATapeSurvivesBeingWrittenDownAndReadBack()
    {
        var tape = Session();

        Assert.Equal(Play(tape), Play(SessionTape.Read(tape.ToString())));
    }

    [Fact]
    public void TheClockOnTheTapeIsWhatTheFramesAreDrawnAgainst()
    {
        var tape = new SessionTape().Shot().Wait(100).Shot().Wait(600_000).Shot();

        var frames = Play(tape, static host => host.State.Output = "saved");

        Assert.Contains("saved", frames[0], StringComparison.Ordinal);
        Assert.Equal(frames[0], frames[1]);
        Assert.DoesNotContain("saved", frames[2], StringComparison.Ordinal);

        Assert.Equal(frames, Play(tape, static host => host.State.Output = "saved"));
    }

    [Fact]
    public void TwoDifferentTapesAreTwoDifferentSessions()
    {
        var typed = new SessionTape().Type(":pro").Shot();
        var other = new SessionTape().Type(":zzz").Shot();

        Assert.NotEqual(Play(typed), Play(other));
    }

    [Fact]
    public void EveryKindOfStepSurvivesTheRoundTrip()
    {
        var tape = new SessionTape()
            .Key(ConsoleKey.F1)
            .Type("hi")
            .Click(3, 5)
            .Scroll(4, 6, down: true)
            .Paste("pasted text with spaces")
            .Wait(250)
            .Shot();

        var written = tape.ToString();
        var reread = SessionTape.Read(written);

        Assert.Equal(8, tape.Count);
        Assert.Equal(tape.Count, reread.Count);
        Assert.Equal(written, reread.ToString());
    }

    [Fact]
    public void ATapeCapturedFromAClockKeepsTheGapsItWaited()
    {
        var clock = new TestClock();
        var tape = new SessionTape(clock);

        tape.RecordKey(new('a', ConsoleKey.A, false, false, false));
        clock.Advance(TimeSpan.FromMilliseconds(400));
        tape.RecordMouse(new(MouseAction.Pressed, MouseButton.Left, 2, 3, default));
        clock.Advance(TimeSpan.FromMilliseconds(150));
        tape.Shot();

        var written = tape.ToString();

        Assert.Contains("400 mouse Pressed Left 2 3", written, StringComparison.Ordinal);
        Assert.Contains("150 frame", written, StringComparison.Ordinal);
        Assert.Equal(written, SessionTape.Read(written).ToString());
    }

    [Fact]
    public void APasteOnTheTapeReachesTheApplication()
    {
        var frames = Play(new SessionTape().Type(":").Paste("copy").Shot());

        Assert.Single(frames);
    }

    [Fact]
    public void AWaitOnItsOwnIsNotAFrame()
    {
        var frames = Play(new SessionTape().Wait(50).Wait(50).Shot());

        Assert.Single(frames);
    }

    private static SessionTape Session() =>
        new SessionTape()
            .Shot()
            .Key(ConsoleKey.F1)
            .Shot()
            .Key(ConsoleKey.Escape)
            .Type(":")
            .Shot()
            .Type("pro")
            .Shot()
            .Wait(1200)
            .Shot()
            .Key(ConsoleKey.Escape)
            .Shot();

    private static List<string> Play(SessionTape tape, Action<ArlecchinoTestHost>? before = null)
    {
        using var host = new ArlecchinoTestHost(configure: static builder =>
            builder.AddGeneratedViews().StartAt(ViewKind.Probe).AddCommand<ProbeCommand>());

        before?.Invoke(host);

        return tape.Play(host);
    }
}
