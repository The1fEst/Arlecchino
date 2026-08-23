using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// A set whose changes go on the undo stack. A plain <see cref="AtomsSet{T}"/> is the one whose changes
/// do not.
/// </summary>
/// <typeparam name="T">What the set holds.</typeparam>
public sealed class HistoryAtomsSet<T> : AtomsSet<T>
{
    /// <summary>Creates a set that keeps history.</summary>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">How items are compared.</param>
    public HistoryAtomsSet(IReadOnlyList<T>? initial = null, IEqualityComparer<T>? comparer = null)
        : base(initial, comparer) { }

    /// <inheritdoc />
    protected override bool IsObserved => HistoryChanges.IsRecording;

    /// <inheritdoc />
    protected override void OnAdded(T[] items) => HistoryChanges.NotifyRecorded(new Added(this, items));

    /// <inheritdoc />
    protected override void OnRemoved(T[] items) => HistoryChanges.NotifyRecorded(new Removed(this, items));

    /// <inheritdoc />
    protected override void OnReset(T[] previous, T[] current) =>
        HistoryChanges.NotifyRecorded(new Swapped(this, previous, current));

    private sealed class Added : IAtomEdit
    {
        private readonly HistoryAtomsSet<T> _set;
        private readonly T[] _items;

        public Added(HistoryAtomsSet<T> set, T[] items)
        {
            _set = set;
            _items = items;
        }

        public object Owner => _set;

        public void Undo() => _set.RemoveSilently(_items);

        public void Redo() => _set.AddSilently(_items);
    }

    private sealed class Removed : IAtomEdit
    {
        private readonly HistoryAtomsSet<T> _set;
        private readonly T[] _items;

        public Removed(HistoryAtomsSet<T> set, T[] items)
        {
            _set = set;
            _items = items;
        }

        public object Owner => _set;

        public void Undo() => _set.AddSilently(_items);

        public void Redo() => _set.RemoveSilently(_items);
    }

    private sealed class Swapped : IAtomEdit
    {
        private readonly HistoryAtomsSet<T> _set;
        private readonly T[] _oldValue;
        private readonly T[] _newValue;

        public Swapped(HistoryAtomsSet<T> set, T[] oldValue, T[] newValue)
        {
            _set = set;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public object Owner => _set;

        public void Undo() => _set.ResetSilently(_oldValue);

        public void Redo() => _set.ResetSilently(_newValue);
    }
}
