using System;
using System.Threading;
using Arlecchino.Atoms;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests;

public sealed class FrameThreadTests
{
    [Fact]
    public void NothingIsCheckedUntilSomeoneIsDrawing()
    {
        var value = new LocalAtom<int>(0);

        value.Value = 1;

        Assert.True(FrameThread.IsCurrent);
        Assert.Equal(1, value.Value);
    }

    [Fact]
    public void TheThreadThatClaimedDrawingMayWrite()
    {
        var value = new LocalAtom<int>(0);

        using var drawing = FrameThread.Claim();

        value.Value = 1;

        Assert.Equal(1, value.Value);
    }

    [Fact]
    public void AnotherThreadWritingAnAtomIsToldWhereToPostIt()
    {
        var value = new LocalAtom<int>(0);

        using var drawing = FrameThread.Claim();

        var failure = Refused(() => value.Value = 1);

        Assert.Contains("FrameThread.Post", failure.Message, StringComparison.Ordinal);
        Assert.Equal(0, value.Value);
    }

    [Fact]
    public void AnotherThreadNavigatingIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var failure = Refused(() => app.Navigator.Apply(ViewKind.Other));

        Assert.Contains("Navigator.Apply", failure.Message, StringComparison.Ordinal);
        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void AnotherThreadWritingTheOutputRowIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var failure = Refused(() => app.State.Output = "from the background");

        Assert.Contains("ArlecchinoState.Output", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void PostingFromAnotherThreadIsTheWayThrough()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<int>(0);

        using var drawing = FrameThread.Claim();
        var posted = new Thread(() => FrameThread.Post(() => value.Value = 7));

        posted.Start();
        posted.Join();

        FrameThread.RunPending(static _ => { });

        Assert.Equal(7, value.Value);
    }

    [Fact]
    public void GivingTheClaimUpLetsAnyThreadWriteAgain()
    {
        var value = new LocalAtom<int>(0);

        using (FrameThread.Claim())
        {
        }

        value.Value = 3;

        Assert.Equal(3, value.Value);
    }

    private static InvalidOperationException Refused(Action write)
    {
        Exception? thrown = null;

        var writing = new Thread(() =>
        {
            try
            {
                write();
            }
            catch (Exception exception)
            {
                thrown = exception;
            }
        });

        writing.Start();
        writing.Join();

        return Assert.IsType<InvalidOperationException>(thrown);
    }
}
