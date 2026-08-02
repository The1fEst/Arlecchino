using System;
using System.Collections.Generic;

using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;

namespace Arlecchino.Atoms;

/// <summary>
/// Undo and redo over every <see cref="TrackedAtom{T}"/>. It collects from the moment it exists, so the
/// hosted service resolves it at startup; a headless run has to create it before the edits it wants
/// to undo.
///
/// The undo stack is bounded: a long-running application would otherwise hold on to every edit it has
/// ever made, and each of those keeps the old value alive too. Steps past <see cref="Capacity"/> fall
/// off the far end, which is the end nobody is going to reach.
/// </summary>
public sealed class AtomHistory : IDisposable
{
    private readonly LinkedList<List<IAtomEdit>> _undoable = new();
    private readonly Stack<List<IAtomEdit>> _redoable = new();

    private int _capacity = 200;
    private List<IAtomEdit>? _group;
    private int _openGroups;
    private bool _isReplaying;

    /// <summary>Starts collecting edits.</summary>
    public AtomHistory()
    {
        AtomChanges.Recorded += Record;
    }

    /// <summary>
    /// How many steps to keep. Lowering it drops the oldest straight away; anything below one step is
    /// treated as one, since a history that remembers nothing is what <see cref="LocalAtom{T}"/> is
    /// for.
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
    public bool CanUndo => _undoable.Count > 0;

    /// <summary>Whether an undone step can be applied again.</summary>
    public bool CanRedo => _redoable.Count > 0;

    /// <summary>How many steps are on the undo stack.</summary>
    public int Depth => _undoable.Count;

    /// <summary>
    /// Collects everything written until the scope is disposed into a single undo step, so related
    /// edits go back together. Groups nest: a group opened inside another joins it rather than
    /// closing it early, so wrapping code that groups edits of its own still yields one step.
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
        if (_undoable.Count == 0)
        {
            return false;
        }

        var step = _undoable.Last!.Value;
        _undoable.RemoveLast();

        Replay(() =>
        {
            for (var i = step.Count - 1; i >= 0; i--)
            {
                step[i].Undo();
            }
        });

        _redoable.Push(step);
        return true;
    }

    /// <summary>Applies an undone step again. Writing something new drops this branch.</summary>
    /// <returns><c>false</c> when there was nothing to redo.</returns>
    public bool Redo()
    {
        if (_redoable.Count == 0)
        {
            return false;
        }

        var step = _redoable.Pop();
        Replay(() =>
        {
            foreach (var edit in step)
            {
                edit.Redo();
            }
        });

        _undoable.AddLast(step);
        return true;
    }

    /// <summary>
    /// Forgets both stacks. The hosted service does this once the application is up, so wiring does
    /// not end up as the first undo step.
    /// </summary>
    public void Clear()
    {
        _undoable.Clear();
        _redoable.Clear();
    }

    /// <summary>Stops collecting edits.</summary>
    public void Dispose() => AtomChanges.Recorded -= Record;

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

        Remember([edit]);
    }

    private void Remember(List<IAtomEdit> step)
    {
        _undoable.AddLast(step);
        _redoable.Clear();
        DropOldest();
    }

    private void DropOldest()
    {
        while (_undoable.Count > _capacity)
        {
            _undoable.RemoveFirst();
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

        Remember(group);
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
