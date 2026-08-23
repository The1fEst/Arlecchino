using System;
using System.Collections.Generic;

namespace Arlecchino.Atoms.History;

/// <summary>
/// Undo and redo over every <see cref="HistoryAtom{T}"/> and history collection. Recent steps are held in
/// memory, and older ones are read back from the <see cref="HistoryFile"/> a step at a time.
/// </summary>
public sealed class AtomHistory : IDisposable
{
    private readonly LinkedList<Step> _undoStack = new();
    private readonly Stack<Step> _redoStack = new();

    private readonly HistoryFile? _file;

    private int _capacity = 200;
    private int _floor;
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
        _floor = file?.Count ?? 0;

        HistoryChanges.Recorded += Record;
    }

    /// <summary>
    /// How many steps to keep in memory. Lowering it drops the oldest straight away, and what was written
    /// down is read back from the file as it is needed.
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

    /// <summary>Whether there is a step to undo, in memory or in the file.</summary>
    public bool CanUndo => _undoStack.Count > 0 || _floor > 0;

    /// <summary>Whether an undone step can be applied again.</summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>How many steps can be taken back, counting the ones only the file holds.</summary>
    public int Depth => _undoStack.Count + _floor;

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
            return UndoFromFile();
        }

        var step = _undoStack.Last!.Value;
        _undoStack.RemoveLast();

        Undo(step.Edits);

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
            foreach (var edit in step.Edits)
            {
                edit.Redo();
            }
        });

        _undoStack.AddLast(step);
        return true;
    }

    /// <summary>
    /// Forgets what this run holds in memory, so that wiring does not end up as the first undo step. What
    /// the file holds stays where it is.
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();

        _floor = _file?.Count ?? 0;
    }

    /// <summary>Stops collecting edits.</summary>
    public void Dispose() => HistoryChanges.Recorded -= Record;

    /// <summary>
    /// Takes back a step the file holds and memory does not, seeking to it rather than loading what came
    /// before. A step that cannot be rebuilt ends the walk back, since what is older assumes it.
    /// </summary>
    /// <returns><c>false</c> when the file had nothing more to give.</returns>
    private bool UndoFromFile()
    {
        if (_file is null || _floor <= 0)
        {
            return false;
        }

        if (_file.Read(_floor - 1) is not { } edits)
        {
            _floor = 0;
            return false;
        }

        _floor--;

        Undo(edits);

        _redoStack.Push(new Step(edits, _floor));
        return true;
    }

    private void Undo(List<IAtomEdit> edits) =>
        Replay(() =>
        {
            for (var i = edits.Count - 1; i >= 0; i--)
            {
                edits[i].Undo();
            }
        });

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

    private void Push(List<IAtomEdit> edits)
    {
        var written = _file?.Write(edits) == true;

        _undoStack.AddLast(new Step(edits, written ? _file!.Count - 1 : -1));
        _redoStack.Clear();

        DropOldest();
    }

    private void DropOldest()
    {
        while (_undoStack.Count > _capacity)
        {
            var oldest = _undoStack.First!.Value;

            _undoStack.RemoveFirst();

            if (oldest.FileStep >= 0)
            {
                _floor = oldest.FileStep + 1;
            }
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

    /// <summary>
    /// One step, and where the file put it. <see cref="FileStep"/> is <c>-1</c> for a step the file could
    /// not take, which is lost once it falls out of memory.
    /// </summary>
    /// <param name="Edits">What the step changed.</param>
    /// <param name="FileStep">Which step of the file this is, or <c>-1</c>.</param>
    private readonly record struct Step(List<IAtomEdit> Edits, int FileStep);

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
