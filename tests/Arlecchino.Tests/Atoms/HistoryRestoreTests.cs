using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Arlecchino.Atoms.History;
using Arlecchino.Atoms.History.Collections;

namespace Arlecchino.Tests.Atoms;

public sealed class HistoryRestoreTests : IDisposable
{
    private const string Key = "Restore.Server";
    private const string ListKey = "Restore.Rows";
    private const string QueueKey = "Restore.Waiting";
    private const string StackKey = "Restore.Visited";
    private const string SetKey = "Restore.Marked";
    private const string MapKey = "Restore.Sizes";

    private readonly string _folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public HistoryRestoreTests() => Directory.CreateDirectory(_folder);

    public void Dispose() => Directory.Delete(_folder, true);

    [Fact]
    public void AStepWrittenByAnEarlierRunIsTakenBackByThisOne()
    {
        var path = Kept();

        using var file = Opened(path);
        using var history = new AtomHistory(file);
        var atom = new HistoryAtom<string>("changed", Key);

        Assert.True(history.CanUndo);
        Assert.True(history.Undo());
        Assert.Equal("start", atom.Value);
    }

    [Fact]
    public void WhatWasTakenBackFromTheFileIsPutForwardAgain()
    {
        var path = Kept();

        using var file = Opened(path);
        using var history = new AtomHistory(file);
        var atom = new HistoryAtom<string>("changed", Key);

        history.Undo();

        Assert.True(history.CanRedo);
        Assert.True(history.Redo());
        Assert.Equal("changed", atom.Value);
    }

    /// <summary>
    /// The point of the file: a history longer than what memory holds. A single step stays, and the rest
    /// are sought on disk as they are asked for.
    /// </summary>
    [Fact]
    public void StepsPastTheMemoryWindowAreReadBackFromTheFile()
    {
        using var file = new HistoryFile(Path.Combine(_folder, "kept.history"), keep: true);
        using var history = new AtomHistory(file);

        history.Capacity = 1;

        var atom = new HistoryAtom<string>("0", Key);

        for (var step = 1; step <= 5; step++)
        {
            atom.Value = step.ToString();
        }

        Assert.Equal(5, history.Depth);

        for (var step = 4; step >= 0; step--)
        {
            Assert.True(history.Undo());
            Assert.Equal(step.ToString(), atom.Value);
        }

        Assert.False(history.CanUndo);
    }

    [Fact]
    public void AStepNamingAnAtomNothingAnswersToIsNotTakenBack()
    {
        var path = Path.Combine(_folder, "kept.history");

        File.WriteAllText(path, """[{"key":"Nobody.Answers","from":"start","to":"changed"}]""" + "\n");

        using var file = Opened(path);
        using var history = new AtomHistory(file);

        Assert.False(history.Undo());
    }

    [Fact]
    public void ALineThatDoesNotParseIsNotTakenBack()
    {
        var path = Path.Combine(_folder, "kept.history");

        File.WriteAllText(path, """[{"key":"Restore.Server","from":"start","to":"cha""" + "\n");

        using var file = Opened(path);
        using var history = new AtomHistory(file);

        _ = new HistoryAtom<string>("changed", Key);

        Assert.False(history.Undo());
    }

    /// <summary>
    /// What a killed run leaves: the step it was in the middle of has no line feed after it. The line is
    /// not counted, and everything before it still reads.
    /// </summary>
    [Fact]
    public void ARunKilledMidStepLeavesTheStepsBeforeItReadable()
    {
        var path = Kept();

        File.AppendAllText(path, """[{"key":"Restore.Server","from":"changed","to":"cha""");

        using var file = Opened(path);
        using var history = new AtomHistory(file);
        var atom = new HistoryAtom<string>("changed", Key);

        Assert.Equal(1, file.Count);
        Assert.True(history.Undo());
        Assert.Equal("start", atom.Value);
    }

    /// <summary>
    /// A screen's store is rebuilt on the way back, so the same name claimed again by the same kind of
    /// atom is how the slot is taken over, not a mistake.
    /// </summary>
    [Fact]
    public void OneNameIsClaimedAgainByAnAtomHoldingTheSameKind()
    {
        _ = new HistoryAtom<string>("first", "Claimed.Twice");

        var second = new HistoryAtom<string>("second", "Claimed.Twice");

        Assert.Equal("second", second.Value);
    }

    [Fact]
    public void OneNameCannotStandForTwoKindsOfValue()
    {
        _ = new HistoryAtom<string>("text", "Claimed.ByTwoKinds");

        Assert.Throws<InvalidOperationException>(static () => new HistoryAtom<int>(0, "Claimed.ByTwoKinds"));
    }

    [Fact]
    public void AnAtomWithoutANameIsRefused() =>
        Assert.Throws<ArgumentException>(static () => new HistoryAtom<string>("", ""));

    [Fact]
    public void AScratchFileLeavesNothingForTheNextRun()
    {
        var path = Path.Combine(_folder, "scratch.history");

        using (var earlier = new HistoryFile(path))
        using (new AtomHistory(earlier))
        {
            new HistoryAtom<string>("start", Key).Value = "changed";
        }

        using var file = new HistoryFile(path);
        file.Prepare();

        using var later = new AtomHistory(file);

        Assert.Equal(0, file.Count);
        Assert.False(later.CanUndo);
    }

    [Fact]
    public void AKeptFileIsAddedToRatherThanWrittenOver()
    {
        var path = Kept();

        using var file = Opened(path);
        using var history = new AtomHistory(file);

        new HistoryAtom<string>("changed", Key).Value = "changed again";

        Assert.Equal(2, file.Count);
        Assert.Equal(2, File.ReadAllLines(path).Length);
    }

    [Fact]
    public void AStepOfAListWrittenByAnEarlierRunIsTakenBackByThisOne()
    {
        var path = Wrote(
            "rows.history",
            static () => new HistoryAtomsList<string>(ListKey, ["alpha"]).Add("beta"));

        using var file = Opened(path);
        using var history = new AtomHistory(file);
        var rows = new HistoryAtomsList<string>(ListKey, ["alpha", "beta"]);

        Assert.True(history.Undo());
        Assert.Equal(["alpha"], rows.Value);

        Assert.True(history.Redo());
        Assert.Equal(["alpha", "beta"], rows.Value);
    }

    [Fact]
    public void AStepOfAQueueWrittenByAnEarlierRunIsTakenBackByThisOne()
    {
        var path = Wrote(
            "waiting.history",
            static () => new HistoryAtomsQueue<string>(QueueKey, ["first"]).Enqueue("second"));

        using var file = Opened(path);
        using var history = new AtomHistory(file);
        var waiting = new HistoryAtomsQueue<string>(QueueKey, ["first", "second"]);

        Assert.True(history.Undo());
        Assert.Equal(["first"], waiting.Value);
    }

    [Fact]
    public void AStepOfAStackWrittenByAnEarlierRunIsTakenBackByThisOne()
    {
        var path = Wrote(
            "visited.history",
            static () => new HistoryAtomsStack<string>(StackKey, ["oldest"]).Push("newest"));

        using var file = Opened(path);
        using var history = new AtomHistory(file);
        var visitedPages = new HistoryAtomsStack<string>(StackKey, ["newest", "oldest"]);

        Assert.True(history.Undo());
        Assert.Equal(["oldest"], visitedPages.Value);
    }

    [Fact]
    public void AStepOfASetWrittenByAnEarlierRunIsTakenBackByThisOne()
    {
        var path = Wrote(
            "marked.history",
            static () => new HistoryAtomsSet<string>(SetKey, ["alpha"]).Add("beta"));

        using var file = Opened(path);
        using var history = new AtomHistory(file);
        var markedFiles = new HistoryAtomsSet<string>(SetKey, ["alpha", "beta"]);

        Assert.True(history.Undo());
        Assert.True(markedFiles.Value.SetEquals(["alpha"]));
    }

    [Fact]
    public void AStepOfAMapWrittenByAnEarlierRunIsTakenBackByThisOne()
    {
        var path = Wrote(
            "sizes.history",
            static () =>
                new HistoryAtomsMap<string, int>(
                    MapKey,
                    new Dictionary<string, int> { ["alpha"] = 1 })["alpha"] = 2);

        using var file = Opened(path);
        using var history = new AtomHistory(file);
        var sizes = new HistoryAtomsMap<string, int>(MapKey, new Dictionary<string, int> { ["alpha"] = 2 });

        Assert.True(history.Undo());
        Assert.Equal(1, sizes["alpha"]);
    }

    /// <summary>
    /// The file carries what the list held either side of the step rather than the difference it made,
    /// so the step lands on those contents whatever this run happened to start with.
    /// </summary>
    [Fact]
    public void AStepLandsOnTheContentsItWasWrittenAgainstWhateverThisRunHolds()
    {
        var path = Wrote(
            "rows.history",
            static () => new HistoryAtomsList<string>(ListKey, ["alpha"]).Add("beta"));

        using var file = Opened(path);
        using var history = new AtomHistory(file);
        var rows = new HistoryAtomsList<string>(ListKey, ["gamma", "delta"]);

        Assert.True(history.Undo());
        Assert.Equal(["alpha"], rows.Value);

        Assert.True(history.Redo());
        Assert.Equal(["alpha", "beta"], rows.Value);
    }

    /// <summary>
    /// Landing is the smallest change that gets there rather than a rewrite, so an item the step never
    /// touched is the item that was already there. It is built rather than written out, since a literal
    /// would be interned.
    /// </summary>
    [Fact]
    public void AStepIsLandedByChangingOnlyWhatDiffers()
    {
        var path = Wrote(
            "rows.history",
            static () => new HistoryAtomsList<string>(ListKey, ["alpha", "beta", "gamma"]).RemoveAt(2));

        using var file = Opened(path);
        using var history = new AtomHistory(file);

        var keptRow = new string(['a', 'l', 'p', 'h', 'a']);
        var rows = new HistoryAtomsList<string>(ListKey, [keptRow, "beta"]);
        var changes = 0;

        using (rows.Subscribe(() => changes++))
        {
            Assert.True(history.Undo());
        }

        Assert.Equal([keptRow, "beta", "gamma"], rows.Value);
        Assert.Same(keptRow, rows.Value[0]);
        Assert.Equal(1, changes);
    }

    /// <summary>
    /// What state outliving the run means: the list starts out holding what the file last saw it hold,
    /// rather than what its store hands it.
    /// </summary>
    [Fact]
    public void AListAskedToResumeStartsOutHoldingWhatTheFileLastSaw()
    {
        var path = Wrote(
            "rows.history",
            static () => new HistoryAtomsList<string>(ListKey, ["alpha"]).Add("beta"));

        using var file = Opened(path);
        using var history = new AtomHistory(file, resume: true);
        var rows = new HistoryAtomsList<string>(ListKey, ["whatever the store says"]);

        Assert.Equal(["alpha", "beta"], rows.Value);
    }

    [Fact]
    public void AMapAskedToResumeStartsOutHoldingWhatTheFileLastSaw()
    {
        var path = Wrote(
            "sizes.history",
            static () => new HistoryAtomsMap<string, int>(MapKey).Add("alpha", 1));

        using var file = Opened(path);
        using var history = new AtomHistory(file, resume: true);
        var sizes = new HistoryAtomsMap<string, int>(MapKey);

        Assert.Equal(1, sizes["alpha"]);
    }

    [Fact]
    public void AnAtomAskedToResumeStartsOutHoldingWhatTheFileLastSaw()
    {
        var path = Kept();

        using var file = Opened(path);
        using var history = new AtomHistory(file, resume: true);

        Assert.Equal("changed", new HistoryAtom<string>("start", Key).Value);
    }

    /// <summary>A store built later in the run, a screen's on the way back to it, picks its state up too.</summary>
    [Fact]
    public void WhatTheRunItselfWroteIsPickedUpByTheNextAtomOfThatName()
    {
        using var file = Opened(Path.Combine(_folder, "kept.history"));
        using var history = new AtomHistory(file, resume: true);

        new HistoryAtom<string>("start", Key).Value = "changed";

        Assert.Equal("changed", new HistoryAtom<string>("start", Key).Value);
    }

    [Fact]
    public void TheNewestStepIsTheOneResumedFrom()
    {
        var path = Path.Combine(_folder, "kept.history");

        using (var earlier = new HistoryFile(path, keep: true))
        using (new AtomHistory(earlier))
        {
            var atom = new HistoryAtom<string>("start", Key);

            Assert.Equal("start", atom.Value);

            atom.Value = "changed";
            atom.Value = "changed again";
        }

        using var file = Opened(path);
        using var history = new AtomHistory(file, resume: true);

        Assert.Equal("changed again", new HistoryAtom<string>("start", Key).Value);
    }

    [Fact]
    public void ResumingIsNotItselfAStep()
    {
        var path = Kept();

        using var file = Opened(path);
        using var history = new AtomHistory(file, resume: true);

        _ = new HistoryAtom<string>("start", Key);

        Assert.Equal(1, file.Count);
        Assert.Equal(1, history.Depth);
    }

    [Fact]
    public void WithoutBeingAskedToResumeAnAtomStartsOutAsItsStoreSaid()
    {
        var path = Kept();

        using var file = Opened(path);
        using var history = new AtomHistory(file);

        Assert.Equal("start", new HistoryAtom<string>("start", Key).Value);
    }

    /// <summary>Runs once, so that what it wrote is what a later run finds.</summary>
    /// <param name="name">What the file is called.</param>
    /// <param name="change">The step the earlier run makes.</param>
    /// <returns>The path it wrote to.</returns>
    private string Wrote(string name, Action change)
    {
        var path = Path.Combine(_folder, name);

        using var file = new HistoryFile(path, keep: true);
        using var history = new AtomHistory(file);

        change();

        return path;
    }

    /// <summary>Runs once, so that what it wrote is what a later run finds.</summary>
    /// <returns>The path it wrote to.</returns>
    private string Kept()
    {
        var path = Path.Combine(_folder, "kept.history");

        using var file = new HistoryFile(path, keep: true);
        using var history = new AtomHistory(file);

        new HistoryAtom<string>("start", Key).Value = "changed";

        return path;
    }

    private static HistoryFile Opened(string path)
    {
        var file = new HistoryFile(path, keep: true);

        file.Prepare();

        return file;
    }
}
