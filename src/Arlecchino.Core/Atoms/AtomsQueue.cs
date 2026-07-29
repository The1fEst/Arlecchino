using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Arlecchino.Atoms;

/// <summary>
/// A queue held as one piece of application state — work waiting to be done, files still to copy,
/// commands typed ahead. Things join at the back and leave from the front, and every change goes
/// through the same path a plain atom's write does: it is checked against the drawing thread, it
/// notifies what reads the queue, it marks the frame stale, and it records an undo step when the
/// queue is undoable.
///
/// It is what a <c>ConcurrentQueue&lt;T&gt;</c> is not for: nothing here is thread-safe, because
/// nothing needs to be. Background work hands its item over with <c>FrameThread.Post</c> and the
/// queue is only ever touched by the thread that draws it, which is what lets a frame read it
/// several times and see the same thing each time.
///
/// <see cref="Value"/> is the contents in order, front first, so a view draws the queue by walking
/// it. Whether changes can be undone is decided by the type created —
/// <see cref="TrackedAtomsQueue{T}"/> or <see cref="LocalAtomsQueue{T}"/>.
/// </summary>
/// <typeparam name="T">What waits in the queue.</typeparam>
public abstract class AtomsQueue<T> : IReadableAtom<IReadOnlyList<T>>
{
    private readonly List<T> _items;
    private readonly ReadOnlyCollection<T> _view;
    private readonly Listeners _listeners = new();

    private string? _member;

    /// <summary>Creates the queue.</summary>
    /// <param name="initial">What is already waiting, front first; empty when omitted. It is copied, not held.</param>
    protected AtomsQueue(IReadOnlyList<T>? initial = null)
    {
        _items = initial is null ? [] : [.. initial];
        _view = _items.AsReadOnly();
    }

    /// <summary>Whether changes of this queue enter the undo history.</summary>
    protected abstract bool RecordsHistory { get; }

    /// <summary>
    /// What is waiting now, front first: a live view rather than a copy, so a widget handed this once
    /// draws whatever is in the queue on every later frame. It is read-only all the way down, so
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

    /// <summary>How many are waiting.</summary>
    public int Count
    {
        get
        {
            Track();
            return _items.Count;
        }
    }

    /// <summary>Whether nothing is waiting.</summary>
    public bool IsEmpty => Count == 0;

    private bool Recording => RecordsHistory && AtomChanges.IsRecording;

    /// <summary>Puts something at the back.</summary>
    /// <param name="item">What to add.</param>
    public void Enqueue(T item)
    {
        Verify();

        _items.Add(item);

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Inserted(this, _items.Count - 1, [item]));
        }

        Notify();
    }

    /// <summary>
    /// Puts several at the back at once, in the order given. One notification, one frame and one undo
    /// step for the lot.
    /// </summary>
    /// <param name="items">What to add. Adding none changes nothing.</param>
    public void Enqueue(IReadOnlyList<T> items)
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

    /// <summary>Takes the one at the front, and throws when nothing is waiting.</summary>
    /// <returns>What was at the front.</returns>
    public T Dequeue()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException($"{GetType().Name} is empty");
        }

        return Take();
    }

    /// <summary>Takes the one at the front without throwing when nothing is waiting.</summary>
    /// <param name="item">What was at the front, when there was one.</param>
    /// <returns><c>true</c> when something was taken.</returns>
    public bool TryDequeue([MaybeNullWhen(false)] out T item)
    {
        if (_items.Count == 0)
        {
            item = default;
            return false;
        }

        item = Take();
        return true;
    }

    /// <summary>Reads the one at the front without taking it, and throws when nothing is waiting.</summary>
    /// <returns>What is at the front.</returns>
    public T Peek()
    {
        Track();

        return _items.Count > 0 ? _items[0] : throw new InvalidOperationException($"{GetType().Name} is empty");
    }

    /// <summary>Reads the one at the front without taking it or throwing.</summary>
    /// <param name="item">What is at the front, when there is one.</param>
    /// <returns><c>true</c> when there was one.</returns>
    public bool TryPeek([MaybeNullWhen(false)] out T item)
    {
        Track();

        if (_items.Count == 0)
        {
            item = default;
            return false;
        }

        item = _items[0];
        return true;
    }

    /// <summary>Drops everything waiting. An empty queue changes nothing.</summary>
    public void Clear() => Reset([]);

    /// <summary>
    /// Replaces what is waiting in one go, for a queue that is rebuilt rather than worked through —
    /// a plan worked out again, a batch reordered. Contents equal to what is already there change
    /// nothing.
    /// </summary>
    /// <param name="items">What should be waiting instead, front first.</param>
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

    /// <summary>Calls back whenever what is waiting changes.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener) => _listeners.Add(listener);

    /// <summary>
    /// Walks what is waiting, front first, so <c>foreach</c> over the queue itself reads the way it
    /// does over a list. Reach for <see cref="Value"/> where a sequence is what is wanted, LINQ
    /// included.
    /// </summary>
    /// <returns>The enumerator, which throws when the queue changes while it is being walked.</returns>
    public List<T>.Enumerator GetEnumerator()
    {
        Track();

        return _items.GetEnumerator();
    }

    private T Take()
    {
        Verify();

        var taken = _items[0];

        _items.RemoveAt(0);

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Removed(this, 0, [taken]));
        }

        Notify();

        return taken;
    }

    private bool Holds(IReadOnlyList<T> items)
    {
        if (items.Count != _items.Count)
        {
            return false;
        }

        for (var index = 0; index < items.Count; index++)
        {
            if (!EqualityComparer<T>.Default.Equals(_items[index], items[index]))
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

    private void Verify() => FrameThread.Verify(_member ??= $"Changing {GetType().Name}");

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

    private void ResetSilently(T[] items)
    {
        Verify();
        _items.Clear();
        _items.AddRange(items);
        Notify();
    }

    private sealed class Inserted : IAtomEdit
    {
        private readonly AtomsQueue<T> _queue;
        private readonly int _index;
        private readonly T[] _items;

        public Inserted(AtomsQueue<T> queue, int index, T[] items)
        {
            _queue = queue;
            _index = index;
            _items = items;
        }

        public object Owner => _queue;

        public void Undo() => _queue.RemoveSilently(_index, _items.Length);

        public void Redo() => _queue.InsertSilently(_index, _items);
    }

    private sealed class Removed : IAtomEdit
    {
        private readonly AtomsQueue<T> _queue;
        private readonly int _index;
        private readonly T[] _items;

        public Removed(AtomsQueue<T> queue, int index, T[] items)
        {
            _queue = queue;
            _index = index;
            _items = items;
        }

        public object Owner => _queue;

        public void Undo() => _queue.InsertSilently(_index, _items);

        public void Redo() => _queue.RemoveSilently(_index, _items.Length);
    }

    private sealed class Swapped : IAtomEdit
    {
        private readonly AtomsQueue<T> _queue;
        private readonly T[] _before;
        private readonly T[] _after;

        public Swapped(AtomsQueue<T> queue, T[] before, T[] after)
        {
            _queue = queue;
            _before = before;
            _after = after;
        }

        public object Owner => _queue;

        public void Undo() => _queue.ResetSilently(_before);

        public void Redo() => _queue.ResetSilently(_after);
    }
}
