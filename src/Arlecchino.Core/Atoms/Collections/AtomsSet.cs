using System.Collections;
using System.Collections.Generic;

namespace Arlecchino.Atoms.Collections;

/// <summary>
/// A set held as one piece of application state, behaving as a <c>HashSet&lt;T&gt;</c> does: adding what is
/// already there changes nothing. A walk of it is in no order, so sort it where the reader sees the order.
/// </summary>
/// <typeparam name="T">What the set holds.</typeparam>
public class AtomsSet<T> : AtomsCollectionBase, IReadableAtom<IReadOnlySet<T>>
{
    private readonly HashSet<T> _items;
    private readonly View _view;

    /// <summary>Creates the set.</summary>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">
    /// How items are compared; the default comparer for <typeparamref name="T"/> is used when
    /// omitted.
    /// </param>
    public AtomsSet(IReadOnlyList<T>? initial = null, IEqualityComparer<T>? comparer = null)
    {
        _items = new(comparer);
        _view = new(_items);

        if (initial is null)
        {
            return;
        }

        foreach (var item in initial)
        {
            _items.Add(item);
        }
    }

    /// <summary>
    /// What the set holds now, as a live view rather than a copy. It is read-only all the way down, so every
    /// change goes through the members below.
    /// </summary>
    public IReadOnlySet<T> Value
    {
        get
        {
            Track();
            return _view;
        }
    }

    /// <summary>How many it holds.</summary>
    public int Count
    {
        get
        {
            Track();
            return _items.Count;
        }
    }

    /// <summary>Whether it holds nothing.</summary>
    public bool IsEmpty => Count == 0;

    /// <summary>Puts something in. Putting in what is already there changes nothing.</summary>
    /// <param name="item">What to put in.</param>
    public void Add(T item) => TryAdd(item);

    /// <summary>
    /// Puts several in at once. One notification, one frame and one undo step for however many of
    /// them were not there already.
    /// </summary>
    /// <param name="items">What to put in. Adding none, or only what is there, changes nothing.</param>
    public void Add(IReadOnlyList<T> items)
    {
        var addedItems = new List<T>(items.Count);
        var seenItems = new HashSet<T>(_items.Comparer);

        foreach (var item in items)
        {
            if (!_items.Contains(item) && seenItems.Add(item))
            {
                addedItems.Add(item);
            }
        }

        if (addedItems.Count == 0)
        {
            return;
        }

        Verify();

        foreach (var item in addedItems)
        {
            _items.Add(item);
        }

        if (IsObserved)
        {
            OnAdded([.. addedItems]);
        }

        Notify();
    }

    /// <summary>Puts something in and says whether it was new.</summary>
    /// <param name="item">What to put in.</param>
    /// <returns><c>true</c> when it went in, <c>false</c> when it was already there.</returns>
    public bool TryAdd(T item)
    {
        if (_items.Contains(item))
        {
            return false;
        }

        Verify();

        _items.Add(item);

        if (IsObserved)
        {
            OnAdded([item]);
        }

        Notify();

        return true;
    }

    /// <summary>Takes something out, and does nothing when it is not there.</summary>
    /// <param name="item">What to take out.</param>
    public void Remove(T item) => TryRemove(item);

    /// <summary>Takes something out and says whether it was there.</summary>
    /// <param name="item">What to take out.</param>
    /// <returns><c>true</c> when something was taken out.</returns>
    public bool TryRemove(T item)
    {
        if (!_items.Contains(item))
        {
            return false;
        }

        Verify();

        _items.Remove(item);

        if (IsObserved)
        {
            OnRemoved([item]);
        }

        Notify();

        return true;
    }

    /// <summary>Whether the set holds something.</summary>
    /// <param name="item">What to look for.</param>
    /// <returns><c>true</c> when it is there.</returns>
    public bool Contains(T item)
    {
        Track();

        return _items.Contains(item);
    }

    /// <summary>Takes everything out. An empty set changes nothing.</summary>
    public void Clear() => Reset([]);

    /// <summary>
    /// Replaces the contents in one go, for the set that is worked out again rather than edited —
    /// what a new listing marks, what a filter leaves. Contents equal to what is already there change
    /// nothing.
    /// </summary>
    /// <param name="items">What the set should hold instead.</param>
    public void Reset(IReadOnlyList<T> items)
    {
        if (ContentsEqual(items))
        {
            return;
        }

        Verify();

        var previous = IsObserved ? Snapshot() : [];

        _items.Clear();

        foreach (var item in items)
        {
            _items.Add(item);
        }

        if (IsObserved)
        {
            OnReset(previous, Snapshot());
        }

        Notify();
    }

    /// <summary>
    /// Walks what the set holds, in no order it promises, so <c>foreach</c> over the set itself reads
    /// the way it does over a <c>HashSet&lt;T&gt;</c>. Reach for <see cref="Value"/> where a sequence
    /// is what is wanted, LINQ included.
    /// </summary>
    /// <returns>The enumerator, which throws when the set changes while it is being walked.</returns>
    public HashSet<T>.Enumerator GetEnumerator()
    {
        Track();

        return _items.GetEnumerator();
    }

    private T[] Snapshot() => [.. _items];

    private bool ContentsEqual(IReadOnlyList<T> items)
    {
        foreach (var item in items)
        {
            if (!_items.Contains(item))
            {
                return false;
            }
        }

        var givenItems = new HashSet<T>(_items.Comparer);

        foreach (var item in items)
        {
            givenItems.Add(item);
        }

        return givenItems.Count == _items.Count;
    }

    /// <summary>Called once items went in, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="items">What went in, which is only what was not there already.</param>
    protected virtual void OnAdded(T[] items) { }

    /// <summary>Called once items came out, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="items">What came out.</param>
    protected virtual void OnRemoved(T[] items) { }

    /// <summary>Called once the contents were swapped wholesale, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="previous">What the set held.</param>
    /// <param name="current">What it holds now.</param>
    protected virtual void OnReset(T[] previous, T[] current) { }

    /// <summary>Puts items in without reaching <see cref="OnAdded"/>.</summary>
    /// <param name="items">What to put in.</param>
    protected void AddSilently(T[] items)
    {
        Verify();

        foreach (var item in items)
        {
            _items.Add(item);
        }

        Notify();
    }

    /// <summary>Takes items out without reaching <see cref="OnRemoved"/>.</summary>
    /// <param name="items">What to take out.</param>
    protected void RemoveSilently(T[] items)
    {
        Verify();

        foreach (var item in items)
        {
            _items.Remove(item);
        }

        Notify();
    }

    /// <summary>Swaps the contents without reaching <see cref="OnReset"/>.</summary>
    /// <param name="items">What the set should hold.</param>
    protected void ResetSilently(T[] items)
    {
        Verify();
        _items.Clear();

        foreach (var item in items)
        {
            _items.Add(item);
        }

        Notify();
    }

    private sealed class View : IReadOnlySet<T>
    {
        private readonly HashSet<T> _items;

        public View(HashSet<T> items) => _items = items;

        public int Count => _items.Count;

        public bool Contains(T item) => _items.Contains(item);

        public bool IsProperSubsetOf(IEnumerable<T> other) => _items.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => _items.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) => _items.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) => _items.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) => _items.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other) => _items.SetEquals(other);

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
