using System;
using System.IO;
using Xunit;
using Arlecchino.Atoms.History;

namespace Arlecchino.Tests.Atoms;

public sealed class HistoryRestoreTests : IDisposable
{
    private const string Key = "Restore.Server";

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
