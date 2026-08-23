using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Arlecchino.Atoms.Collections;

/// <summary>
/// A stack held as one piece of application state, put on and taken off the top. <see cref="Value"/> reads
/// from the top down, so <c>Value[0]</c> is what <see cref="Peek"/> answers.
/// </summary>
/// <typeparam name="T">What the stack holds.</typeparam>
public class AtomsStack<T> : AtomsCollectionBase, IReadableAtom<IReadOnlyList<T>>
{
    private readonly List<T> _items;
    private readonly ReadOnlyCollection<T> _view;

    /// <summary>Creates the stack.</summary>
    /// <param name="initial">What is already on it, top first; empty when omitted. It is copied, not held.</param>
    public AtomsStack(IReadOnlyList<T>? initial = null)
    {
        _items = initial is null ? [] : [.. initial];
        _view = _items.AsReadOnly();
    }

    /// <summary>
    /// What is on the stack now, top first, as a live view rather than a copy. It is read-only all the way
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

    /// <summary>Puts something on top.</summary>
    /// <param name="item">What to put on.</param>
    public void Push(T item)
    {
        Verify();

        _items.Insert(0, item);

        if (IsObserved)
        {
            OnInserted(0, [item]);
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

        if (IsObserved)
        {
            OnInserted(0, topFirst);
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

    /// <summary>Called once items went on, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="index">Where they went, counting from the top.</param>
    /// <param name="items">What went on, top first.</param>
    protected virtual void OnInserted(int index, T[] items) { }

    /// <summary>Called once items came off, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="index">Where they were, counting from the top.</param>
    /// <param name="items">What came off, top first.</param>
    protected virtual void OnRemoved(int index, T[] items) { }

    /// <summary>Called once the contents were swapped wholesale, and only while <see cref="AtomsCollectionBase.IsObserved"/>.</summary>
    /// <param name="previous">What was on the stack, top first.</param>
    /// <param name="current">What is on it now, top first.</param>
    protected virtual void OnReset(T[] previous, T[] current) { }

    /// <summary>Puts items on without reaching <see cref="OnInserted"/>.</summary>
    /// <param name="index">Where they go, counting from the top.</param>
    /// <param name="items">What to put on, top first.</param>
    protected void InsertSilently(int index, T[] items)
    {
        Verify();
        _items.InsertRange(index, items);
        Notify();
    }

    /// <summary>Takes items off without reaching <see cref="OnRemoved"/>.</summary>
    /// <param name="index">Where to start, counting from the top.</param>
    /// <param name="count">How many.</param>
    protected void RemoveSilently(int index, int count)
    {
        Verify();
        _items.RemoveRange(index, count);
        Notify();
    }

    /// <summary>Swaps the contents without reaching <see cref="OnReset"/>.</summary>
    /// <param name="items">What the stack should hold, top first.</param>
    protected void ResetSilently(T[] items)
    {
        Verify();
        _items.Clear();
        _items.AddRange(items);
        Notify();
    }
}
