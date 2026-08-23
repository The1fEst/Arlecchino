using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Arlecchino.Atoms.Collections;

/// <summary>
/// A map held as one piece of application state, changed in place the way <see cref="AtomsList{T}"/> is.
/// </summary>
/// <typeparam name="TKey">What the entries are looked up by.</typeparam>
/// <typeparam name="TValue">What is kept against each key.</typeparam>
public class AtomsMap<TKey, TValue> : AtomsCollectionBase, IReadableAtom<IReadOnlyDictionary<TKey, TValue>>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _items;
    private readonly ReadOnlyDictionary<TKey, TValue> _view;

    /// <summary>Creates the map.</summary>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">
    /// How keys are compared; the default comparer for <typeparamref name="TKey"/> is used when
    /// omitted. Values are compared with their own default comparer, which is what decides that
    /// writing to one changed nothing.
    /// </param>
    public AtomsMap(IReadOnlyDictionary<TKey, TValue>? initial = null, IEqualityComparer<TKey>? comparer = null)
    {
        _items = new(comparer);
        _view = _items.AsReadOnly();

        if (initial is null)
        {
            return;
        }

        foreach (var entry in initial)
        {
            _items[entry.Key] = entry.Value;
        }
    }

    /// <summary>
    /// What the map holds now, as a live view rather than a copy. It is read-only all the way down, so every
    /// change goes through the members below.
    /// </summary>
    public IReadOnlyDictionary<TKey, TValue> Value
    {
        get
        {
            Track();
            return _view;
        }
    }

    /// <summary>How many entries there are.</summary>
    public int Count
    {
        get
        {
            Track();
            return _items.Count;
        }
    }

    /// <summary>
    /// The value kept against a key. Reading a key the map does not hold throws, as a dictionary does;
    /// writing puts the entry there whether it was there before, and writing an equal value changes nothing.
    /// </summary>
    /// <param name="key">Which entry.</param>
    public TValue this[TKey key]
    {
        get
        {
            Track();
            return _items[key];
        }

        set
        {
            if (!_items.TryGetValue(key, out var previous))
            {
                Insert(key, value);
                return;
            }

            if (EqualityComparer<TValue>.Default.Equals(previous, value))
            {
                return;
            }

            Verify();

            _items[key] = value;

            if (IsObserved)
            {
                OnReplaced(key, previous, value);
            }

            Notify();
        }
    }

    private void Insert(TKey key, TValue value)
    {
        Verify();

        _items[key] = value;

        if (IsObserved)
        {
            OnAdded(key, value);
        }

        Notify();
    }

    /// <summary>
    /// Puts an entry in, and throws when the key is already there, as a dictionary does. Use the indexer to
    /// put one in whether it is there already.
    /// </summary>
    /// <param name="key">What to keep it under.</param>
    /// <param name="value">What to keep.</param>
    public void Add(TKey key, TValue value)
    {
        if (_items.ContainsKey(key))
        {
            throw new ArgumentException($"{key} is already in this {GetType().Name}", nameof(key));
        }

        Insert(key, value);
    }

    /// <summary>
    /// Puts an entry in unless the key is taken, and says which happened — <see cref="Add"/> without
    /// the exception, for the case where losing the race with an earlier entry is an answer rather
    /// than a fault.
    /// </summary>
    /// <param name="key">What to keep it under.</param>
    /// <param name="value">What to keep.</param>
    /// <returns><c>true</c> when the entry went in, <c>false</c> when the key was already there.</returns>
    public bool TryAdd(TKey key, TValue value)
    {
        if (_items.ContainsKey(key))
        {
            return false;
        }

        Insert(key, value);

        return true;
    }

    /// <summary>Takes an entry out, and does nothing when the key is not there.</summary>
    /// <param name="key">Which entry.</param>
    public void Remove(TKey key)
    {
        if (!_items.TryGetValue(key, out var removed))
        {
            return;
        }

        Verify();

        _items.Remove(key);

        if (IsObserved)
        {
            OnRemoved(key, removed);
        }

        Notify();
    }

    /// <summary>
    /// Takes an entry out and hands back what was kept under it, which is the reading and the removal
    /// in one step rather than a lookup followed by a hope.
    /// </summary>
    /// <param name="key">Which entry.</param>
    /// <param name="value">What was kept under it, when it was there.</param>
    /// <returns><c>true</c> when something was taken out.</returns>
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!_items.TryGetValue(key, out value))
        {
            return false;
        }

        Remove(key);

        return true;
    }

    /// <summary>Takes everything out. An empty map changes nothing.</summary>
    public void Clear() => Reset(Empty);

    /// <summary>
    /// Replaces the contents in one go, for the map that is reloaded rather than edited — settings
    /// read again, a listing answered afresh. Contents equal to what is already there change nothing.
    /// </summary>
    /// <param name="items">What the map should hold instead.</param>
    public void Reset(IReadOnlyDictionary<TKey, TValue> items)
    {
        if (ContentsEqual(items))
        {
            return;
        }

        Verify();

        var previous = IsObserved ? Snapshot() : [];

        _items.Clear();

        foreach (var entry in items)
        {
            _items[entry.Key] = entry.Value;
        }

        if (IsObserved)
        {
            OnReset(previous, Snapshot());
        }

        Notify();
    }

    /// <summary>Whether the map holds an entry under a key.</summary>
    /// <param name="key">What to look for.</param>
    /// <returns><c>true</c> when it is there.</returns>
    public bool ContainsKey(TKey key)
    {
        Track();

        return _items.ContainsKey(key);
    }

    /// <summary>Reads an entry without throwing when it is not there.</summary>
    /// <param name="key">What to look for.</param>
    /// <param name="value">What was kept under it, when it was there.</param>
    /// <returns><c>true</c> when it was there.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        Track();

        return _items.TryGetValue(key, out value);
    }

    /// <summary>
    /// Walks the entries, so <c>foreach</c> over the map itself reads the way it does over a dictionary.
    /// Reach for <see cref="Value"/> where a sequence is wanted, LINQ included.
    /// </summary>
    /// <returns>The enumerator, which throws when the map changes while it is being walked.</returns>
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        Track();

        return _items.GetEnumerator();
    }

    private static Dictionary<TKey, TValue> Empty => [];

    private KeyValuePair<TKey, TValue>[] Snapshot() => [.. _items];

    private bool ContentsEqual(IReadOnlyDictionary<TKey, TValue> items)
    {
        if (items.Count != _items.Count)
        {
            return false;
        }

        foreach (var entry in items)
        {
            if (!_items.TryGetValue(entry.Key, out var held) ||
                !EqualityComparer<TValue>.Default.Equals(held, entry.Value))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Called once an entry went in under a key that held none, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">What went in.</param>
    protected virtual void OnAdded(TKey key, TValue value) { }

    /// <summary>Called once an entry came out, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">What came out.</param>
    protected virtual void OnRemoved(TKey key, TValue value) { }

    /// <summary>Called once a value was replaced, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="key">The key.</param>
    /// <param name="previous">What was kept there.</param>
    /// <param name="current">What is kept there now.</param>
    protected virtual void OnReplaced(TKey key, TValue previous, TValue current) { }

    /// <summary>Called once the contents were swapped wholesale, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="previous">What the map held.</param>
    /// <param name="current">What it holds now.</param>
    protected virtual void OnReset(KeyValuePair<TKey, TValue>[] previous, KeyValuePair<TKey, TValue>[] current) { }

    /// <summary>Puts an entry in without reaching <see cref="OnAdded"/> or <see cref="OnReplaced"/>.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">What to keep there.</param>
    protected void SetSilently(TKey key, TValue value)
    {
        Verify();
        _items[key] = value;
        Notify();
    }

    /// <summary>Takes an entry out without reaching <see cref="OnRemoved"/>.</summary>
    /// <param name="key">The key.</param>
    protected void RemoveSilently(TKey key)
    {
        Verify();
        _items.Remove(key);
        Notify();
    }

    /// <summary>Swaps the contents without reaching <see cref="OnReset"/>.</summary>
    /// <param name="items">What the map should hold.</param>
    protected void ResetSilently(KeyValuePair<TKey, TValue>[] items)
    {
        Verify();
        _items.Clear();

        foreach (var entry in items)
        {
            _items[entry.Key] = entry.Value;
        }

        Notify();
    }

}
