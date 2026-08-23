using System;
using System.Collections.Generic;

namespace Arlecchino.Atoms.History;

/// <summary>
/// Undo and redo over every <see cref="HistoryAtom{T}"/> and history collection, collecting from the moment
/// it exists. The stack is bounded, and steps past <see cref="Capacity"/> fall off the far end.
///
/// There is one of it per application, which is why it cannot be built from outside: atoms announce
/// themselves to the whole process rather than to a history they were given, so a second one would collect
/// the same edits into a second stack, and undoing on one would leave the other holding steps whose values
/// no longer match what the atoms hold. The host registers it; take it from the container.
/// </summary>
public sealed class AtomHistory : IDisposable
{
    private readonly LinkedList<List<IAtomEdit>> _undoStack = new();
    private readonly Stack<List<IAtomEdit>> _redoStack = new();

    private readonly HistoryFile? _file;

    private int _capacity = 200;
    private List<IAtomEdit>? _group;
    private int _openGroups;
    private bool _isReplaying;

    /// <summary>Starts collecting edits, keeping them for as long as the process runs.</summary>
    internal AtomHistory() : this(null) { }

    /// <summary>Starts collecting edits, and writes down the ones it can.</summary>
    /// <param name="file">
    /// Where steps are written as they are made, or <c>null</c> to keep them in memory only. Only steps of
    /// atoms carrying a <see cref="HistoryAtom{T}.Key"/> are written; see <see cref="HistoryFile"/>.
    /// </param>
    internal AtomHistory(HistoryFile? file)
    {
        _file = file;

        HistoryChanges.Recorded += Record;
    }

    /// <summary>
    /// How many steps to keep. Lowering it drops the oldest straight away; anything below one step is
    /// treated as one, since a history that remembers nothing is what a plain <c>Atom&lt;T&gt;</c> is for.
    /// </summary>
    public int Capacity
    {
        get => _capacity;
        set
        {
            _capacity = Math.Max(1, value);
            DropOldest();
        }
    }

    /// <summary>Whether there is a step to undo.</summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>Whether an undone step can be applied again.</summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>How many steps are on the undo stack.</summary>
    public int Depth => _undoStack.Count;

    /// <summary>
    /// Collects everything written until the scope is disposed into a single undo step. A group opened
    /// inside another joins it rather than closing it early.
    /// </summary>
    /// <returns>The scope to dispose when the group is complete.</returns>
    public IDisposable Group()
    {
        _group ??= [];
        _openGroups++;
        return new GroupScope(this);
    }

    /// <summary>Takes back the last step. Undoing does not itself become a step.</summary>
    /// <returns><c>false</c> when there was nothing to undo.</returns>
    public bool Undo()
    {
        if (_undoStack.Count == 0)
        {
            return false;
        }

        var step = _undoStack.Last!.Value;
        _undoStack.RemoveLast();

        Replay(() =>
        {
            for (var i = step.Count - 1; i >= 0; i--)
            {
                step[i].Undo();
            }
        });

        _redoStack.Push(step);
        return true;
    }

    /// <summary>Applies an undone step again. Writing something new drops this branch.</summary>
    /// <returns><c>false</c> when there was nothing to redo.</returns>
    public bool Redo()
    {
        if (_redoStack.Count == 0)
        {
            return false;
        }

        var step = _redoStack.Pop();
        Replay(() =>
        {
            foreach (var edit in step)
            {
                edit.Redo();
            }
        });

        _undoStack.AddLast(step);
        return true;
    }

    /// <summary>
    /// Forgets both stacks. The hosted service does this once the application is up, so wiring does
    /// not end up as the first undo step.
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    /// <summary>Stops collecting edits.</summary>
    public void Dispose() => HistoryChanges.Recorded -= Record;

    private void Record(IAtomEdit edit)
    {
        if (_isReplaying)
        {
            return;
        }

        if (_group is not null)
        {
            _group.Add(edit);
            return;
        }

        Push([edit]);
    }

    private void Push(List<IAtomEdit> step)
    {
        _undoStack.AddLast(step);
        _redoStack.Clear();
        DropOldest();

        _file?.Write(step);
    }

    private void DropOldest()
    {
        while (_undoStack.Count > _capacity)
        {
            _undoStack.RemoveFirst();
        }
    }

    private void Replay(Action apply)
    {
        _isReplaying = true;

        try
        {
            apply();
        }
        finally
        {
            _isReplaying = false;
        }
    }

    private void CommitGroup()
    {
        if (--_openGroups > 0)
        {
            return;
        }

        var group = _group;
        _group = null;
        _openGroups = 0;

        if (group is not { Count: > 0 })
        {
            return;
        }

        Push(group);
    }

    private sealed class GroupScope : IDisposable
    {
        private AtomHistory? _history;

        public GroupScope(AtomHistory history)
        {
            _history = history;
        }

        public void Dispose()
        {
            _history?.CommitGroup();
            _history = null;
        }
    }
}
