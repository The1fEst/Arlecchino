using System;
using System.Collections.Generic;
using System.Threading;
using Arlecchino.Atoms;
using Xunit;

namespace Arlecchino.Tests;

public sealed class AtomsQueueTests
{
    [Fact]
    public void WhatJoinsTheBackLeavesFromTheFront()
    {
        var waiting = new LocalAtomsQueue<string>();

        waiting.Enqueue("first");
        waiting.Enqueue("second");
        waiting.Enqueue("third");

        Assert.Equal(["first", "second", "third"], waiting.Value);
        Assert.Equal("first", waiting.Peek());
        Assert.Equal("first", waiting.Dequeue());
        Assert.Equal("second", waiting.Dequeue());
        Assert.Equal(1, waiting.Count);
        Assert.False(waiting.IsEmpty);
    }

    [Fact]
    public void SeveralJoiningAtOnceIsOneChange()
    {
        var waiting = new LocalAtomsQueue<int>([1]);
        var heard = 0;

        using var subscription = waiting.Subscribe(() => heard++);

        waiting.Enqueue([2, 3, 4]);

        Assert.Equal(1, heard);
        Assert.Equal([1, 2, 3, 4], waiting.Value);

        waiting.Enqueue([]);

        Assert.Equal(1, heard);
    }

    [Fact]
    public void AnEmptyQueueAnswersRatherThanThrowsWhenAskedNicely()
    {
        var waiting = new LocalAtomsQueue<string>();

        Assert.True(waiting.IsEmpty);
        Assert.False(waiting.TryDequeue(out _));
        Assert.False(waiting.TryPeek(out _));
        Assert.Throws<InvalidOperationException>(waiting.Dequeue);
        Assert.Throws<InvalidOperationException>(waiting.Peek);

        waiting.Enqueue("only");

        Assert.True(waiting.TryPeek(out var peeked));
        Assert.Equal("only", peeked);
        Assert.True(waiting.TryDequeue(out var taken));
        Assert.Equal("only", taken);
        Assert.True(waiting.IsEmpty);
    }

    [Fact]
    public void AChangeAsksForAFrame()
    {
        using var repaint = new Repaint();
        var waiting = new LocalAtomsQueue<string>();

        repaint.TakeRequested();

        waiting.Enqueue("work");

        Assert.True(repaint.IsRequested);
    }

    [Fact]
    public void WhatWasTakenGoesBackToTheFront()
    {
        using var history = new AtomHistory();
        var waiting = new TrackedAtomsQueue<string>(["first", "second"]);

        var taken = waiting.Dequeue();

        Assert.Equal("first", taken);
        Assert.Equal(["second"], waiting.Value);

        history.Undo();

        Assert.Equal(["first", "second"], waiting.Value);

        history.Redo();

        Assert.Equal(["second"], waiting.Value);
    }

    [Fact]
    public void ALocalQueueStaysOffTheHistory()
    {
        using var history = new AtomHistory();
        var waiting = new LocalAtomsQueue<string>();

        waiting.Enqueue("work");
        waiting.Dequeue();

        Assert.False(history.CanUndo);
    }

    [Fact]
    public void ARebuiltQueueIsOneStep()
    {
        using var history = new AtomHistory();
        var waiting = new TrackedAtomsQueue<int>([1, 2]);

        waiting.Reset([9]);

        Assert.Equal(1, history.Depth);

        waiting.Reset([9]);

        Assert.Equal(1, history.Depth);

        history.Undo();

        Assert.Equal([1, 2], waiting.Value);
    }

    [Fact]
    public void DroppingWhatIsWaitingIsOneStep()
    {
        using var history = new AtomHistory();
        var waiting = new TrackedAtomsQueue<string>(["first", "second"]);

        waiting.Clear();

        Assert.True(waiting.IsEmpty);
        Assert.Equal(1, history.Depth);

        waiting.Clear();

        Assert.Equal(1, history.Depth);

        history.Undo();

        Assert.Equal(["first", "second"], waiting.Value);
    }

    [Fact]
    public void ItIsWalkedFrontFirst()
    {
        var waiting = new LocalAtomsQueue<string>(["first", "second"]);
        var walked = new List<string>();

        foreach (var item in waiting)
        {
            walked.Add(item);
        }

        Assert.Equal(["first", "second"], walked);
    }

    [Fact]
    public void ADerivedValueFollowsIt()
    {
        var waiting = new LocalAtomsQueue<string>();
        var next = new Computed<string>(() => waiting.TryPeek(out var item) ? item : "idle");

        Assert.Equal("idle", next.Value);

        waiting.Enqueue("work");

        Assert.Equal("work", next.Value);
    }

    [Fact]
    public void ItRefusesToBeChangedFromAnotherThread()
    {
        using var app = new TestApplication();
        var waiting = new LocalAtomsQueue<string>();

        using var drawing = FrameThread.Claim();

        Exception? thrown = null;
        var changing = new Thread(() =>
        {
            try
            {
                waiting.Enqueue("work");
            }
            catch (Exception exception)
            {
                thrown = exception;
            }
        });

        changing.Start();
        changing.Join();

        Assert.IsType<InvalidOperationException>(thrown);
        Assert.Empty(waiting.Value);
    }
}
