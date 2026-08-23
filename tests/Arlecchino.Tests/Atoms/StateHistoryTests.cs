using System.Collections.Generic;
using Xunit;
using Arlecchino.Atoms;
using Arlecchino.Atoms.History;

namespace Arlecchino.Tests.Atoms;

public sealed class StateHistoryTests
{
    private const string TextKey = "StateHistory.Text";
    private const string CountKey = "StateHistory.Count";

    [Fact]
    public void StatesOptOutOfTheHistoryOneAtATime()
    {
        using var history = new AtomHistory();
        var atom = new HistoryAtom<string>("", TextKey);
        var plain = new Atom<string>("");

        Assert.False(history.CanUndo);

        plain.Value = "ignored";
        Assert.False(history.CanUndo);

        atom.Value = "kept";
        Assert.True(history.CanUndo);
    }

    [Fact]
    public void UndoAndRedoWalkTheEdits()
    {
        using var history = new AtomHistory();
        var name = new HistoryAtom<string>("start", TextKey);

        Assert.False(history.CanUndo);

        name.Value = "first";
        name.Value = "second";

        Assert.True(history.Undo());
        Assert.Equal("first", name.Value);

        Assert.True(history.Undo());
        Assert.Equal("start", name.Value);

        Assert.False(history.CanUndo);
        Assert.False(history.Undo());

        Assert.True(history.Redo());
        Assert.Equal("first", name.Value);

        Assert.True(history.Redo());
        Assert.Equal("second", name.Value);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void UndoingDoesNotRecordItselfAsAnEdit()
    {
        using var history = new AtomHistory();
        var count = new HistoryAtom<int>(0, CountKey);

        Assert.Equal(0, history.Depth);

        count.Value = 1;
        history.Undo();

        Assert.Equal(0, count.Value);
        Assert.Equal(0, history.Depth);
        Assert.True(history.CanRedo);
    }

    [Fact]
    public void WritingAfterUndoDropsTheRedoBranch()
    {
        using var history = new AtomHistory();
        var count = new HistoryAtom<int>(0, CountKey);

        Assert.False(history.CanRedo);

        count.Value = 1;
        history.Undo();
        count.Value = 7;

        Assert.False(history.CanRedo);
        Assert.Equal(7, count.Value);
    }

    [Fact]
    public void GroupedEditsUndoTogether()
    {
        using var history = new AtomHistory();
        var first = new HistoryAtom<string>("", TextKey);
        var second = new HistoryAtom<string>("", TextKey);

        using (history.Group())
        {
            first.Value = "a";
            second.Value = "b";
        }

        Assert.Equal(1, history.Depth);

        history.Undo();

        Assert.Equal("", first.Value);
        Assert.Equal("", second.Value);
    }

    [Fact]
    public void TheOldestStepsFallOffOnceTheHistoryIsFull()
    {
        using var history = new AtomHistory();
        history.Capacity = 3;
        var text = new HistoryAtom<string>("", TextKey);

        for (var edit = 1; edit <= 10; edit++)
        {
            text.Value = edit.ToString();
        }

        Assert.Equal(3, history.Depth);

        while (history.Undo()) { }

        Assert.Equal("7", text.Value);
    }

    [Fact]
    public void LoweringTheCapacityDropsWhatNoLongerFits()
    {
        using var history = new AtomHistory();
        var text = new HistoryAtom<string>("", TextKey);

        for (var edit = 1; edit <= 5; edit++)
        {
            text.Value = edit.ToString();
        }

        history.Capacity = 2;

        Assert.Equal(2, history.Depth);
    }

    [Fact]
    public void AGroupCountsAsOneStepAgainstTheCapacity()
    {
        using var history = new AtomHistory();
        history.Capacity = 2;
        var text = new HistoryAtom<string>("", TextKey);

        for (var step = 1; step <= 3; step++)
        {
            using (history.Group())
            {
                text.Value = $"{step}a";
                text.Value = $"{step}b";
            }
        }

        Assert.Equal(2, history.Depth);
    }

    [Fact]
    public void EditsCarryTheStateTheyBelongTo()
    {
        using var history = new AtomHistory();
        var owners = new List<object?>();
        var count = new HistoryAtom<int>(0, CountKey);

        HistoryChanges.Recorded += Collect;
        count.Value = 3;
        HistoryChanges.Recorded -= Collect;

        Assert.Equal([count], owners);

        void Collect(IAtomEdit edit) => owners.Add(edit.Owner);
    }
}
