using System;
using System.Collections.Generic;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;
using Xunit;

namespace Arlecchino.Tests.Atoms;

public sealed class AtomsSetTests
{
    [Fact]
    public void PuttingInWhatIsAlreadyThereChangesNothing()
    {
        var set = new LocalAtomsSet<string>();
        var changes = 0;

        using var subscription = set.Subscribe(() => changes++);

        set.Add("alpha");
        set.Add("alpha");
        set.Remove("beta");

        Assert.Equal(1, changes);
        Assert.Equal(1, set.Count);
        Assert.True(set.Contains("alpha"));
        Assert.False(set.IsEmpty);
    }

    [Fact]
    public void TheTryingMembersSayWhetherAnythingHappened()
    {
        var set = new LocalAtomsSet<string>();

        Assert.True(set.TryAdd("alpha"));
        Assert.False(set.TryAdd("alpha"));
        Assert.True(set.TryRemove("alpha"));
        Assert.False(set.TryRemove("alpha"));
        Assert.True(set.IsEmpty);
    }

    [Fact]
    public void SeveralGoingInAtOnceIsOneChange()
    {
        var set = new LocalAtomsSet<string>(["alpha"]);
        var changes = 0;

        using var subscription = set.Subscribe(() => changes++);

        set.Add(["alpha", "beta", "beta", "gamma"]);

        Assert.Equal(1, changes);
        Assert.Equal(3, set.Count);

        set.Add(["alpha"]);
        set.Add([]);

        Assert.Equal(1, changes);
    }

    [Fact]
    public void AChangeAsksForAFrame()
    {
        using var repaint = new Repaint();
        var set = new LocalAtomsSet<string>();

        repaint.TakeRequested();

        set.Add("alpha");

        Assert.True(repaint.IsRequested);
    }

    [Fact]
    public void ItComparesTheWayItWasToldTo()
    {
        var set = new LocalAtomsSet<string>(["Alpha"], StringComparer.OrdinalIgnoreCase);

        Assert.True(set.Contains("ALPHA"));
        Assert.False(set.TryAdd("alpha"));

        set.Remove("ALPHA");

        Assert.True(set.IsEmpty);
    }

    [Fact]
    public void WhatItHoldsCannotBeChangedBehindItsBack()
    {
        var set = new LocalAtomsSet<string>(["alpha"]);
        var value = set.Value;

        Assert.IsNotType<HashSet<string>>(value);

        set.Add("beta");

        Assert.Equal(2, value.Count);
        Assert.True(value.Contains("beta"));
        Assert.True(value.IsSupersetOf(["alpha"]));
        Assert.True(value.SetEquals(["beta", "alpha"]));
    }

    [Fact]
    public void ItIsCopiedOutOfWhatItWasGiven()
    {
        var initial = new List<string> { "alpha" };
        var set = new LocalAtomsSet<string>(initial);

        initial.Add("beta");

        Assert.Equal(1, set.Count);
    }

    [Fact]
    public void ADerivedValueFollowsIt()
    {
        var set = new LocalAtomsSet<string>();
        var says = new Computed<string>(() => set.Contains("alpha") ? "marked" : "plain");

        Assert.Equal("plain", says.Value);

        set.Add("alpha");

        Assert.Equal("marked", says.Value);

        set.Remove("alpha");

        Assert.Equal("plain", says.Value);
    }

    [Fact]
    public void ATrackedSetGoesOnTheUndoStackAndALocalOneDoesNot()
    {
        using var history = new AtomHistory();
        var survivors = new TrackedAtomsSet<string>();
        var ignored = new LocalAtomsSet<string>();

        ignored.Add("nothing to undo");

        Assert.False(history.CanUndo);

        survivors.Add("alpha");

        Assert.True(history.CanUndo);
        Assert.True(history.Undo());
        Assert.True(survivors.IsEmpty);

        Assert.True(history.Redo());
        Assert.True(survivors.Contains("alpha"));
    }

    [Fact]
    public void EveryKindOfChangeComesBackAgain()
    {
        using var history = new AtomHistory();
        var set = new TrackedAtomsSet<string>(["alpha"]);

        set.Add(["beta", "gamma"]);
        set.Remove("alpha");
        set.Reset(["only"]);

        Assert.True(set.Value.SetEquals(["only"]));

        history.Undo();

        Assert.True(set.Value.SetEquals(["beta", "gamma"]));

        history.Undo();

        Assert.True(set.Value.SetEquals(["alpha", "beta", "gamma"]));

        history.Undo();

        Assert.True(set.Value.SetEquals(["alpha"]));
        Assert.False(history.CanUndo);
    }

    [Fact]
    public void ClearingIsOneStepAndAnEmptySetChangesNothing()
    {
        using var history = new AtomHistory();
        var set = new TrackedAtomsSet<string>(["alpha", "beta"]);

        set.Clear();

        Assert.True(set.IsEmpty);
        Assert.Equal(1, history.Depth);

        set.Clear();

        Assert.Equal(1, history.Depth);

        history.Undo();

        Assert.True(set.Value.SetEquals(["alpha", "beta"]));
    }

    [Fact]
    public void ResettingToTheSameContentsChangesNothing()
    {
        var set = new LocalAtomsSet<string>(["alpha", "beta"]);
        var changes = 0;

        using var subscription = set.Subscribe(() => changes++);

        set.Reset(["beta", "alpha"]);

        Assert.Equal(0, changes);

        set.Reset(["beta"]);

        Assert.Equal(1, changes);
    }

    [Fact]
    public void ItIsWalkedWithoutCopyingIt()
    {
        var set = new LocalAtomsSet<int>([1, 2, 3]);
        var total = 0;

        foreach (var item in set)
        {
            total += item;
        }

        foreach (var item in set.Value)
        {
            total += item;
        }

        Assert.Equal(12, total);
    }
}
