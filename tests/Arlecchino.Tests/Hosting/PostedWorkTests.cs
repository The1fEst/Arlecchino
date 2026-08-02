using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Hosting;

public sealed class PostedWorkTests
{
    [Fact]
    public void PostedActionRunsBeforeTheNextFrame()
    {
        using var app = new TestApplication();

        FrameThread.Post(() => app.State.Output = "loaded");

        Assert.Equal("", app.State.Output);

        app.Frame();

        Assert.Equal("loaded", app.State.Output);
        Assert.False(FrameThread.HasPending);
    }

    [Fact]
    public void PostingRequestsARepaint()
    {
        using var app = new TestApplication();
        app.Repaint.TakeRequested();

        using var drawing = FrameThread.Claim(app.Repaint.Request);

        FrameThread.Post(static () => { });

        Assert.True(app.Repaint.IsRequested);
    }

    [Fact]
    public void WorkPostedToAFrameLoopThatStopsIsDropped()
    {
        using var app = new TestApplication();
        var ran = false;

        using (FrameThread.Claim())
        {
            FrameThread.Post(() => ran = true);
        }

        app.Frame();

        Assert.False(ran);
        Assert.False(FrameThread.HasPending);
    }

    [Fact]
    public void ActionsRunInTheOrderTheyWerePosted()
    {
        using var app = new TestApplication();
        var order = new List<int>();

        FrameThread.Post(() => order.Add(1));
        FrameThread.Post(() => order.Add(2));
        FrameThread.Post(() => order.Add(3));

        app.Frame();

        Assert.Equal([1, 2, 3], order);
    }

    [Fact]
    public async Task ActionsPostedFromAnotherThreadAreRunOnTheFrame()
    {
        using var app = new TestApplication();
        var posted = 0;

        await Task.WhenAll(
            Task.Run(() => FrameThread.Post(() => posted++)),
            Task.Run(() => FrameThread.Post(() => posted++)),
            Task.Run(() => FrameThread.Post(() => posted++)));

        app.Frame();

        Assert.Equal(3, posted);
    }

    [Fact]
    public void FailingActionIsReportedAndTheRestStillRun()
    {
        using var app = new TestApplication();
        var ran = false;

        FrameThread.Post(static () => throw new InvalidOperationException("background failed"));
        FrameThread.Post(() => ran = true);

        var frame = app.Frame();

        Assert.True(ran);
        Assert.Contains("background failed", frame, StringComparison.Ordinal);
    }
}
