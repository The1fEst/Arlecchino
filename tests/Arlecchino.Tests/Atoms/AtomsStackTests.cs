using System;
using System.Collections.Generic;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;
using Xunit;

namespace Arlecchino.Tests.Atoms;

public sealed class AtomsStackTests
{
    [Fact]
    public void WhatGoesOnTopComesOffFirst()
    {
        var stack = new LocalAtomsStack<string>();

        stack.Push("first");
        stack.Push("second");
        stack.Push("third");

        Assert.Equal(["third", "second", "first"], stack.Value);
        Assert.Equal("third", stack.Peek());
        Assert.Equal("third", stack.Pop());
        Assert.Equal("second", stack.Pop());
        Assert.Equal(1, stack.Count);
    }

    [Fact]
    public void SeveralGoingOnAtOnceLeaveTheLastOnTop()
    {
        var stack = new LocalAtomsStack<int>([0]);
        var changes = 0;

        using var subscription = stack.Subscribe(() => changes++);

        stack.Push([1, 2, 3]);

        Assert.Equal(1, changes);
        Assert.Equal([3, 2, 1, 0], stack.Value);
        Assert.Equal(3, stack.Peek());

        stack.Push([]);

        Assert.Equal(1, changes);
    }

    [Fact]
    public void AnEmptyStackAnswersRatherThanThrowsWhenAskedNicely()
    {
        var stack = new LocalAtomsStack<string>();

        Assert.True(stack.IsEmpty);
        Assert.False(stack.TryPop(out _));
        Assert.False(stack.TryPeek(out _));
        Assert.Throws<InvalidOperationException>(stack.Pop);
        Assert.Throws<InvalidOperationException>(stack.Peek);

        stack.Push("only");

        Assert.True(stack.TryPeek(out var peeked));
        Assert.Equal("only", peeked);
        Assert.True(stack.TryPop(out var taken));
        Assert.Equal("only", taken);
        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void AChangeAsksForAFrame()
    {
        using var repaint = new Repaint();
        var stack = new LocalAtomsStack<string>();

        repaint.TakeRequested();

        stack.Push("here");

        Assert.True(repaint.IsRequested);
    }

    [Fact]
    public void WhatWasTakenOffGoesBackOnTop()
    {
        using var history = new AtomHistory();
        var stack = new TrackedAtomsStack<string>(["newest", "oldest"]);

        stack.Push("newer still");

        Assert.Equal(["newer still", "newest", "oldest"], stack.Value);

        stack.Pop();
        stack.Pop();

        Assert.Equal(["oldest"], stack.Value);

        history.Undo();

        Assert.Equal(["newest", "oldest"], stack.Value);

        history.Undo();

        Assert.Equal(["newer still", "newest", "oldest"], stack.Value);

        history.Undo();

        Assert.Equal(["newest", "oldest"], stack.Value);
        Assert.False(history.CanUndo);
    }

    [Fact]
    public void ALocalStackStaysOffTheHistory()
    {
        using var history = new AtomHistory();
        var stack = new LocalAtomsStack<string>();

        stack.Push("here");
        stack.Pop();

        Assert.False(history.CanUndo);
    }

    [Fact]
    public void ARebuiltStackIsGivenTopFirst()
    {
        var stack = new LocalAtomsStack<string>(["home"]);
        var changes = 0;

        using var subscription = stack.Subscribe(() => changes++);

        stack.Reset(["deepest", "middle", "outermost"]);

        Assert.Equal("deepest", stack.Peek());
        Assert.Equal(["deepest", "middle", "outermost"], stack.Value);
        Assert.Equal(1, changes);

        stack.Reset(["deepest", "middle", "outermost"]);

        Assert.Equal(1, changes);
    }

    [Fact]
    public void ItIsWalkedFromTheTopDown()
    {
        var stack = new LocalAtomsStack<string>(["top", "bottom"]);
        var visits = new List<string>();

        foreach (var item in stack)
        {
            visits.Add(item);
        }

        Assert.Equal(["top", "bottom"], visits);
    }

    [Fact]
    public void ADerivedValueFollowsIt()
    {
        var stack = new LocalAtomsStack<string>();
        var depth = new Computed<string>(() => stack.TryPeek(out var item) ? item : "nowhere");

        Assert.Equal("nowhere", depth.Value);

        stack.Push("home");

        Assert.Equal("home", depth.Value);

        stack.Pop();

        Assert.Equal("nowhere", depth.Value);
    }

    [Fact]
    public void ClearingAndRebuildingAreOneStepEach()
    {
        using var history = new AtomHistory();
        var stack = new TrackedAtomsStack<int>([2, 1]);

        stack.Clear();

        Assert.Empty(stack.Value);
        Assert.Equal(1, history.Depth);

        stack.Clear();

        Assert.Equal(1, history.Depth);

        history.Undo();

        Assert.Equal([2, 1], stack.Value);
    }
}
