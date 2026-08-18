using System;
using System.Collections.Generic;
using System.Threading;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Atoms;

public sealed class AtomsListTests
{
    [Fact]
    public void ChangingTheListTellsWhoeverIsListening()
    {
        var rows = new LocalAtomsList<string>(["alpha"]);
        var changes = 0;

        using var subscription = rows.Subscribe(() => changes++);

        rows.Add("beta");
        rows.Insert(0, "start");
        rows[1] = "changed";
        rows.RemoveAt(0);
        rows.Remove("beta");
        rows.Clear();

        Assert.Equal(6, changes);
        Assert.Empty(rows.Value);
    }

    [Fact]
    public void AChangeAsksForAFrameTheWayAnAtomWriteDoes()
    {
        using var repaint = new Repaint();
        var rows = new LocalAtomsList<string>();

        repaint.TakeRequested();

        Assert.False(repaint.IsRequested);

        rows.Add("alpha");

        Assert.True(repaint.IsRequested);
    }

    [Fact]
    public void AListInAPlainAtomIsWhatThisTypeIsFor()
    {
        using var repaint = new Repaint();
        var mutable = new LocalAtom<List<string>>([]);

        repaint.TakeRequested();

        mutable.Value.Add("alpha");

        Assert.False(repaint.IsRequested);

        mutable.Value = mutable.Value;

        Assert.False(repaint.IsRequested);
    }

    [Fact]
    public void AddingSeveralItemsIsOneChange()
    {
        var rows = new LocalAtomsList<int>([1]);
        var changes = 0;

        using var subscription = rows.Subscribe(() => changes++);

        rows.Add([2, 3, 4]);

        Assert.Equal(1, changes);
        Assert.Equal([1, 2, 3, 4], rows.Value);
    }

    [Fact]
    public void ChangingNothingNotifiesNobody()
    {
        var rows = new LocalAtomsList<string>(["alpha", "beta"]);
        var changes = 0;

        using var subscription = rows.Subscribe(() => changes++);

        rows.Add([]);
        rows.Remove("gamma");
        rows[0] = "alpha";
        rows.Reset(["alpha", "beta"]);

        Assert.Equal(0, changes);

        rows.Clear();
        rows.Clear();

        Assert.Equal(1, changes);
    }

    [Fact]
    public void WhatItHoldsCannotBeChangedBehindItsBack()
    {
        var rows = new LocalAtomsList<string>(["alpha"]);
        var value = rows.Value;

        Assert.IsNotType<List<string>>(value);

        rows.Add("beta");

        Assert.Equal(2, value.Count);
        Assert.Equal("beta", value[1]);
    }

    [Fact]
    public void ItIsCopiedOutOfWhatItWasGiven()
    {
        var initial = new List<string> { "alpha" };
        var rows = new LocalAtomsList<string>(initial);

        initial.Add("beta");

        Assert.Single(rows.Value);
    }

    [Fact]
    public void ADerivedValueFollowsTheList()
    {
        var rows = new LocalAtomsList<int>([1, 2]);
        var total = new Computed<int>(() =>
        {
            var sum = 0;

            foreach (var row in rows.Value)
            {
                sum += row;
            }

            return sum;
        });

        Assert.Equal(3, total.Value);

        rows.Add(4);

        Assert.Equal(7, total.Value);

        rows.RemoveAt(0);

        Assert.Equal(6, total.Value);
    }

    [Fact]
    public void ItIsWalkedWithoutCopyingIt()
    {
        var rows = new LocalAtomsList<string>(["alpha", "beta"]);
        var visits = new List<string>();

        foreach (var row in rows)
        {
            visits.Add(row);
        }

        foreach (var row in rows.Value)
        {
            visits.Add(row);
        }

        Assert.Equal(["alpha", "beta", "alpha", "beta"], visits);
    }

    [Fact]
    public void ChangingItWhileWalkingItIsCaught()
    {
        var rows = new LocalAtomsList<string>(["alpha", "beta"]);

        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var row in rows.Value)
            {
                rows.Remove(row);
            }
        });

        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var row in rows)
            {
                rows.Remove(row);
            }
        });
    }

    [Fact]
    public void ASnapshotIsWhatSurvivesBeingChangedUnderneath()
    {
        var rows = new LocalAtomsList<string>(["alpha", "beta"]);

        string[] snapshot = [.. rows.Value];

        foreach (var row in snapshot)
        {
            rows.Remove(row);
        }

        Assert.Empty(rows.Value);
    }

    [Fact]
    public void ARangeGoesOutAsOneChange()
    {
        var rows = new LocalAtomsList<int>([0, 1, 2, 3, 4]);
        var changes = 0;

        using var subscription = rows.Subscribe(() => changes++);

        rows.RemoveRange(0, 3);

        Assert.Equal(1, changes);
        Assert.Equal([3, 4], rows.Value);

        rows.RemoveRange(0, 0);

        Assert.Equal(1, changes);
    }

    [Fact]
    public void ATrimmedListIsFilledBackInWhereItWasCut()
    {
        using var history = new AtomHistory();
        var rows = new TrackedAtomsList<string>(["one", "two", "three", "four"]);

        rows.RemoveRange(1, 2);

        Assert.Equal(["one", "four"], rows.Value);
        Assert.Equal(1, history.Depth);

        history.Undo();

        Assert.Equal(["one", "two", "three", "four"], rows.Value);

        history.Redo();

        Assert.Equal(["one", "four"], rows.Value);
    }

    [Fact]
    public void ARangeOutsideTheListIsRefused()
    {
        var rows = new LocalAtomsList<string>(["alpha", "beta"]);

        Assert.Throws<ArgumentOutOfRangeException>(() => rows.RemoveRange(1, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => rows.RemoveRange(-1, 1));

        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void ReadingHowManyOrWhichIsEnoughToDependOnIt()
    {
        var rows = new LocalAtomsList<string>(["alpha"]);
        var summary = new Computed<string>(() => $"{rows.Count}: {rows[0]}");

        Assert.Equal("1: alpha", summary.Value);

        rows.Add("beta");

        Assert.Equal("2: alpha", summary.Value);

        rows[0] = "first";

        Assert.Equal("2: first", summary.Value);
    }

    [Fact]
    public void ATrackedAtomsListGoesOnTheUndoStackAndALocalOneDoesNot()
    {
        using var history = new AtomHistory();
        var survivors = new TrackedAtomsList<string>();
        var ignored = new LocalAtomsList<string>();

        ignored.Add("nothing to undo");

        Assert.False(history.CanUndo);

        survivors.Add("alpha");

        Assert.True(history.CanUndo);
        Assert.True(history.Undo());
        Assert.Empty(survivors.Value);

        Assert.True(history.Redo());
        Assert.Equal(["alpha"], survivors.Value);
    }

    [Fact]
    public void EveryKindOfChangeComesBackAgain()
    {
        using var history = new AtomHistory();
        var rows = new TrackedAtomsList<string>(["alpha", "beta"]);

        rows.Insert(1, "middle");
        rows[0] = "first";
        rows.Remove("beta");
        rows.Reset(["only"]);

        Assert.Equal(["only"], rows.Value);

        history.Undo();

        Assert.Equal(["first", "middle"], rows.Value);

        history.Undo();

        Assert.Equal(["first", "middle", "beta"], rows.Value);

        history.Undo();

        Assert.Equal(["alpha", "middle", "beta"], rows.Value);

        history.Undo();

        Assert.Equal(["alpha", "beta"], rows.Value);
        Assert.False(history.CanUndo);
    }

    [Fact]
    public void APageAddedAtOnceComesBackAsAPage()
    {
        using var history = new AtomHistory();
        var rows = new TrackedAtomsList<int>([0]);

        rows.Add([1, 2, 3]);

        Assert.Equal(1, history.Depth);

        history.Undo();

        Assert.Equal([0], rows.Value);
    }

    [Fact]
    public void AListThatWasClearedIsFilledBackIn()
    {
        using var history = new AtomHistory();
        var rows = new TrackedAtomsList<string>(["alpha", "beta"]);

        rows.Clear();

        Assert.Empty(rows.Value);

        history.Undo();

        Assert.Equal(["alpha", "beta"], rows.Value);
    }

    [Fact]
    public void ItRefusesToBeChangedFromAnotherThread()
    {
        using var app = new TestApplication();
        var rows = new LocalAtomsList<string>();

        using var drawing = FrameThread.Claim();

        var failure = Refused(() => rows.Add("alpha"));

        Assert.Contains("LocalAtomsList`1", failure.Message, StringComparison.Ordinal);
        Assert.Empty(rows.Value);
    }

    [Fact]
    public void PostingIsHowBackgroundWorkFillsIt()
    {
        using var app = new TestApplication();
        var rows = new LocalAtomsList<string>();

        using var drawing = FrameThread.Claim();

        var loading = new Thread(() => FrameThread.Post(() => rows.Add(["alpha", "beta"])));

        loading.Start();
        loading.Join();

        Assert.Empty(rows.Value);

        FrameThread.RunPending(static _ => { });

        Assert.Equal(["alpha", "beta"], rows.Value);
    }

    [Fact]
    public void ItFindsAnItemTheWayItRemovesOne()
    {
        var rows = new LocalAtomsList<string>(["Alpha"], StringComparer.OrdinalIgnoreCase);

        Assert.Equal(0, rows.IndexOf("ALPHA"));
        Assert.Equal(-1, rows.IndexOf("beta"));

        rows.Remove("alpha");

        Assert.Empty(rows.Value);
    }

    private static InvalidOperationException Refused(Action change)
    {
        Exception? failure = null;

        var changing = new Thread(() =>
        {
            try
            {
                change();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });

        changing.Start();
        changing.Join();

        return Assert.IsType<InvalidOperationException>(failure);
    }
}
