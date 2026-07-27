using System;
using System.Threading;
using System.Threading.Tasks;
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
    public async Task AnotherThreadWritingAnAtomIsToldWhereToPostIt()
    {
        var value = new LocalAtom<int>(0);

        using var drawing = FrameThread.Claim();

        var failure = await Task.Run(() => Assert.Throws<InvalidOperationException>(() => value.Value = 1));

        Assert.Contains("UiDispatcher.Post", failure.Message, StringComparison.Ordinal);
        Assert.Equal(0, value.Value);
    }

    [Fact]
    public async Task AnotherThreadNavigatingIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var failure = await Task.Run(() =>
            Assert.Throws<InvalidOperationException>(() => app.Navigator.Apply(ViewKind.Other)));

        Assert.Contains("Navigator.Apply", failure.Message, StringComparison.Ordinal);
        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
    }

    [Fact]
    public async Task AnotherThreadWritingTheOutputRowIsToldTheSame()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var failure = await Task.Run(() =>
            Assert.Throws<InvalidOperationException>(() => app.State.Output = "from the background"));

        Assert.Contains("ArlecchinoState.Output", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void PostingFromAnotherThreadIsTheWayThrough()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<int>(0);

        using var drawing = FrameThread.Claim();
        var posted = new Thread(() => app.Dispatcher.Post(() => value.Value = 7));

        posted.Start();
        posted.Join();

        app.Dispatcher.RunPending(static _ => { });

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
}
