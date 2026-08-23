using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// A stack whose changes go on the undo stack. A plain <see cref="AtomsStack{T}"/> is the one whose
/// changes do not.
/// </summary>
/// <typeparam name="T">What the stack holds.</typeparam>
public sealed class HistoryAtomsStack<T> : AtomsStack<T>
{
    /// <summary>Creates a stack that keeps history.</summary>
    /// <param name="initial">What is already on it, top first; empty when omitted. It is copied, not held.</param>
    public HistoryAtomsStack(IReadOnlyList<T>? initial = null) : base(initial) { }

    /// <inheritdoc />
    protected override bool IsObserved => HistoryChanges.IsRecording;

    /// <inheritdoc />
    protected override void OnInserted(int index, T[] items) =>
        HistoryChanges.NotifyRecorded(new Inserted(this, index, items));

    /// <inheritdoc />
    protected override void OnRemoved(int index, T[] items) =>
        HistoryChanges.NotifyRecorded(new Removed(this, index, items));

    /// <inheritdoc />
    protected override void OnReset(T[] previous, T[] current) =>
        HistoryChanges.NotifyRecorded(new Swapped(this, previous, current));

    private sealed class Inserted : IAtomEdit
    {
        private readonly HistoryAtomsStack<T> _stack;
        private readonly int _index;
        private readonly T[] _items;

        public Inserted(HistoryAtomsStack<T> stack, int index, T[] items)
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
        private readonly HistoryAtomsStack<T> _stack;
        private readonly int _index;
        private readonly T[] _items;

        public Removed(HistoryAtomsStack<T> stack, int index, T[] items)
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
        private readonly HistoryAtomsStack<T> _stack;
        private readonly T[] _oldValue;
        private readonly T[] _newValue;

        public Swapped(HistoryAtomsStack<T> stack, T[] oldValue, T[] newValue)
        {
            _stack = stack;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public object Owner => _stack;

        public void Undo() => _stack.ResetSilently(_oldValue);

        public void Redo() => _stack.ResetSilently(_newValue);
    }
}
