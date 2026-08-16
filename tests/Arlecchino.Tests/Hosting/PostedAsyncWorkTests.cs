using System;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Tests.Support;
using Xunit;

namespace Arlecchino.Tests.Hosting;

/// <summary>
/// Asynchronous work handed to the drawing thread. It reads and changes what a frame draws, so it comes
/// back to that thread after every wait, and its failures reach the frame loop.
/// </summary>
public sealed class PostedAsyncWorkTests
{
    private const int Attempts = 200;

    [Fact]
    public void ItStartsOnTheDrawingThread()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim(app.Repaint.Request);
        var started = false;

        FrameThread.Post(() =>
        {
            started = FrameThread.IsCurrent;

            return Task.CompletedTask;
        });

        app.Frame();

        Assert.True(started);
    }

    [Fact]
    public void ItComesBackToTheDrawingThreadAfterAWait()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim(app.Repaint.Request);
        var thread = 0;
        var after = 0;

        FrameThread.Post(async () =>
        {
            thread = Environment.CurrentManagedThreadId;

            await Task.Delay(1);

            after = Environment.CurrentManagedThreadId;
            app.State.Output = "back";
        });

        Until(app, () => app.State.Output.Length > 0);

        Assert.Equal("back", app.State.Output);
        Assert.Equal(thread, after);
    }

    /// <summary>
    /// The atoms a view is built on refuse to be set from another thread, so coming back means the state
    /// changes rather than merely that the thread numbers match.
    /// </summary>
    [Fact]
    public void WhatItWritesAfterAWaitIsAccepted()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim(app.Repaint.Request);

        FrameThread.Post(async () =>
        {
            await Task.Delay(1);

            FrameThread.Verify(nameof(WhatItWritesAfterAWaitIsAccepted));
            app.State.Output = "written";
        });

        Until(app, () => app.State.Output.Length > 0);

        Assert.Equal("written", app.State.Output);
    }

    [Fact]
    public void FailingWithoutWaitingIsReportedLikeAPostedAction()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim(app.Repaint.Request);

        FrameThread.Post(static () => Task.FromException(new InvalidOperationException("nothing doing")));

        app.Frame();

        Assert.Contains("nothing doing", app.State.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void FailingAfterAWaitIsReportedToo()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim(app.Repaint.Request);

        FrameThread.Post(async () =>
        {
            await Task.Delay(1);

            throw new InvalidOperationException("late failure");
        });

        Until(app, () => app.State.Output.Length > 0);

        Assert.Contains("late failure", app.State.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void BeingCanceledIsNotAFailure()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim(app.Repaint.Request);
        using var cancelling = new CancellationTokenSource();
        using var ended = new ManualResetEventSlim();

        FrameThread.Post(async () =>
        {
            try
            {
                await Task.Delay(Timeout.Infinite, cancelling.Token);
            }
            finally
            {
                ended.Set();
            }
        });

        app.Frame();
        cancelling.Cancel();

        Until(app, () => ended.IsSet);

        Assert.Equal("", app.State.Output);
    }

    private static void Until(TestApplication app, Func<bool> done)
    {
        for (var attempt = 0; attempt < Attempts && !done(); attempt++)
        {
            app.Frame();
            Thread.Sleep(5);
        }

        app.Frame();
    }
}
