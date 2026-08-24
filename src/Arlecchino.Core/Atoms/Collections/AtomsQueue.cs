using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Arlecchino.Atoms.Collections;

/// <summary>
/// A queue held as one piece of application state, joined at the back and left from the front. It is touched
/// only by the drawing thread, so background work hands its item over with <c>FrameThread.Post</c>.
/// </summary>
/// <typeparam name="T">What waits in the queue.</typeparam>
public class AtomsQueue<T> : AtomsCollectionBase, IReadableAtom<IReadOnlyList<T>>
{
    private readonly List<T> _items;
    private readonly ReadOnlyCollection<T> _view;

    /// <summary>Creates the queue.</summary>
    /// <param name="initial">What is already waiting, front first; empty when omitted. It is copied, not held.</param>
    public AtomsQueue(IReadOnlyList<T>? initial = null)
    {
        _items = initial is null ? [] : [.. initial];
        _view = _items.AsReadOnly();
    }

    /// <summary>
    /// What is waiting now, front first, as a live view rather than a copy. It is read-only all the way
    /// down, so every change goes through the members below.
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

    /// <summary>Puts something at the back.</summary>
    /// <param name="item">What to add.</param>
    public void Enqueue(T item)
    {
        Verify();

        _items.Add(item);

        if (IsObserved)
        {
            OnInserted(_items.Count - 1, [item]);
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
        if (items.Count == 0)
        {
            return;
        }

        Verify();

        var index = _items.Count;

        _items.AddRange(items);

        if (IsObserved)
        {
            OnInserted(index, [.. items]);
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
        if (ContentsEqual(items))
        {
            return;
        }

        Verify();

        var previous = IsObserved ? _items.ToArray() : [];

        _items.Clear();
        _items.AddRange(items);

        if (IsObserved)
        {
            OnReset(previous, [.. items]);
        }

        Notify();
    }

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

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Join(",", _items);
    }

    private T Take()
    {
        Verify();

        var item = _items[0];

        _items.RemoveAt(0);

        if (IsObserved)
        {
            OnRemoved(0, [item]);
        }

        Notify();

        return item;
    }

    private bool ContentsEqual(IReadOnlyList<T> items)
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

    /// <summary>Called once items joined, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="index">Where they joined, counting from the front.</param>
    /// <param name="items">What joined, front first.</param>
    protected virtual void OnInserted(int index, T[] items) { }

    /// <summary>Called once items left, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="index">Where they were, counting from the front.</param>
    /// <param name="items">What left, front first.</param>
    protected virtual void OnRemoved(int index, T[] items) { }

    /// <summary>Called once the contents were swapped wholesale, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="previous">What was waiting, front first.</param>
    /// <param name="current">What is waiting now, front first.</param>
    protected virtual void OnReset(T[] previous, T[] current) { }

    /// <summary>Puts items in without reaching <see cref="OnInserted"/>.</summary>
    /// <param name="index">Where they go, counting from the front.</param>
    /// <param name="items">What to put in, front first.</param>
    protected void InsertSilently(int index, T[] items)
    {
        Verify();
        _items.InsertRange(index, items);
        Notify();
    }

    /// <summary>Takes items out without reaching <see cref="OnRemoved"/>.</summary>
    /// <param name="index">Where to start, counting from the front.</param>
    /// <param name="count">How many.</param>
    protected void RemoveSilently(int index, int count)
    {
        Verify();
        _items.RemoveRange(index, count);
        Notify();
    }

    /// <summary>Swaps the contents without reaching <see cref="OnReset"/>.</summary>
    /// <param name="items">What the queue should hold, front first.</param>
    protected void ResetSilently(T[] items)
    {
        Verify();
        _items.Clear();
        _items.AddRange(items);
        Notify();
    }
}
