using System;
using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// A queue whose changes go on the undo stack. A plain <see cref="AtomsQueue{T}"/> is the one whose
/// changes do not.
/// </summary>
/// <typeparam name="T">What waits in the queue.</typeparam>
public sealed class HistoryAtomsQueue<T> : AtomsQueue<T>, IRestorableAtom, IHistoryCollection<T>
{
    /// <summary>Creates a queue that keeps history.</summary>
    /// <param name="key">What the queue is called; see <see cref="Key"/>.</param>
    /// <param name="initial">What is already waiting, front first; empty when omitted. It is copied, not held.</param>
    /// <exception cref="InvalidOperationException">The name is taken by an atom holding something else.</exception>
    public HistoryAtomsQueue(string key, IReadOnlyList<T>? initial = null) : base(initial)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        Key = key;
        HistoryChanges.Name(key, typeof(T[]), this);
    }

    /// <summary>
    /// What the queue is called in a <see cref="HistoryFile"/>, written in full — usually the store and
    /// the property — so that it outlives a rename.
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
        var (index, count, items) = Contents.Difference(Value, target, EqualityComparer<T>.Default);

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
    protected override void OnReset(T[] previous, T[] current) =>
        HistoryChanges.NotifyRecorded(new Swapped(this, previous, current));

    private sealed class Inserted : ContentsEdit<HistoryAtomsQueue<T>, T>
    {
        private readonly int _index;
        private readonly T[] _items;

        public Inserted(HistoryAtomsQueue<T> queue, int index, T[] items, T[]? undone, T[]? redone)
            : base(queue, undone, redone)
        {
            _index = index;
            _items = items;
        }

        public override void Undo() => Collection.RemoveSilently(_index, _items.Length);

        public override void Redo() => Collection.InsertSilently(_index, _items);
    }

    private sealed class Removed : ContentsEdit<HistoryAtomsQueue<T>, T>
    {
        private readonly int _index;
        private readonly T[] _items;

        public Removed(HistoryAtomsQueue<T> queue, int index, T[] items, T[]? undone, T[]? redone)
            : base(queue, undone, redone)
        {
            _index = index;
            _items = items;
        }

        public override void Undo() => Collection.InsertSilently(_index, _items);

        public override void Redo() => Collection.RemoveSilently(_index, _items.Length);
    }

    private sealed class Swapped : ContentsEdit<HistoryAtomsQueue<T>, T>
    {
        private readonly T[] _oldValue;
        private readonly T[] _newValue;

        public Swapped(HistoryAtomsQueue<T> queue, T[] oldValue, T[] newValue) : base(queue, oldValue, newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override void Undo() => Collection.ResetSilently(_oldValue);

        public override void Redo() => Collection.ResetSilently(_newValue);
    }
}
