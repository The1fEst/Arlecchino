using System.Collections.Generic;
using Arlecchino.State;
using Xunit;

namespace Arlecchino.Tests;

public sealed class StateHistoryTests
{
    [Fact]
    public void StatesOptOutOfTheHistoryOneAtATime()
    {
        using var history = new StateHistory();
        var tracked = new State<string>("");
        var untracked = new State<string>("") { RecordsHistory = false };


        untracked.Value = "ignored";
        Assert.False(history.CanUndo);

        tracked.Value = "kept";
        Assert.True(history.CanUndo);
    }

    [Fact]
    public void UndoAndRedoWalkTheEdits()
    {
        using var history = new StateHistory();
        var name = new State<string>("start");

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
        using var history = new StateHistory();
        var count = new State<int>(0);

        count.Value = 1;
        history.Undo();

        Assert.Equal(0, history.Depth);
        Assert.True(history.CanRedo);
    }

    [Fact]
    public void WritingAfterUndoDropsTheRedoBranch()
    {
        using var history = new StateHistory();
        var count = new State<int>(0);

        count.Value = 1;
        history.Undo();
        count.Value = 7;

        Assert.False(history.CanRedo);
        Assert.Equal(7, count.Value);
    }

    [Fact]
    public void GroupedEditsUndoTogether()
    {
        using var history = new StateHistory();
        var first = new State<string>("");
        var second = new State<string>("");

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
        using var history = new StateHistory { Capacity = 3 };
        var text = new State<string>("");

        for (var edit = 1; edit <= 10; edit++)
        {
            text.Value = edit.ToString();
        }

        Assert.Equal(3, history.Depth);

        while (history.Undo())
        {
        }

        Assert.Equal("7", text.Value);
    }

    [Fact]
    public void LoweringTheCapacityDropsWhatNoLongerFits()
    {
        using var history = new StateHistory();
        var text = new State<string>("");

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
        using var history = new StateHistory { Capacity = 2 };
        var text = new State<string>("");

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
        using var history = new StateHistory();
        var owners = new List<object?>();
        var count = new State<int>(0);

        StateChanges.Recorded += Collect;
        count.Value = 3;
        StateChanges.Recorded -= Collect;

        Assert.Equal([count], owners);

        void Collect(IStateEdit edit) => owners.Add(edit.Owner);
    }
}
