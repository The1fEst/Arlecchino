using System;
using System.IO;
using System.Text.Json;
using Xunit;
using Arlecchino.Atoms;
using Arlecchino.Atoms.History;

namespace Arlecchino.Tests.Atoms;

public sealed class HistoryFileTests : IDisposable
{
    private const string Key = "Settings.Server";

    private readonly string _folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public HistoryFileTests() => Directory.CreateDirectory(_folder);

    public void Dispose() => Directory.Delete(_folder, true);

    [Fact]
    public void ARunThatRecordedNothingLeavesNoFile()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);

        _ = new HistoryAtom<string>("start", Key);

        Assert.False(file.Exists);
    }

    [Fact]
    public void AStepOfANamedAtomIsWrittenDown()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);
        var atom = new HistoryAtom<string>("start", Key);

        atom.Value = "changed";

        var edit = SingleEdit(file);

        Assert.Equal(Key, edit.GetProperty("key").GetString());
        Assert.Equal("start", edit.GetProperty("from").GetString());
        Assert.Equal("changed", edit.GetProperty("to").GetString());
    }

    [Fact]
    public void AStepOfAnAtomNobodyNamedIsLeftOut()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);
        var atom = new HistoryAtom<string>("start");

        atom.Value = "changed";

        Assert.True(history.CanUndo);
        Assert.False(file.Exists);
    }

    [Fact]
    public void AValueOfAKindTheFileWasNeverTaughtIsLeftOut()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);
        var atom = new HistoryAtom<Uri>(new("https://one.example"), Key);

        atom.Value = new("https://two.example");

        Assert.True(history.CanUndo);
        Assert.False(file.Exists);
    }

    [Fact]
    public void AKindOfValueIsTaughtOnceForEveryAtomHoldingIt()
    {
        using var file = Scratch();
        file.Writes<Uri>(static (writer, value) => writer.WriteStringValue(value.ToString()));

        using var history = new AtomHistory(file);
        var atom = new HistoryAtom<Uri>(new("https://one.example"), Key);

        atom.Value = new("https://two.example");

        Assert.Equal("https://two.example/", SingleEdit(file).GetProperty("to").GetString());
    }

    /// <summary>
    /// A group is one step, and half of it on disk would move some atoms and not others when it is read
    /// back. So a group holding anything the file cannot describe is left out whole.
    /// </summary>
    [Fact]
    public void AGroupIsLeftOutWholeWhenPartOfItCannotBeWritten()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);
        var server = new HistoryAtom<string>("start", Key);
        var draft = new HistoryAtom<string>("start");

        using (history.Group())
        {
            server.Value = "changed";
            draft.Value = "changed";
        }

        Assert.True(history.CanUndo);
        Assert.False(file.Exists);
    }

    [Fact]
    public void EveryStepIsALineOfItsOwn()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);
        var atom = new HistoryAtom<string>("start", Key);

        atom.Value = "first";
        atom.Value = "second";

        Assert.Equal(2, File.ReadAllLines(file.Path).Length);
    }

    [Fact]
    public void APlainAtomIsNeverWrittenDownSinceItIsNeverOnTheStack()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);
        var atom = new Atom<string>("start");

        atom.Value = "changed";

        Assert.False(history.CanUndo);
        Assert.False(file.Exists);
    }

    [Fact]
    public void PreparingThrowsAwayWhatAKilledRunLeftBehind()
    {
        var path = Path.Combine(_folder, "scratch.undo");
        File.WriteAllText(path, "[{\"key\":\"stale\"}]\n");

        using var file = new HistoryFile(path);
        file.Prepare();

        Assert.False(file.Exists);
    }

    [Fact]
    public void PreparingLeavesAFileTheApplicationAskedToKeep()
    {
        var path = Path.Combine(_folder, "kept.undo");
        File.WriteAllText(path, "[{\"key\":\"earlier\"}]\n");

        using var file = new HistoryFile(path, keep: true);
        file.Prepare();

        Assert.True(file.Exists);
    }

    [Fact]
    public void TheScratchFileGoesWhenTheRunDoes()
    {
        var file = Scratch();
        using (var history = new AtomHistory(file))
        {
            new HistoryAtom<string>("start", Key).Value = "changed";
        }

        Assert.True(file.Exists);

        file.Dispose();

        Assert.False(file.Exists);
    }

    [Fact]
    public void AKeptFileOutlivesTheRun()
    {
        var file = new HistoryFile(Path.Combine(_folder, "kept.undo"), keep: true);
        using (var history = new AtomHistory(file))
        {
            new HistoryAtom<string>("start", Key).Value = "changed";
        }

        file.Dispose();

        Assert.True(file.Exists);
    }

    private HistoryFile Scratch() => new(Path.Combine(_folder, "scratch.undo"));

    private static JsonElement SingleEdit(HistoryFile file)
    {
        var lines = File.ReadAllLines(file.Path);

        Assert.Single(lines);

        var step = JsonDocument.Parse(lines[0]).RootElement;

        Assert.Equal(1, step.GetArrayLength());

        return step[0];
    }
}
