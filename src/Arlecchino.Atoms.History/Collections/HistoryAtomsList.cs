using System;
using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// A list whose changes go on the undo stack. A plain <see cref="AtomsList{T}"/> is the one whose changes
/// do not.
/// </summary>
/// <typeparam name="T">What the list holds.</typeparam>
public sealed class HistoryAtomsList<T> : AtomsList<T>, IRestorableAtom, IHistoryCollection<T>
{
    /// <summary>What the base compares by, kept here as well because it does not hand its own out.</summary>
    private readonly IEqualityComparer<T> _comparer;

    /// <summary>Creates a list that keeps history.</summary>
    /// <param name="key">What the list is called; see <see cref="Key"/>.</param>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">How <c>Remove</c> finds an item, and how the indexer decides nothing changed.</param>
    /// <exception cref="InvalidOperationException">The name is taken by an atom holding something else.</exception>
    public HistoryAtomsList(
        string key,
        IReadOnlyList<T>? initial = null,
        IEqualityComparer<T>? comparer = null)
        : base(initial, comparer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _comparer = comparer ?? EqualityComparer<T>.Default;

        Key = key;

        HistoryChanges.Name(key, typeof(T[]), this);
    }

    /// <summary>
    /// What the list is called in a <see cref="HistoryFile"/>, written in full — usually the store and the
    /// property — so that it outlives a rename.
    /// </summary>
    public string Key { get; }

    /// <inheritdoc />
    public IAtomEdit? Restore(IValueReader reader) =>
        reader.ReadItems<T>(out var undone, out var redone)
            ? new RestoredContents<T>(this, undone, redone)
            : null;

    /// <inheritdoc />
    protected override bool IsObserved => HistoryChanges.IsRecording;

    /// <inheritdoc />
    void IHistoryCollection<T>.Land(T[] target)
    {
        var (index, count, items) = Contents.Difference(Value, target, _comparer);

        if (count > 0)
        {
            RemoveSilently(index, count);
        }

        if (items.Length > 0)
        {
            InsertSilently(index, items);
        }
    }

    /// <inheritdoc />
    protected override void OnInserted(int index, T[] items)
    {
        var redone = Contents.Copied(Value);

        HistoryChanges.NotifyRecorded(
            new Inserted(this, index, items, Contents.Without(redone, index, items.Length), redone));
    }

    /// <inheritdoc />
    protected override void OnRemoved(int index, T[] items)
    {
        var redone = Contents.Copied(Value);

        HistoryChanges.NotifyRecorded(
            new Removed(this, index, items, Contents.With(redone, index, items), redone));
    }

    /// <inheritdoc />
    protected override void OnReplaced(int index, T previous, T current)
    {
        var redone = Contents.Copied(Value);

        HistoryChanges.NotifyRecorded(
            new Replaced(this, index, previous, current, Contents.With(redone, index, previous), redone));
    }

    /// <inheritdoc />
    protected override void OnReset(T[] previous, T[] current) =>
        HistoryChanges.NotifyRecorded(new Swapped(this, previous, current));

    private sealed class Inserted : ContentsEdit<HistoryAtomsList<T>, T>
    {
        private readonly int _index;
        private readonly T[] _items;

        public Inserted(HistoryAtomsList<T> list, int index, T[] items, T[]? undone, T[]? redone)
            : base(list, undone, redone)
        {
            _index = index;
            _items = items;
        }

        public override void Undo() => Collection.RemoveSilently(_index, _items.Length);

        public override void Redo() => Collection.InsertSilently(_index, _items);
    }

    private sealed class Removed : ContentsEdit<HistoryAtomsList<T>, T>
    {
        private readonly int _index;
        private readonly T[] _items;

        public Removed(HistoryAtomsList<T> list, int index, T[] items, T[]? undone, T[]? redone)
            : base(list, undone, redone)
        {
            _index = index;
            _items = items;
        }

        public override void Undo() => Collection.InsertSilently(_index, _items);

        public override void Redo() => Collection.RemoveSilently(_index, _items.Length);
    }

    private sealed class Replaced : ContentsEdit<HistoryAtomsList<T>, T>
    {
        private readonly int _index;
        private readonly T _oldValue;
        private readonly T _newValue;

        public Replaced(
            HistoryAtomsList<T> list,
            int index,
            T oldValue,
            T newValue,
            T[]? undone,
            T[]? redone)
            : base(list, undone, redone)
        {
            _index = index;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override void Undo() => Collection.SetSilently(_index, _oldValue);

        public override void Redo() => Collection.SetSilently(_index, _newValue);
    }

    private sealed class Swapped : ContentsEdit<HistoryAtomsList<T>, T>
    {
        private readonly T[] _oldValue;
        private readonly T[] _newValue;

        public Swapped(HistoryAtomsList<T> list, T[] oldValue, T[] newValue) : base(list, oldValue, newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override void Undo() => Collection.ResetSilently(_oldValue);

        public override void Redo() => Collection.ResetSilently(_newValue);
    }
}
