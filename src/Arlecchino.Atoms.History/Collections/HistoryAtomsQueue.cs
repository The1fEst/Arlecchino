using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// A queue whose changes go on the undo stack. A plain <see cref="AtomsQueue{T}"/> is the one whose
/// changes do not.
/// </summary>
/// <typeparam name="T">What waits in the queue.</typeparam>
public sealed class HistoryAtomsQueue<T> : AtomsQueue<T>
{
    /// <summary>Creates a queue that keeps history.</summary>
    /// <param name="initial">What is already waiting, front first; empty when omitted. It is copied, not held.</param>
    public HistoryAtomsQueue(IReadOnlyList<T>? initial = null) : base(initial) { }

    /// <inheritdoc />
    protected override bool IsObserved => HistoryChanges.IsRecording;

    /// <inheritdoc />
    protected override void OnInserted(int index, T[] items) =>
        HistoryChanges.NotifyRecorded(new Inserted(this, index, items));

    /// <inheritdoc />
    protected override void OnRemoved(int index, T[] items) =>
        HistoryChanges.NotifyRecorded(new Removed(this, index, items));

    /// <inheritdoc />
    protected override void OnReset(T[] previous, T[] current) =>
        HistoryChanges.NotifyRecorded(new Swapped(this, previous, current));

    private sealed class Inserted : IAtomEdit
    {
        private readonly HistoryAtomsQueue<T> _queue;
        private readonly int _index;
        private readonly T[] _items;

        public Inserted(HistoryAtomsQueue<T> queue, int index, T[] items)
        {
            _queue = queue;
            _index = index;
            _items = items;
        }

        public object Owner => _queue;

        public void Undo() => _queue.RemoveSilently(_index, _items.Length);

        public void Redo() => _queue.InsertSilently(_index, _items);
    }

    private sealed class Removed : IAtomEdit
    {
        private readonly HistoryAtomsQueue<T> _queue;
        private readonly int _index;
        private readonly T[] _items;

        public Removed(HistoryAtomsQueue<T> queue, int index, T[] items)
        {
            _queue = queue;
            _index = index;
            _items = items;
        }

        public object Owner => _queue;

        public void Undo() => _queue.InsertSilently(_index, _items);

        public void Redo() => _queue.RemoveSilently(_index, _items.Length);
    }

    private sealed class Swapped : IAtomEdit
    {
        private readonly HistoryAtomsQueue<T> _queue;
        private readonly T[] _oldValue;
        private readonly T[] _newValue;

        public Swapped(HistoryAtomsQueue<T> queue, T[] oldValue, T[] newValue)
        {
            _queue = queue;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public object Owner => _queue;

        public void Undo() => _queue.ResetSilently(_oldValue);

        public void Redo() => _queue.ResetSilently(_newValue);
    }
}
