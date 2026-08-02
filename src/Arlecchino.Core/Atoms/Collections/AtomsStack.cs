using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;

namespace Arlecchino.Atoms.Collections;

/// <summary>
/// A stack held as one piece of application state — where the user has been, the modals over one
/// another, a plan being unwound. Things go on top and come off the top, and every change goes
/// through the same path a plain atom's write does: it is checked against the drawing thread, it
/// notifies what reads the stack, it marks the frame stale, and it records an undo step when the
/// stack is undoable.
///
/// It is what a <c>ConcurrentStack&lt;T&gt;</c> is not for: nothing here is thread-safe, because
/// nothing needs to be. Background work hands its item over with <c>FrameThread.Post</c> and the
/// stack is only ever touched by the thread that draws it.
///
/// <see cref="Value"/> reads from the top down, the way <c>Stack&lt;T&gt;</c> itself enumerates, so
/// <c>Value[0]</c> is what <see cref="Peek"/> answers. Whether changes can be undone is decided by
/// the type created — <see cref="TrackedAtomsStack{T}"/> or <see cref="LocalAtomsStack{T}"/>.
/// </summary>
/// <typeparam name="T">What the stack holds.</typeparam>
public abstract class AtomsStack<T> : IReadableAtom<IReadOnlyList<T>>
{
    private readonly List<T> _items;
    private readonly ReadOnlyCollection<T> _view;
    private readonly Listeners _listeners = new();

    private string? _member;

    /// <summary>Creates the stack.</summary>
    /// <param name="initial">What is already on it, top first; empty when omitted. It is copied, not held.</param>
    protected AtomsStack(IReadOnlyList<T>? initial = null)
    {
        _items = initial is null ? [] : [.. initial];
        _view = _items.AsReadOnly();
    }

    /// <summary>Whether changes of this stack enter the undo history.</summary>
    protected abstract bool RecordsHistory { get; }

    /// <summary>
    /// What is on the stack now, top first: a live view rather than a copy, so a widget handed this
    /// once draws whatever is on it on every later frame. It is read-only all the way down, so every
    /// change goes through the members below.
    /// </summary>
    public IReadOnlyList<T> Value
    {
        get
        {
            Track();
            return _view;
        }
    }

    /// <summary>How many are on it.</summary>
    public int Count
    {
        get
        {
            Track();
            return _items.Count;
        }
    }

    /// <summary>Whether nothing is on it.</summary>
    public bool IsEmpty => Count == 0;

    private bool Recording => RecordsHistory && AtomChanges.IsRecording;

    /// <summary>Puts something on top.</summary>
    /// <param name="item">What to put on.</param>
    public void Push(T item)
    {
        Verify();

        _items.Insert(0, item);

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Inserted(this, 0, [item]));
        }

        Notify();
    }

    /// <summary>
    /// Puts several on at once, the last of them ending up on top — what a loop of
    /// <see cref="Push(T)"/> would leave, in one notification and one undo step.
    /// </summary>
    /// <param name="items">What to put on. Putting none on changes nothing.</param>
    public void Push(IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            return;
        }

        Verify();

        var topFirst = new T[items.Count];

        for (var index = 0; index < items.Count; index++)
        {
            topFirst[items.Count - 1 - index] = items[index];
        }

        _items.InsertRange(0, topFirst);

        if (Recording)
        {
            AtomChanges.NotifyRecorded(new Inserted(this, 0, topFirst));
        }

        Notify();
    }

    /// <summary>Takes the one on top, and throws when the stack is empty.</summary>
    /// <returns>What was on top.</returns>
    public T Pop()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException($"{GetType().Name} is empty");
        }

        return Take();
    }

    /// <summary>Takes the one on top without throwing when the stack is empty.</summary>
    /// <param name="item">What was on top, when there was one.</param>
    /// <returns><c>true</c> when something was taken.</returns>
    public bool TryPop([MaybeNullWhen(false)] out T item)
    {
        if (_items.Count == 0)
        {
            item = default;
            return false;
        }

        item = Take();
        return true;
    }

    /// <summary>Reads the one on top without taking it, and throws when the stack is empty.</summary>
    /// <returns>What is on top.</returns>
    public T Peek()
    {
        Track();

        return _items.Count > 0 ? _items[0] : throw new InvalidOperationException($"{GetType().Name} is empty");
    }

    /// <summary>Reads the one on top without taking it or throwing.</summary>
    /// <param name="item">What is on top, when there is one.</param>
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

    /// <summary>Takes everything off. An empty stack changes nothing.</summary>
    public void Clear() => Reset([]);

    /// <summary>
    /// Replaces what is on the stack in one go, for a stack that is rebuilt rather than unwound.
    /// Contents equal to what is already there change nothing.
    /// </summary>
    /// <param name="items">What should be on it instead, top first.</param>
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

    /// <summary>Calls back whenever what is on the stack changes.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener) => _listeners.Add(listener);

    /// <summary>
    /// Walks the stack from the top down, so <c>foreach</c> over it reads the way it does over a
    /// <c>Stack&lt;T&gt;</c>. Reach for <see cref="Value"/> where a sequence is what is wanted, LINQ
    /// included.
    /// </summary>
    /// <returns>The enumerator, which throws when the stack changes while it is being walked.</returns>
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

    private void ResetSilently(T[] items)
    {
        Verify();
        _items.Clear();
        _items.AddRange(items);
        Notify();
    }

    private sealed class Inserted : IAtomEdit
    {
        private readonly AtomsStack<T> _stack;
        private readonly int _index;
        private readonly T[] _items;

        public Inserted(AtomsStack<T> stack, int index, T[] items)
        {
            _stack = stack;
            _index = index;
            _items = items;
        }

        public object Owner => _stack;

        public void Undo() => _stack.RemoveSilently(_index, _items.Length);

        public void Redo() => _stack.InsertSilently(_index, _items);
    }

    private sealed class Removed : IAtomEdit
    {
        private readonly AtomsStack<T> _stack;
        private readonly int _index;
        private readonly T[] _items;

        public Removed(AtomsStack<T> stack, int index, T[] items)
        {
            _stack = stack;
            _index = index;
            _items = items;
        }

        public object Owner => _stack;

        public void Undo() => _stack.InsertSilently(_index, _items);

        public void Redo() => _stack.RemoveSilently(_index, _items.Length);
    }

    private sealed class Swapped : IAtomEdit
    {
        private readonly AtomsStack<T> _stack;
        private readonly T[] _before;
        private readonly T[] _after;

        public Swapped(AtomsStack<T> stack, T[] before, T[] after)
        {
            _stack = stack;
            _before = before;
            _after = after;
        }

        public object Owner => _stack;

        public void Undo() => _stack.ResetSilently(_before);

        public void Redo() => _stack.ResetSilently(_after);
    }
}
