using System;
using System.Collections.Generic;
using System.Threading;
using Arlecchino.Atoms;
using Xunit;

namespace Arlecchino.Tests;

public sealed class AtomsMapTests
{
    [Fact]
    public void ChangingTheMapTellsWhoeverIsListening()
    {
        var sizes = new LocalAtomsMap<string, int>();
        var heard = 0;

        using var subscription = sizes.Subscribe(() => heard++);

        sizes["alpha"] = 1;
        sizes["alpha"] = 2;
        sizes.Add("beta", 3);

        Assert.Equal(2, sizes.Count);

        sizes.Remove("alpha");

        Assert.Equal(1, sizes.Count);

        sizes.Clear();

        Assert.Equal(5, heard);
        Assert.Equal(0, sizes.Count);
        Assert.Empty(sizes.Value);
    }

    [Fact]
    public void AChangeAsksForAFrameTheWayAnAtomWriteDoes()
    {
        using var repaint = new Repaint();
        var sizes = new LocalAtomsMap<string, int>();

        repaint.TakeRequested();

        Assert.False(repaint.IsRequested);

        sizes["alpha"] = 1;

        Assert.True(repaint.IsRequested);
    }

    [Fact]
    public void ChangingNothingNotifiesNobody()
    {
        var sizes = new LocalAtomsMap<string, int>(new Dictionary<string, int> { ["alpha"] = 1 });
        var heard = 0;

        using var subscription = sizes.Subscribe(() => heard++);

        sizes["alpha"] = 1;
        sizes.Remove("missing");
        sizes.Reset(new Dictionary<string, int> { ["alpha"] = 1 });

        Assert.Equal(0, heard);

        sizes.Clear();
        sizes.Clear();

        Assert.Equal(1, heard);
    }

    [Fact]
    public void ItIsWalkedWithoutCopyingIt()
    {
        var sizes = new LocalAtomsMap<string, int>(new Dictionary<string, int> { ["alpha"] = 1, ["beta"] = 2 });
        var total = 0;

        foreach (var entry in sizes)
        {
            total += entry.Value;
        }

        foreach (var entry in sizes.Value)
        {
            total += entry.Value;
        }

        Assert.Equal(6, total);
    }

    [Fact]
    public void TakingEntriesOutWhileWalkingIsAllowedButPuttingThemInIsNot()
    {
        var sizes = new LocalAtomsMap<string, int>(new Dictionary<string, int> { ["alpha"] = 1, ["beta"] = 2 });

        foreach (var entry in sizes)
        {
            sizes.Remove(entry.Key);
        }

        Assert.Empty(sizes.Value);

        sizes["alpha"] = 1;

        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var entry in sizes)
            {
                sizes[$"{entry.Key}-again"] = entry.Value;
            }
        });
    }

    [Fact]
    public void AKeyThatIsAlreadyThereIsRefusedByAddButNotByTheIndexer()
    {
        var sizes = new LocalAtomsMap<string, int>(new Dictionary<string, int> { ["alpha"] = 1 });

        Assert.Throws<ArgumentException>(() => sizes.Add("alpha", 2));

        sizes["alpha"] = 2;

        Assert.Equal(2, sizes["alpha"]);
    }

    [Fact]
    public void TheTryingMembersAnswerInsteadOfThrowingOrGuessing()
    {
        var sizes = new LocalAtomsMap<string, int>();

        Assert.True(sizes.TryAdd("alpha", 1));
        Assert.False(sizes.TryAdd("alpha", 2));
        Assert.Equal(1, sizes["alpha"]);

        Assert.True(sizes.TryRemove("alpha", out var taken));
        Assert.Equal(1, taken);
        Assert.False(sizes.TryRemove("alpha", out _));
        Assert.Empty(sizes.Value);
    }

    [Fact]
    public void TryingAndFailingChangesNothing()
    {
        var sizes = new LocalAtomsMap<string, int>(new Dictionary<string, int> { ["alpha"] = 1 });
        var heard = 0;

        using var subscription = sizes.Subscribe(() => heard++);

        Assert.False(sizes.TryAdd("alpha", 2));
        Assert.False(sizes.TryRemove("beta", out _));

        Assert.Equal(0, heard);
    }

    [Fact]
    public void ItLooksUpTheWayItWasToldTo()
    {
        var sizes = new LocalAtomsMap<string, int>(
            new Dictionary<string, int> { ["Alpha"] = 1 },
            StringComparer.OrdinalIgnoreCase);

        Assert.True(sizes.ContainsKey("ALPHA"));
        Assert.True(sizes.TryGetValue("alpha", out var held));
        Assert.Equal(1, held);

        Assert.False(sizes.TryGetValue("beta", out _));
        Assert.Throws<KeyNotFoundException>(() => sizes["beta"]);
    }

    [Fact]
    public void WhatItHoldsCannotBeChangedBehindItsBack()
    {
        var sizes = new LocalAtomsMap<string, int>(new Dictionary<string, int> { ["alpha"] = 1 });
        var value = sizes.Value;

        Assert.IsNotType<Dictionary<string, int>>(value);

        sizes["beta"] = 2;

        Assert.Equal(2, value.Count);
        Assert.Equal(2, value["beta"]);
    }

    [Fact]
    public void ItIsCopiedOutOfWhatItWasGiven()
    {
        var initial = new Dictionary<string, int> { ["alpha"] = 1 };
        var sizes = new LocalAtomsMap<string, int>(initial);

        initial["beta"] = 2;

        Assert.Single(sizes.Value);
    }

    [Fact]
    public void ADerivedValueFollowsTheMap()
    {
        var sizes = new LocalAtomsMap<string, int>(new Dictionary<string, int> { ["alpha"] = 1 });
        var total = new Computed<int>(() =>
        {
            var sum = 0;

            foreach (var entry in sizes.Value)
            {
                sum += entry.Value;
            }

            return sum;
        });

        Assert.Equal(1, total.Value);

        sizes["beta"] = 4;

        Assert.Equal(5, total.Value);

        sizes.Remove("alpha");

        Assert.Equal(4, total.Value);
    }

    [Fact]
    public void ReadingOneKeyIsEnoughToDependOnIt()
    {
        var sizes = new LocalAtomsMap<string, int>();
        var reading = new Computed<string>(() => sizes.TryGetValue("alpha", out var held) ? $"{held}" : "none");

        Assert.Equal("none", reading.Value);

        sizes["alpha"] = 7;

        Assert.Equal("7", reading.Value);
    }

    [Fact]
    public void ATrackedMapGoesOnTheUndoStackAndALocalOneDoesNot()
    {
        using var history = new AtomHistory();
        var kept = new TrackedAtomsMap<string, int>();
        var ignored = new LocalAtomsMap<string, int>();

        ignored["nothing"] = 1;

        Assert.False(history.CanUndo);

        kept["alpha"] = 1;

        Assert.True(history.CanUndo);
        Assert.True(history.Undo());
        Assert.Empty(kept.Value);

        Assert.True(history.Redo());
        Assert.Equal(1, kept["alpha"]);
    }

    [Fact]
    public void EveryKindOfChangeComesBackAgain()
    {
        using var history = new AtomHistory();
        var sizes = new TrackedAtomsMap<string, int>(new Dictionary<string, int> { ["alpha"] = 1 });

        sizes["beta"] = 2;
        sizes["alpha"] = 10;
        sizes.Remove("beta");
        sizes.Reset(new Dictionary<string, int> { ["only"] = 99 });

        Assert.Equal(new Dictionary<string, int> { ["only"] = 99 }, sizes.Value);

        history.Undo();

        Assert.Equal(new Dictionary<string, int> { ["alpha"] = 10 }, sizes.Value);

        history.Undo();

        Assert.Equal(new Dictionary<string, int> { ["alpha"] = 10, ["beta"] = 2 }, sizes.Value);

        history.Undo();

        Assert.Equal(new Dictionary<string, int> { ["alpha"] = 1, ["beta"] = 2 }, sizes.Value);

        history.Undo();

        Assert.Equal(new Dictionary<string, int> { ["alpha"] = 1 }, sizes.Value);
        Assert.False(history.CanUndo);
    }

    [Fact]
    public void AMapThatWasClearedIsFilledBackIn()
    {
        using var history = new AtomHistory();
        var sizes = new TrackedAtomsMap<string, int>(new Dictionary<string, int> { ["alpha"] = 1, ["beta"] = 2 });

        sizes.Clear();

        Assert.Empty(sizes.Value);

        history.Undo();

        Assert.Equal(new Dictionary<string, int> { ["alpha"] = 1, ["beta"] = 2 }, sizes.Value);
    }

    [Fact]
    public void ItRefusesToBeChangedFromAnotherThread()
    {
        using var app = new TestApplication();
        var sizes = new LocalAtomsMap<string, int>();

        using var drawing = FrameThread.Claim();

        var thrown = Refused(() => sizes["alpha"] = 1);

        Assert.Contains("LocalAtomsMap`2", thrown.Message, StringComparison.Ordinal);
        Assert.Empty(sizes.Value);
    }

    [Fact]
    public void PostingIsHowBackgroundWorkFillsIt()
    {
        using var app = new TestApplication();
        var sizes = new LocalAtomsMap<string, int>();

        using var drawing = FrameThread.Claim();

        var loading = new Thread(() =>
            FrameThread.Post(() => sizes.Reset(new Dictionary<string, int> { ["alpha"] = 1 })));

        loading.Start();
        loading.Join();

        Assert.Empty(sizes.Value);

        FrameThread.RunPending(static _ => { });

        Assert.Equal(1, sizes["alpha"]);
    }

    [Fact]
    public void AKeyThatWasNotThereIsPutInWhateverTheValueTypeIs()
    {
        var names = new LocalAtomsMap<int, string>();

        names[1] = "alpha";

        Assert.Equal(1, names.Count);
        Assert.Equal("alpha", names[1]);
    }

    [Fact]
    public void AKeyHeldAgainstNullIsReplacedRatherThanAdded()
    {
        using var history = new AtomHistory();
        var names = new TrackedAtomsMap<int, string?>(new Dictionary<int, string?> { [1] = null });

        names[1] = "alpha";

        Assert.Equal("alpha", names[1]);

        history.Undo();

        Assert.True(names.ContainsKey(1));
        Assert.Null(names[1]);
    }

    private static InvalidOperationException Refused(Action change)
    {
        Exception? thrown = null;

        var changing = new Thread(() =>
        {
            try
            {
                change();
            }
            catch (Exception exception)
            {
                thrown = exception;
            }
        });

        changing.Start();
        changing.Join();

        return Assert.IsType<InvalidOperationException>(thrown);
    }
}
