using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;
using Arlecchino.Atoms;
using Arlecchino.Atoms.History;
using Arlecchino.Atoms.History.Collections;

namespace Arlecchino.Tests.Atoms;

public sealed class HistoryFileTests : IDisposable
{
    private const string Key = "Settings.Server";
    private const string UriKey = "Settings.Home";
    private const string ListKey = "Settings.Rows";
    private const string MapKey = "Settings.Sizes";

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
        new HistoryAtom<string>("start", Key).Value = "changed";

        var edit = SingleEdit(file);

        Assert.Equal(Key, edit.GetProperty("key").GetString());
        Assert.Equal("start", edit.GetProperty("from").GetString());
        Assert.Equal("changed", edit.GetProperty("to").GetString());
    }

    /// <summary>
    /// A collection is written in full either side of the step rather than as the difference it made. A
    /// later run holds something else, and only the contents say where the step lands.
    /// </summary>
    [Fact]
    public void AStepOfANamedListIsWrittenDownAsWhatItHeldEitherSideOfIt()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);
        var rows = new HistoryAtomsList<string>(ListKey, ["alpha", "beta"]);

        Assert.False(file.Exists);

        rows[1] = "second";

        var edit = SingleEdit(file);

        Assert.Equal(ListKey, edit.GetProperty("key").GetString());
        Assert.Equal("""["alpha","beta"]""", edit.GetProperty("from").GetRawText());
        Assert.Equal("""["alpha","second"]""", edit.GetProperty("to").GetRawText());
    }

    [Fact]
    public void AStepOfANamedMapIsWrittenDownAsTheEntriesItHeldEitherSideOfIt()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);
        var sizes = new HistoryAtomsMap<string, int>(MapKey, new Dictionary<string, int> { ["alpha"] = 1 });

        Assert.False(file.Exists);

        sizes["alpha"] = 2;

        var edit = SingleEdit(file);

        Assert.Equal("""[{"key":"alpha","value":1}]""", edit.GetProperty("from").GetRawText());
        Assert.Equal("""[{"key":"alpha","value":2}]""", edit.GetProperty("to").GetRawText());
    }

    [Fact]
    public void AValueOfAKindTheFileWasNeverTaughtIsLeftOut()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);
        new HistoryAtom<Uri>(new("https://one.example"), UriKey).Value = new("https://two.example");

        Assert.True(history.CanUndo);
        Assert.False(file.Exists);
    }

    [Fact]
    public void AKindOfValueIsTaughtOnceForEveryAtomHoldingIt()
    {
        using var file = Scratch();
        file.Handles<Uri>(
            static (writer, value) => writer.WriteStringValue(value.ToString()),
            static element => new(element.GetString()!));

        using var history = new AtomHistory(file);
        new HistoryAtom<Uri>(new("https://one.example"), UriKey).Value = new("https://two.example");

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
        var home = new HistoryAtom<Uri>(new("https://one.example"), UriKey);

        using (history.Group())
        {
            server.Value = "changed";
            home.Value = new("https://two.example");
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

        foreach (var step in new[] { "first", "second" })
        {
            atom.Value = step;
        }

        Assert.Equal(2, File.ReadAllLines(file.Path).Length);
    }

    [Fact]
    public void APlainAtomIsNeverWrittenDownSinceItIsNeverOnTheStack()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);
        new Atom<string>("start").Value = "changed";

        Assert.False(history.CanUndo);
        Assert.False(file.Exists);
    }

    [Fact]
    public void PreparingIndexesWhatAnEarlierRunLeftBehind()
    {
        var path = Path.Combine(_folder, "kept.undo");
        File.WriteAllText(path, "[{\"key\":\"earlier\"}]\n");

        using var file = new HistoryFile(path);
        file.Prepare();

        Assert.True(file.Exists);
        Assert.Equal(1, file.Count);
    }

    [Fact]
    public void TheFileOutlivesTheRunThatWroteIt()
    {
        var file = Scratch();
        using (new AtomHistory(file))
        {
            new HistoryAtom<string>("start", Key).Value = "changed";
        }

        Assert.True(file.Exists);

        file.Dispose();

        Assert.True(file.Exists);
    }

    [Fact]
    public void AKeptFileOutlivesTheRun()
    {
        var file = new HistoryFile(Path.Combine(_folder, "kept.undo"));
        using (new AtomHistory(file))
        {
            new HistoryAtom<string>("start", Key).Value = "changed";
        }

        file.Dispose();

        Assert.True(file.Exists);
    }

    /// <summary>
    /// The file is held open for the whole run, where a hand or a housekeeping job can reach it.
    /// Windows refuses the removal unless the share allows it, where Linux allows it either way.
    /// </summary>
    [Fact]
    public void TheFileCanBeRemovedWhileItIsBeingWrittenTo()
    {
        using var file = Scratch();
        using var history = new AtomHistory(file);

        new HistoryAtom<string>("start", Key).Value = "changed";

        Assert.True(file.Exists);

        File.Delete(file.Path);

        Assert.False(file.Exists);
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
