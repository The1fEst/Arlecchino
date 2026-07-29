using System;
using System.Collections;
using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// A set held as one piece of application state — the files marked, the rows expanded, the hosts
/// reachable. Every change goes through the same path a plain atom's write does: it is checked
/// against the drawing thread, it notifies what reads the set, it marks the frame stale, and it
/// records an undo step when the set is undoable.
///
/// It behaves as a <c>HashSet&lt;T&gt;</c> does rather than as a map: adding what is already there
/// changes nothing and is not an error, which is why <see cref="Add(T)"/> answers nothing and
/// <see cref="TryAdd"/> is there for the times the answer matters.
///
/// <see cref="Value"/> is a live, read-only view. A set has no order, so what a walk of it hands
/// back is whatever the set holds them in — sort it where the order is what the reader sees.
/// Whether changes can be undone is decided by the type created — <see cref="TrackedAtomsSet{T}"/>
/// or <see cref="LocalAtomsSet{T}"/>.
/// </summary>
/// <typeparam name="T">What the set holds.</typeparam>
public abstract class AtomsSet<T> : IReadableAtom<IReadOnlySet<T>>
{
    private readonly HashSet<T> _items;
    private readonly View _view;
    private readonly Listeners _listeners = new();

    private string? _member;

    /// <summary>Creates the set.</summary>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">
    /// How items are compared; the default comparer for <typeparamref name="T"/> is used when
    /// omitted.
    /// </param>
    protected AtomsSet(IReadOnlyList<T>? initial = null, IEqualityComparer<T>? comparer = null)
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

    /// <summary>Whether changes of this set enter the undo history.</summary>
    protected abstract bool RecordsHistory { get; }

    /// <summary>
    /// What the set holds now: a live view rather than a copy, so something handed this once reads
    /// whatever is in it on every later frame. It is read-only all the way down — there is no cast
    /// that gets a caller back to the set underneath.
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

    private bool Recording => RecordsHistory && AtomChanges.IsRecording;

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
        ArgumentNullException.ThrowIfNull(items);

        var added = new List<T>(items.Count);
        var seen = new HashSet<T>(_items.Comparer);

        foreach (var item in items)
        {
            if (!_items.Contains(item) && seen.Add(item))
            {
                added.Add(item);
            }
        }

        if (added.Count == 0)
        {
            return;
        }

        Verify();

        foreach (var item in added)
        {
            _items.Add(item);
        }

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Added(this, [.. added]));
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

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Added(this, [item]));
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

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Removed(this, [item]));
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
        ArgumentNullException.ThrowIfNull(items);

        if (Holds(items))
        {
            return;
        }

        Verify();

        var previous = Snapshot();

        _items.Clear();

        foreach (var item in items)
        {
            _items.Add(item);
        }

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Swapped(this, previous, Snapshot()));
        }

        Notify();
    }

    /// <summary>Calls back whenever the contents change.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener) => _listeners.Add(listener);

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

    private bool Holds(IReadOnlyList<T> items)
    {
        foreach (var item in items)
        {
            if (!_items.Contains(item))
            {
                return false;
            }
        }

        var given = new HashSet<T>(_items.Comparer);

        foreach (var item in items)
        {
            given.Add(item);
        }

        return given.Count == _items.Count;
    }

    private void Track()
    {
        if (AtomTracking.IsCapturing)
        {
            AtomTracking.NoteRead(Subscribe);
        }
    }

    private void Verify() => FrameThread.Verify(_member ??= $"Changing {GetType().Name}");

    private void Notify()
    {
        _listeners.Notify();

        AtomChanges.NotifyWritten();
    }

    private void AddSilently(T[] items)
    {
        Verify();

        foreach (var item in items)
        {
            _items.Add(item);
        }

        Notify();
    }

    private void RemoveSilently(T[] items)
    {
        Verify();

        foreach (var item in items)
        {
            _items.Remove(item);
        }

        Notify();
    }

    private void ResetSilently(T[] items)
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

    private sealed class Added : IAtomEdit
    {
        private readonly AtomsSet<T> _set;
        private readonly T[] _items;

        public Added(AtomsSet<T> set, T[] items)
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
        private readonly AtomsSet<T> _set;
        private readonly T[] _items;

        public Removed(AtomsSet<T> set, T[] items)
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
        private readonly AtomsSet<T> _set;
        private readonly T[] _before;
        private readonly T[] _after;

        public Swapped(AtomsSet<T> set, T[] before, T[] after)
        {
            _set = set;
            _before = before;
            _after = after;
        }

        public object Owner => _set;

        public void Undo() => _set.ResetSilently(_before);

        public void Redo() => _set.ResetSilently(_after);
    }
}
