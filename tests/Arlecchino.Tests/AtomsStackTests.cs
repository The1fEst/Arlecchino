using System;
using System.Collections.Generic;
using Arlecchino.Atoms;
using Xunit;

namespace Arlecchino.Tests;

public sealed class AtomsStackTests
{
    [Fact]
    public void WhatGoesOnTopComesOffFirst()
    {
        var been = new LocalAtomsStack<string>();

        been.Push("first");
        been.Push("second");
        been.Push("third");

        Assert.Equal(["third", "second", "first"], been.Value);
        Assert.Equal("third", been.Peek());
        Assert.Equal("third", been.Pop());
        Assert.Equal("second", been.Pop());
        Assert.Equal(1, been.Count);
    }

    [Fact]
    public void SeveralGoingOnAtOnceLeaveTheLastOnTop()
    {
        var been = new LocalAtomsStack<int>([0]);
        var heard = 0;

        using var subscription = been.Subscribe(() => heard++);

        been.Push([1, 2, 3]);

        Assert.Equal(1, heard);
        Assert.Equal([3, 2, 1, 0], been.Value);
        Assert.Equal(3, been.Peek());

        been.Push([]);

        Assert.Equal(1, heard);
    }

    [Fact]
    public void AnEmptyStackAnswersRatherThanThrowsWhenAskedNicely()
    {
        var been = new LocalAtomsStack<string>();

        Assert.True(been.IsEmpty);
        Assert.False(been.TryPop(out _));
        Assert.False(been.TryPeek(out _));
        Assert.Throws<InvalidOperationException>(been.Pop);
        Assert.Throws<InvalidOperationException>(been.Peek);

        been.Push("only");

        Assert.True(been.TryPeek(out var peeked));
        Assert.Equal("only", peeked);
        Assert.True(been.TryPop(out var taken));
        Assert.Equal("only", taken);
        Assert.True(been.IsEmpty);
    }

    [Fact]
    public void AChangeAsksForAFrame()
    {
        using var repaint = new Repaint();
        var been = new LocalAtomsStack<string>();

        repaint.TakeRequested();

        been.Push("here");

        Assert.True(repaint.IsRequested);
    }

    [Fact]
    public void WhatWasTakenOffGoesBackOnTop()
    {
        using var history = new AtomHistory();
        var been = new TrackedAtomsStack<string>(["newest", "oldest"]);

        been.Push("newer still");

        Assert.Equal(["newer still", "newest", "oldest"], been.Value);

        been.Pop();
        been.Pop();

        Assert.Equal(["oldest"], been.Value);

        history.Undo();

        Assert.Equal(["newest", "oldest"], been.Value);

        history.Undo();

        Assert.Equal(["newer still", "newest", "oldest"], been.Value);

        history.Undo();

        Assert.Equal(["newest", "oldest"], been.Value);
        Assert.False(history.CanUndo);
    }

    [Fact]
    public void ALocalStackStaysOffTheHistory()
    {
        using var history = new AtomHistory();
        var been = new LocalAtomsStack<string>();

        been.Push("here");
        been.Pop();

        Assert.False(history.CanUndo);
    }

    [Fact]
    public void ARebuiltStackIsGivenTopFirst()
    {
        var been = new LocalAtomsStack<string>(["home"]);
        var heard = 0;

        using var subscription = been.Subscribe(() => heard++);

        been.Reset(["deepest", "middle", "outermost"]);

        Assert.Equal("deepest", been.Peek());
        Assert.Equal(["deepest", "middle", "outermost"], been.Value);
        Assert.Equal(1, heard);

        been.Reset(["deepest", "middle", "outermost"]);

        Assert.Equal(1, heard);
    }

    [Fact]
    public void ItIsWalkedFromTheTopDown()
    {
        var been = new LocalAtomsStack<string>(["top", "bottom"]);
        var walked = new List<string>();

        foreach (var item in been)
        {
            walked.Add(item);
        }

        Assert.Equal(["top", "bottom"], walked);
    }

    [Fact]
    public void ADerivedValueFollowsIt()
    {
        var been = new LocalAtomsStack<string>();
        var here = new Computed<string>(() => been.TryPeek(out var item) ? item : "nowhere");

        Assert.Equal("nowhere", here.Value);

        been.Push("home");

        Assert.Equal("home", here.Value);

        been.Pop();

        Assert.Equal("nowhere", here.Value);
    }

    [Fact]
    public void ClearingAndRebuildingAreOneStepEach()
    {
        using var history = new AtomHistory();
        var been = new TrackedAtomsStack<int>([2, 1]);

        been.Clear();

        Assert.Empty(been.Value);
        Assert.Equal(1, history.Depth);

        been.Clear();

        Assert.Equal(1, history.Depth);

        history.Undo();

        Assert.Equal([2, 1], been.Value);
    }
}
