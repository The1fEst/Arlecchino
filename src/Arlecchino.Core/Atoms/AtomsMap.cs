using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Arlecchino.Atoms;

/// <summary>
/// A map held as one piece of application state — what is known about each server, the settings read
/// so far, a count per kind. Every change goes through the same path a plain atom's write does: it is
/// checked against the drawing thread, it notifies what reads the map, it marks the frame stale, and
/// it records an undo step when the map is undoable.
///
/// It is to <c>Atom&lt;Dictionary&lt;TKey, TValue&gt;&gt;</c> what <see cref="AtomsList{T}"/> is to an
/// atom around a list, and for the same reason: writing into the dictionary inside an atom never
/// reaches <c>Atom.Value</c>, so nothing is notified and no frame is asked for, and writing the same
/// instance back is taken for a change of nothing because a dictionary is compared by reference.
///
/// It holds a dictionary but is not one: there is no <c>IDictionary</c> to write through, because the
/// members below are the only way in and that is what keeps every change on the frame's path.
///
/// Whether changes can be undone is decided by the type created —
/// <see cref="TrackedAtomsMap{TKey, TValue}"/> or <see cref="LocalAtomsMap{TKey, TValue}"/> — exactly
/// as it is for atoms and lists.
/// </summary>
/// <typeparam name="TKey">What the entries are looked up by.</typeparam>
/// <typeparam name="TValue">What is kept against each key.</typeparam>
public abstract class AtomsMap<TKey, TValue> : IReadableAtom<IReadOnlyDictionary<TKey, TValue>>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _items;
    private readonly ReadOnlyDictionary<TKey, TValue> _view;
    private readonly Listeners _listeners = new();

    private string? _member;

    /// <summary>Creates the map.</summary>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">
    /// How keys are compared; the default comparer for <typeparamref name="TKey"/> is used when
    /// omitted. Values are compared with their own default comparer, which is what decides that a
    /// write changed nothing.
    /// </param>
    protected AtomsMap(IReadOnlyDictionary<TKey, TValue>? initial = null, IEqualityComparer<TKey>? comparer = null)
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

    /// <summary>Whether changes of this map enter the undo history.</summary>
    protected abstract bool RecordsHistory { get; }

    /// <summary>
    /// What the map holds now: a live view rather than a copy, so something handed this once reads
    /// whatever is in it on every later frame. It is read-only all the way down, so every change goes
    /// through the members below and is seen by the frame and by the history.
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
    /// writing puts the entry there whether or not it was there before, and writing an equal value
    /// changes nothing and notifies nobody.
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
            var held = _items.TryGetValue(key, out var previous);

            if (held && EqualityComparer<TValue>.Default.Equals(previous, value))
            {
                return;
            }

            Verify();

            _items[key] = value;

            if (Recording)
            {
                AtomChanges.NotifyRecorded(held
                    ? new Replaced(this, key, previous!, value)
                    : new Added(this, key, value));
            }

            Notify();
        }
    }

    private bool Recording => RecordsHistory && AtomChanges.IsRecording;

    /// <summary>
    /// Puts an entry in, and throws when the key is already there — the dictionary's own rule, for the
    /// cases where a second entry under one key means something has gone wrong. Use the indexer to put
    /// one in whether or not it is there already.
    /// </summary>
    /// <param name="key">What to keep it under.</param>
    /// <param name="value">What to keep.</param>
    public void Add(TKey key, TValue value)
    {
        if (_items.ContainsKey(key))
        {
            throw new ArgumentException($"{key} is already in this {GetType().Name}", nameof(key));
        }

        this[key] = value;
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

        this[key] = value;

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

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Removed(this, key, removed));
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
        ArgumentNullException.ThrowIfNull(items);

        if (Holds(items))
        {
            return;
        }

        Verify();

        var previous = Snapshot();

        _items.Clear();

        foreach (var entry in items)
        {
            _items[entry.Key] = entry.Value;
        }

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Swapped(this, previous, Snapshot()));
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

    /// <summary>Calls back whenever the contents change.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener) => _listeners.Add(listener);

    /// <summary>
    /// Walks the entries, so <c>foreach</c> over the map itself reads the way it does over a
    /// dictionary. It is not an <c>IEnumerable</c> — the enumerator is all a <c>foreach</c> asks for,
    /// and stopping there is what keeps the members above the only way to change anything. Reach for
    /// <see cref="Value"/> where a sequence is what is wanted, LINQ included.
    /// </summary>
    /// <returns>The enumerator, which throws when the map changes while it is being walked.</returns>
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        Track();

        return _items.GetEnumerator();
    }

    private static Dictionary<TKey, TValue> Empty => [];

    private KeyValuePair<TKey, TValue>[] Snapshot() => [.. _items];

    private bool Holds(IReadOnlyDictionary<TKey, TValue> items)
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

    private void Track()
    {
        if (AtomTracking.IsCapturing)
        {
            AtomTracking.NoteRead(Subscribe);
        }
    }

    private void Verify() => FrameThread.Verify(_member ??= FrameMembers.Changing(this));

    private void Notify()
    {
        _listeners.Notify();

        AtomChanges.NotifyWritten();
    }

    private void SetSilently(TKey key, TValue value)
    {
        Verify();
        _items[key] = value;
        Notify();
    }

    private void RemoveSilently(TKey key)
    {
        Verify();
        _items.Remove(key);
        Notify();
    }

    private void ResetSilently(KeyValuePair<TKey, TValue>[] items)
    {
        Verify();
        _items.Clear();

        foreach (var entry in items)
        {
            _items[entry.Key] = entry.Value;
        }

        Notify();
    }

    private sealed class Added : IAtomEdit
    {
        private readonly AtomsMap<TKey, TValue> _map;
        private readonly TKey _key;
        private readonly TValue _value;

        public Added(AtomsMap<TKey, TValue> map, TKey key, TValue value)
        {
            _map = map;
            _key = key;
            _value = value;
        }

        public object Owner => _map;

        public void Undo() => _map.RemoveSilently(_key);

        public void Redo() => _map.SetSilently(_key, _value);
    }

    private sealed class Removed : IAtomEdit
    {
        private readonly AtomsMap<TKey, TValue> _map;
        private readonly TKey _key;
        private readonly TValue _value;

        public Removed(AtomsMap<TKey, TValue> map, TKey key, TValue value)
        {
            _map = map;
            _key = key;
            _value = value;
        }

        public object Owner => _map;

        public void Undo() => _map.SetSilently(_key, _value);

        public void Redo() => _map.RemoveSilently(_key);
    }

    private sealed class Replaced : IAtomEdit
    {
        private readonly AtomsMap<TKey, TValue> _map;
        private readonly TKey _key;
        private readonly TValue _before;
        private readonly TValue _after;

        public Replaced(AtomsMap<TKey, TValue> map, TKey key, TValue before, TValue after)
        {
            _map = map;
            _key = key;
            _before = before;
            _after = after;
        }

        public object Owner => _map;

        public void Undo() => _map.SetSilently(_key, _before);

        public void Redo() => _map.SetSilently(_key, _after);
    }

    private sealed class Swapped : IAtomEdit
    {
        private readonly AtomsMap<TKey, TValue> _map;
        private readonly KeyValuePair<TKey, TValue>[] _before;
        private readonly KeyValuePair<TKey, TValue>[] _after;

        public Swapped(
            AtomsMap<TKey, TValue> map,
            KeyValuePair<TKey, TValue>[] before,
            KeyValuePair<TKey, TValue>[] after)
        {
            _map = map;
            _before = before;
            _after = after;
        }

        public object Owner => _map;

        public void Undo() => _map.ResetSilently(_before);

        public void Redo() => _map.ResetSilently(_after);
    }
}
