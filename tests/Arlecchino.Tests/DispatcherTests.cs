using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Arlecchino.Tests;

public sealed class DispatcherTests
{
    [Fact]
    public void PostedActionRunsBeforeTheNextFrame()
    {
        using var app = new TestApplication();

        app.Dispatcher.Post(() => app.State.Output = "loaded");

        Assert.Equal("", app.State.Output);

        app.Frame();

        Assert.Equal("loaded", app.State.Output);
        Assert.False(app.Dispatcher.HasPending);
    }

    [Fact]
    public void PostingRequestsARepaint()
    {
        using var app = new TestApplication();
        app.Repaint.TakeRequested();

        app.Dispatcher.Post(static () => { });

        Assert.True(app.Repaint.IsRequested);
    }

    [Fact]
    public void ActionsRunInTheOrderTheyWerePosted()
    {
        using var app = new TestApplication();
        var order = new List<int>();

        app.Dispatcher.Post(() => order.Add(1));
        app.Dispatcher.Post(() => order.Add(2));
        app.Dispatcher.Post(() => order.Add(3));

        app.Frame();

        Assert.Equal([1, 2, 3], order);
    }

    [Fact]
    public async Task ActionsPostedFromAnotherThreadAreRunOnTheFrame()
    {
        using var app = new TestApplication();
        var posted = 0;

        await Task.WhenAll(
            Task.Run(() => app.Dispatcher.Post(() => posted++)),
            Task.Run(() => app.Dispatcher.Post(() => posted++)),
            Task.Run(() => app.Dispatcher.Post(() => posted++)));

        app.Frame();

        Assert.Equal(3, posted);
    }

    [Fact]
    public void FailingActionIsReportedAndTheRestStillRun()
    {
        using var app = new TestApplication();
        var ran = false;

        app.Dispatcher.Post(static () => throw new InvalidOperationException("background failed"));
        app.Dispatcher.Post(() => ran = true);

        var frame = app.Frame();

        Assert.True(ran);
        Assert.Contains("background failed", frame, StringComparison.Ordinal);
    }
}
