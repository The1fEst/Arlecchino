using System;
using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// A set whose changes go on the undo stack. A plain <see cref="AtomsSet{T}"/> is the one whose changes
/// do not.
/// </summary>
/// <typeparam name="T">What the set holds.</typeparam>
public sealed class HistoryAtomsSet<T> : AtomsSet<T>, IRestorableAtom, IHistoryCollection<T>
{
    /// <summary>What the base compares by, kept here as well because it does not hand its own out.</summary>
    private readonly IEqualityComparer<T> _comparer;

    /// <summary>Creates a set that keeps history.</summary>
    /// <param name="key">What the set is called; see <see cref="Key"/>.</param>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">How items are compared.</param>
    /// <exception cref="InvalidOperationException">The name is taken by an atom holding something else.</exception>
    public HistoryAtomsSet(
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
    /// What the set is called in a <see cref="HistoryFile"/>, written in full — usually the store and the
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

    /// <summary>
    /// Makes the set hold what a step says it held, by the smallest change that gets there: what both
    /// agree on stays, and only what the two do not share moves.
    /// </summary>
    /// <param name="target">The contents to land on.</param>
    void IHistoryCollection<T>.Land(T[] target)
    {
        var contents = Value;
        var wantedItems = new HashSet<T>(target, _comparer);
        var removedItems = new List<T>();
        var addedItems = new List<T>();

        foreach (var item in contents)
        {
            if (!wantedItems.Contains(item))
            {
                removedItems.Add(item);
            }
        }

        foreach (var item in target)
        {
            if (!contents.Contains(item))
            {
                addedItems.Add(item);
            }
        }

        if (removedItems.Count > 0)
        {
            RemoveSilently([.. removedItems]);
        }

        if (addedItems.Count > 0)
        {
            AddSilently([.. addedItems]);
        }
    }

    /// <inheritdoc />
    protected override void OnAdded(T[] items)
    {
        var redone = Contents.Copied(Value);

        HistoryChanges.NotifyRecorded(new Added(this, items, Without(redone, items), redone));
    }

    /// <inheritdoc />
    protected override void OnRemoved(T[] items)
    {
        var redone = Contents.Copied(Value);

        HistoryChanges.NotifyRecorded(new Removed(this, items, With(redone, items), redone));
    }

    /// <inheritdoc />
    protected override void OnReset(T[] previous, T[] current) =>
        HistoryChanges.NotifyRecorded(new Swapped(this, previous, current));

    /// <summary>The same contents with items put back, which is what they were before they were taken out.</summary>
    /// <param name="contents">What the set holds now, or nothing when it was not copied.</param>
    /// <param name="items">What to put back.</param>
    /// <returns>The contents with them, or <c>null</c> when there were none to work from.</returns>
    private static T[]? With(T[]? contents, T[] items) => contents is null ? null : [.. contents, .. items];

    /// <summary>The same contents without items, which is what they were before they were put in.</summary>
    /// <param name="contents">What the set holds now, or nothing when it was not copied.</param>
    /// <param name="items">What to leave out.</param>
    /// <returns>The contents without them, or <c>null</c> when there were none to work from.</returns>
    private T[]? Without(T[]? contents, T[] items)
    {
        if (contents is null)
        {
            return null;
        }

        var goneItems = new HashSet<T>(items, _comparer);
        var keptItems = new List<T>(contents.Length);

        foreach (var item in contents)
        {
            if (!goneItems.Contains(item))
            {
                keptItems.Add(item);
            }
        }

        return [.. keptItems];
    }

    private sealed class Added : ContentsEdit<HistoryAtomsSet<T>, T>
    {
        private readonly T[] _items;

        public Added(HistoryAtomsSet<T> set, T[] items, T[]? undone, T[]? redone) : base(set, undone, redone) =>
            _items = items;

        public override void Undo() => Collection.RemoveSilently(_items);

        public override void Redo() => Collection.AddSilently(_items);
    }

    private sealed class Removed : ContentsEdit<HistoryAtomsSet<T>, T>
    {
        private readonly T[] _items;

        public Removed(HistoryAtomsSet<T> set, T[] items, T[]? undone, T[]? redone) : base(set, undone, redone) =>
            _items = items;

        public override void Undo() => Collection.AddSilently(_items);

        public override void Redo() => Collection.RemoveSilently(_items);
    }

    private sealed class Swapped : ContentsEdit<HistoryAtomsSet<T>, T>
    {
        private readonly T[] _oldValue;
        private readonly T[] _newValue;

        public Swapped(HistoryAtomsSet<T> set, T[] oldValue, T[] newValue) : base(set, oldValue, newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override void Undo() => Collection.ResetSilently(_oldValue);

        public override void Redo() => Collection.ResetSilently(_newValue);
    }
}
