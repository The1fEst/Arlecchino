using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// A list whose changes go on the undo stack. A plain <see cref="AtomsList{T}"/> is the one whose changes
/// do not.
/// </summary>
/// <typeparam name="T">What the list holds.</typeparam>
public sealed class HistoryAtomsList<T> : AtomsList<T>
{
    /// <summary>Creates a list that keeps history.</summary>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">How <c>Remove</c> finds an item, and how the indexer decides nothing changed.</param>
    public HistoryAtomsList(IReadOnlyList<T>? initial = null, IEqualityComparer<T>? comparer = null)
        : base(initial, comparer) { }

    /// <inheritdoc />
    protected override bool IsObserved => HistoryChanges.IsRecording;

    /// <inheritdoc />
    protected override void OnInserted(int index, T[] items) =>
        HistoryChanges.NotifyRecorded(new Inserted(this, index, items));

    /// <inheritdoc />
    protected override void OnRemoved(int index, T[] items) =>
        HistoryChanges.NotifyRecorded(new Removed(this, index, items));

    /// <inheritdoc />
    protected override void OnReplaced(int index, T previous, T current) =>
        HistoryChanges.NotifyRecorded(new Replaced(this, index, previous, current));

    /// <inheritdoc />
    protected override void OnReset(T[] previous, T[] current) =>
        HistoryChanges.NotifyRecorded(new Swapped(this, previous, current));

    private sealed class Inserted : IAtomEdit
    {
        private readonly HistoryAtomsList<T> _list;
        private readonly int _index;
        private readonly T[] _items;

        public Inserted(HistoryAtomsList<T> list, int index, T[] items)
        {
            _list = list;
            _index = index;
            _items = items;
        }

        public object Owner => _list;

        public void Undo() => _list.RemoveSilently(_index, _items.Length);

        public void Redo() => _list.InsertSilently(_index, _items);
    }

    private sealed class Removed : IAtomEdit
    {
        private readonly HistoryAtomsList<T> _list;
        private readonly int _index;
        private readonly T[] _items;

        public Removed(HistoryAtomsList<T> list, int index, T[] items)
        {
            _list = list;
            _index = index;
            _items = items;
        }

        public object Owner => _list;

        public void Undo() => _list.InsertSilently(_index, _items);

        public void Redo() => _list.RemoveSilently(_index, _items.Length);
    }

    private sealed class Replaced : IAtomEdit
    {
        private readonly HistoryAtomsList<T> _list;
        private readonly int _index;
        private readonly T _oldValue;
        private readonly T _newValue;

        public Replaced(HistoryAtomsList<T> list, int index, T oldValue, T newValue)
        {
            _list = list;
            _index = index;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public object Owner => _list;

        public void Undo() => _list.SetSilently(_index, _oldValue);

        public void Redo() => _list.SetSilently(_index, _newValue);
    }

    private sealed class Swapped : IAtomEdit
    {
        private readonly HistoryAtomsList<T> _list;
        private readonly T[] _oldValue;
        private readonly T[] _newValue;

        public Swapped(HistoryAtomsList<T> list, T[] oldValue, T[] newValue)
        {
            _list = list;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public object Owner => _list;

        public void Undo() => _list.ResetSilently(_oldValue);

        public void Redo() => _list.ResetSilently(_newValue);
    }
}
