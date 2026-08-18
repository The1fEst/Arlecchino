using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;

namespace Arlecchino.Atoms.Collections;

/// <summary>
/// A list held as one piece of application state, changed in place. Every change notifies what reads the
/// list, marks the frame stale and records an undo step.
/// </summary>
/// <seealso cref="TrackedAtomsList{T}"/>
/// <seealso cref="LocalAtomsList{T}"/>
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
    /// How <see cref="Remove"/> finds an item, and how writing to the indexer decides it changed
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
    /// What the list holds now, as a live view rather than a copy. It is read-only all the way down, so
    /// every change goes through the members below.
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

    /// <summary>The item at a position. Writing an equal item changes nothing and notifies no one.</summary>
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
    /// Takes out several items in a row at once, with one notification, one frame and one undo step for the
    /// lot. Trimming a list one item at a time would come back the same way.
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

        var removedItems = Recording ? _items.GetRange(index, count).ToArray() : [];

        _items.RemoveRange(index, count);

        if (removedItems.Length > 0)
        {
            AtomChanges.NotifyRecorded(new Removed(this, index, removedItems));
        }

        Notify();
    }

    /// <summary>Takes everything out. An empty list changes nothing.</summary>
    public void Clear() => Reset([]);

    /// <summary>
    /// Says that an item already in the list changed inside itself, so everything watching the list hears
    /// about it. Replace the item instead, unless its identity has to survive the change.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called from off the drawing thread.</exception>
    public void Touch()
    {
        Verify();
        Notify();
    }

    /// <summary>
    /// Replaces the contents in one go, for the case the list is not edited but reloaded — a query
    /// answered, a folder read again, a filter applied. Contents equal to what is already there
    /// change nothing.
    /// </summary>
    /// <param name="items">What the list should hold instead.</param>
    public void Reset(IReadOnlyList<T> items)
    {
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
    /// Walks what the list holds, so <c>foreach</c> over the list itself reads the way it does over a list.
    /// Reach for <see cref="Value"/> where a sequence is wanted, LINQ included.
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
        private readonly T _oldValue;
        private readonly T _newValue;

        public Replaced(AtomsList<T> list, int index, T oldValue, T newValue)
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
        private readonly AtomsList<T> _list;
        private readonly T[] _oldValue;
        private readonly T[] _newValue;

        public Swapped(AtomsList<T> list, T[] oldValue, T[] newValue)
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
