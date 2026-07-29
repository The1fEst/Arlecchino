using System;
using System.Collections.Generic;
using Arlecchino.Atoms;
using Xunit;

namespace Arlecchino.Tests;

public sealed class AtomsSetTests
{
    [Fact]
    public void PuttingInWhatIsAlreadyThereChangesNothing()
    {
        var marked = new LocalAtomsSet<string>();
        var heard = 0;

        using var subscription = marked.Subscribe(() => heard++);

        marked.Add("alpha");
        marked.Add("alpha");
        marked.Remove("beta");

        Assert.Equal(1, heard);
        Assert.Equal(1, marked.Count);
        Assert.True(marked.Contains("alpha"));
        Assert.False(marked.IsEmpty);
    }

    [Fact]
    public void TheTryingMembersSayWhetherAnythingHappened()
    {
        var marked = new LocalAtomsSet<string>();

        Assert.True(marked.TryAdd("alpha"));
        Assert.False(marked.TryAdd("alpha"));
        Assert.True(marked.TryRemove("alpha"));
        Assert.False(marked.TryRemove("alpha"));
        Assert.True(marked.IsEmpty);
    }

    [Fact]
    public void SeveralGoingInAtOnceIsOneChange()
    {
        var marked = new LocalAtomsSet<string>(["alpha"]);
        var heard = 0;

        using var subscription = marked.Subscribe(() => heard++);

        marked.Add(["alpha", "beta", "beta", "gamma"]);

        Assert.Equal(1, heard);
        Assert.Equal(3, marked.Count);

        marked.Add(["alpha"]);
        marked.Add([]);

        Assert.Equal(1, heard);
    }

    [Fact]
    public void AChangeAsksForAFrame()
    {
        using var repaint = new Repaint();
        var marked = new LocalAtomsSet<string>();

        repaint.TakeRequested();

        marked.Add("alpha");

        Assert.True(repaint.IsRequested);
    }

    [Fact]
    public void ItComparesTheWayItWasToldTo()
    {
        var marked = new LocalAtomsSet<string>(["Alpha"], StringComparer.OrdinalIgnoreCase);

        Assert.True(marked.Contains("ALPHA"));
        Assert.False(marked.TryAdd("alpha"));

        marked.Remove("ALPHA");

        Assert.True(marked.IsEmpty);
    }

    [Fact]
    public void WhatItHoldsCannotBeChangedBehindItsBack()
    {
        var marked = new LocalAtomsSet<string>(["alpha"]);
        var value = marked.Value;

        Assert.IsNotType<HashSet<string>>(value);

        marked.Add("beta");

        Assert.Equal(2, value.Count);
        Assert.True(value.Contains("beta"));
        Assert.True(value.IsSupersetOf(["alpha"]));
        Assert.True(value.SetEquals(["beta", "alpha"]));
    }

    [Fact]
    public void ItIsCopiedOutOfWhatItWasGiven()
    {
        var initial = new List<string> { "alpha" };
        var marked = new LocalAtomsSet<string>(initial);

        initial.Add("beta");

        Assert.Equal(1, marked.Count);
    }

    [Fact]
    public void ADerivedValueFollowsIt()
    {
        var marked = new LocalAtomsSet<string>();
        var says = new Computed<string>(() => marked.Contains("alpha") ? "marked" : "plain");

        Assert.Equal("plain", says.Value);

        marked.Add("alpha");

        Assert.Equal("marked", says.Value);

        marked.Remove("alpha");

        Assert.Equal("plain", says.Value);
    }

    [Fact]
    public void ATrackedSetGoesOnTheUndoStackAndALocalOneDoesNot()
    {
        using var history = new AtomHistory();
        var kept = new TrackedAtomsSet<string>();
        var ignored = new LocalAtomsSet<string>();

        ignored.Add("nothing to undo");

        Assert.False(history.CanUndo);

        kept.Add("alpha");

        Assert.True(history.CanUndo);
        Assert.True(history.Undo());
        Assert.True(kept.IsEmpty);

        Assert.True(history.Redo());
        Assert.True(kept.Contains("alpha"));
    }

    [Fact]
    public void EveryKindOfChangeComesBackAgain()
    {
        using var history = new AtomHistory();
        var marked = new TrackedAtomsSet<string>(["alpha"]);

        marked.Add(["beta", "gamma"]);
        marked.Remove("alpha");
        marked.Reset(["only"]);

        Assert.True(marked.Value.SetEquals(["only"]));

        history.Undo();

        Assert.True(marked.Value.SetEquals(["beta", "gamma"]));

        history.Undo();

        Assert.True(marked.Value.SetEquals(["alpha", "beta", "gamma"]));

        history.Undo();

        Assert.True(marked.Value.SetEquals(["alpha"]));
        Assert.False(history.CanUndo);
    }

    [Fact]
    public void ClearingIsOneStepAndAnEmptySetChangesNothing()
    {
        using var history = new AtomHistory();
        var marked = new TrackedAtomsSet<string>(["alpha", "beta"]);

        marked.Clear();

        Assert.True(marked.IsEmpty);
        Assert.Equal(1, history.Depth);

        marked.Clear();

        Assert.Equal(1, history.Depth);

        history.Undo();

        Assert.True(marked.Value.SetEquals(["alpha", "beta"]));
    }

    [Fact]
    public void ResettingToTheSameContentsChangesNothing()
    {
        var marked = new LocalAtomsSet<string>(["alpha", "beta"]);
        var heard = 0;

        using var subscription = marked.Subscribe(() => heard++);

        marked.Reset(["beta", "alpha"]);

        Assert.Equal(0, heard);

        marked.Reset(["beta"]);

        Assert.Equal(1, heard);
    }

    [Fact]
    public void ItIsWalkedWithoutCopyingIt()
    {
        var marked = new LocalAtomsSet<int>([1, 2, 3]);
        var total = 0;

        foreach (var item in marked)
        {
            total += item;
        }

        foreach (var item in marked.Value)
        {
            total += item;
        }

        Assert.Equal(12, total);
    }
}
