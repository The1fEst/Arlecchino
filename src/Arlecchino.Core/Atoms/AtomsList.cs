using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Arlecchino.Atoms;

/// <summary>
/// A list held as one piece of application state. Every change goes through the same path a plain
/// atom's write does — it is checked against the drawing thread, it notifies what reads the list, it
/// marks the frame stale, and it records an undo step when the list is undoable.
///
/// This is what an <c>Atom&lt;List&lt;T&gt;&gt;</c> cannot be. Adding to a list held in an ordinary
/// atom never reaches <c>Atom.Value</c>, so nothing is notified and no frame is asked for; writing
/// the same instance back does not help either, because an atom compares by the default comparer and
/// a list is compared by reference, so the write is taken for a change of nothing and dropped. Hold
/// an <c>Atom&lt;IReadOnlyList&lt;T&gt;&gt;</c> and replace it wholesale, or hold this and change it
/// in place.
///
/// Which of the two to reach for is a question of size and rate: replacing a list of a few settings
/// on a keystroke costs nothing, while a log appended to line by line copies the whole of itself on
/// every line. Whether edits can be undone is decided by the type created —
/// <see cref="TrackedAtomsList{T}"/> or <see cref="LocalAtomsList{T}"/> — exactly as it is for atoms.
/// </summary>
/// <typeparam name="T">What the list holds.</typeparam>
public abstract class AtomsList<T> : IReadableAtom<IReadOnlyList<T>>
{
    private readonly List<T> _items;
    private readonly ReadOnlyCollection<T> _view;
    private readonly IEqualityComparer<T> _comparer;
    private readonly Listeners _listeners = new();

    private string? _member;

    /// <summary>Creates the list.</summary>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">
    /// How <see cref="Remove"/> finds an item and how a write to the indexer decides it changed
    /// nothing; the default comparer for <typeparamref name="T"/> is used when omitted.
    /// </param>
    protected AtomsList(IReadOnlyList<T>? initial = null, IEqualityComparer<T>? comparer = null)
    {
        _items = initial is null ? [] : [.. initial];
        _view = _items.AsReadOnly();
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    /// <summary>Whether changes of this list enter the undo history.</summary>
    protected abstract bool RecordsHistory { get; }

    /// <summary>
    /// What the list holds now: a live view rather than a copy, so a widget handed this once draws
    /// whatever is in it on every later frame, and handing it out costs nothing. It is read-only all
    /// the way down — there is no cast that gets a caller back to the list underneath — so every
    /// change goes through the members below and is seen by the frame and by the history.
    /// </summary>
    public IReadOnlyList<T> Value
    {
        get
        {
            Track();
            return _view;
        }
    }

    /// <summary>How many items there are.</summary>
    public int Count
    {
        get
        {
            Track();
            return _items.Count;
        }
    }

    /// <summary>The item at a position. Writing an equal item changes nothing and notifies nobody.</summary>
    /// <param name="index">Which one.</param>
    public T this[int index]
    {
        get
        {
            Track();
            return _items[index];
        }

        set
        {
            if (_comparer.Equals(_items[index], value))
            {
                return;
            }

            Verify();

            var previous = _items[index];
            _items[index] = value;

            if (Recording)
            {
                AtomChanges.NotifyRecorded(new Replaced(this, index, previous, value));
            }

            Notify();
        }
    }

    private bool Recording => RecordsHistory && AtomChanges.IsRecording;

    /// <summary>Puts an item at the end.</summary>
    /// <param name="item">What to add.</param>
    public void Add(T item) => Insert(_items.Count, item);

    /// <summary>
    /// Puts several items at the end at once. One notification, one frame and one undo step for the
    /// lot, which is what a loop of <see cref="Add(T)"/> cannot give — that would undo a page of
    /// rows one row at a time.
    /// </summary>
    /// <param name="items">What to add. Adding none changes nothing.</param>
    public void Add(IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            return;
        }

        Verify();

        var index = _items.Count;

        _items.AddRange(items);

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Inserted(this, index, [.. items]));
        }

        Notify();
    }

    /// <summary>Puts an item at a position, moving the rest along.</summary>
    /// <param name="index">Where it goes.</param>
    /// <param name="item">What to insert.</param>
    public void Insert(int index, T item)
    {
        Verify();

        _items.Insert(index, item);

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Inserted(this, index, [item]));
        }

        Notify();
    }

    /// <summary>Takes out the first item equal to this one, and does nothing when there is none.</summary>
    /// <param name="item">What to take out.</param>
    public void Remove(T item)
    {
        var index = IndexOf(item);

        if (index >= 0)
        {
            RemoveAt(index);
        }
    }

    /// <summary>Takes out the item at a position.</summary>
    /// <param name="index">Which one.</param>
    public void RemoveAt(int index) => RemoveRange(index, 1);

    /// <summary>
    /// Takes out several items in a row at once. One notification, one frame and one undo step for the
    /// lot — which is what trimming a list that has grown too long needs, since doing it one item at a
    /// time would notify once per item and come back the same way.
    /// </summary>
    /// <param name="index">Where to start.</param>
    /// <param name="count">How many to take out. Taking none changes nothing.</param>
    public void RemoveRange(int index, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index + count, _items.Count);

        if (count == 0)
        {
            return;
        }

        Verify();

        var removed = Recording ? _items.GetRange(index, count).ToArray() : [];

        _items.RemoveRange(index, count);

        if (removed.Length > 0)
        {
            AtomChanges.NotifyRecorded(new Removed(this, index, removed));
        }

        Notify();
    }

    /// <summary>Takes everything out. An empty list changes nothing.</summary>
    public void Clear() => Reset([]);

    /// <summary>
    /// Replaces the contents in one go, for the case the list is not edited but reloaded — a query
    /// answered, a folder read again, a filter applied. Contents equal to what is already there
    /// change nothing.
    /// </summary>
    /// <param name="items">What the list should hold instead.</param>
    public void Reset(IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (Holds(items))
        {
            return;
        }

        Verify();

        var previous = _items.ToArray();

        _items.Clear();
        _items.AddRange(items);

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Swapped(this, previous, [.. items]));
        }

        Notify();
    }

    /// <summary>Where an item is, or <c>-1</c> when the list does not hold it.</summary>
    /// <param name="item">What to look for.</param>
    /// <returns>The position of the first item equal to it.</returns>
    public int IndexOf(T item)
    {
        Track();

        for (var index = 0; index < _items.Count; index++)
        {
            if (_comparer.Equals(_items[index], item))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>Calls back whenever the contents change.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener) => _listeners.Add(listener);

    /// <summary>
    /// Walks what the list holds, so <c>foreach</c> over the list itself reads the way it does over a
    /// list. It is not an <c>IEnumerable&lt;T&gt;</c> — the enumerator is all a <c>foreach</c> asks
    /// for, and stopping there is what keeps the members above the only way to change anything. Reach
    /// for <see cref="Value"/> where a sequence is what is wanted, LINQ included.
    /// </summary>
    /// <returns>The enumerator, which throws when the list changes while it is being walked.</returns>
    public List<T>.Enumerator GetEnumerator()
    {
        Track();

        return _items.GetEnumerator();
    }

    private bool Holds(IReadOnlyList<T> items)
    {
        if (items.Count != _items.Count)
        {
            return false;
        }

        for (var index = 0; index < items.Count; index++)
        {
            if (!_comparer.Equals(_items[index], items[index]))
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

    private void InsertSilently(int index, T[] items)
    {
        Verify();
        _items.InsertRange(index, items);
        Notify();
    }

    private void RemoveSilently(int index, int count)
    {
        Verify();
        _items.RemoveRange(index, count);
        Notify();
    }

    private void SetSilently(int index, T item)
    {
        Verify();
        _items[index] = item;
        Notify();
    }

    private void ResetSilently(T[] items)
    {
        Verify();
        _items.Clear();
        _items.AddRange(items);
        Notify();
    }

    private sealed class Inserted : IAtomEdit
    {
        private readonly AtomsList<T> _list;
        private readonly int _index;
        private readonly T[] _items;

        public Inserted(AtomsList<T> list, int index, T[] items)
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
        private readonly AtomsList<T> _list;
        private readonly int _index;
        private readonly T[] _items;

        public Removed(AtomsList<T> list, int index, T[] items)
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
        private readonly AtomsList<T> _list;
        private readonly int _index;
        private readonly T _before;
        private readonly T _after;

        public Replaced(AtomsList<T> list, int index, T before, T after)
        {
            _list = list;
            _index = index;
            _before = before;
            _after = after;
        }

        public object Owner => _list;

        public void Undo() => _list.SetSilently(_index, _before);

        public void Redo() => _list.SetSilently(_index, _after);
    }

    private sealed class Swapped : IAtomEdit
    {
        private readonly AtomsList<T> _list;
        private readonly T[] _before;
        private readonly T[] _after;

        public Swapped(AtomsList<T> list, T[] before, T[] after)
        {
            _list = list;
            _before = before;
            _after = after;
        }

        public object Owner => _list;

        public void Undo() => _list.ResetSilently(_before);

        public void Redo() => _list.ResetSilently(_after);
    }
}
