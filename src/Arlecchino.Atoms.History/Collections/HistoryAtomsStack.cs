using System;
using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// A stack whose changes go on the undo stack. A plain <see cref="AtomsStack{T}"/> is the one whose
/// changes do not.
/// </summary>
/// <typeparam name="T">What the stack holds.</typeparam>
public sealed class HistoryAtomsStack<T> : AtomsStack<T>, IRestorableAtom, IHistoryCollection<T>
{
    /// <summary>Creates a stack that keeps history.</summary>
    /// <param name="key">What the stack is called; see <see cref="Key"/>.</param>
    /// <param name="initial">What is already on it, top first; empty when omitted. It is copied, not held.</param>
    /// <exception cref="InvalidOperationException">The name is taken by an atom holding something else.</exception>
    public HistoryAtomsStack(string key, IReadOnlyList<T>? initial = null) : base(initial)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        Key = key;
        HistoryChanges.Name(key, typeof(T[]), this);
    }

    /// <summary>
    /// What the stack is called in a <see cref="HistoryFile"/>, written in full — usually the store and
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

    private sealed class Inserted : ContentsEdit<HistoryAtomsStack<T>, T>
    {
        private readonly int _index;
        private readonly T[] _items;

        public Inserted(HistoryAtomsStack<T> stack, int index, T[] items, T[]? undone, T[]? redone)
            : base(stack, undone, redone)
        {
            _index = index;
            _items = items;
        }

        public override void Undo() => Collection.RemoveSilently(_index, _items.Length);

        public override void Redo() => Collection.InsertSilently(_index, _items);
    }

    private sealed class Removed : ContentsEdit<HistoryAtomsStack<T>, T>
    {
        private readonly int _index;
        private readonly T[] _items;

        public Removed(HistoryAtomsStack<T> stack, int index, T[] items, T[]? undone, T[]? redone)
            : base(stack, undone, redone)
        {
            _index = index;
            _items = items;
        }

        public override void Undo() => Collection.InsertSilently(_index, _items);

        public override void Redo() => Collection.RemoveSilently(_index, _items.Length);
    }

    private sealed class Swapped : ContentsEdit<HistoryAtomsStack<T>, T>
    {
        private readonly T[] _oldValue;
        private readonly T[] _newValue;

        public Swapped(HistoryAtomsStack<T> stack, T[] oldValue, T[] newValue) : base(stack, oldValue, newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override void Undo() => Collection.ResetSilently(_oldValue);

        public override void Redo() => Collection.ResetSilently(_newValue);
    }
}
