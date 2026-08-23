using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// A map whose changes go on the undo stack. A plain <see cref="AtomsMap{TKey, TValue}"/> is the one whose
/// changes do not.
/// </summary>
/// <typeparam name="TKey">What the entries are looked up by.</typeparam>
/// <typeparam name="TValue">What is kept against each key.</typeparam>
public sealed class HistoryAtomsMap<TKey, TValue> : AtomsMap<TKey, TValue>
    where TKey : notnull
{
    /// <summary>Creates a map that keeps history.</summary>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">How keys are compared.</param>
    public HistoryAtomsMap(
        IReadOnlyDictionary<TKey, TValue>? initial = null,
        IEqualityComparer<TKey>? comparer = null)
        : base(initial, comparer) { }

    /// <inheritdoc />
    protected override bool IsObserved => HistoryChanges.IsRecording;

    /// <inheritdoc />
    protected override void OnAdded(TKey key, TValue value) =>
        HistoryChanges.NotifyRecorded(new Added(this, key, value));

    /// <inheritdoc />
    protected override void OnRemoved(TKey key, TValue value) =>
        HistoryChanges.NotifyRecorded(new Removed(this, key, value));

    /// <inheritdoc />
    protected override void OnReplaced(TKey key, TValue previous, TValue current) =>
        HistoryChanges.NotifyRecorded(new Replaced(this, key, previous, current));

    /// <inheritdoc />
    protected override void OnReset(KeyValuePair<TKey, TValue>[] previous, KeyValuePair<TKey, TValue>[] current) =>
        HistoryChanges.NotifyRecorded(new Swapped(this, previous, current));

    private sealed class Added : IAtomEdit
    {
        private readonly HistoryAtomsMap<TKey, TValue> _map;
        private readonly TKey _key;
        private readonly TValue _value;

        public Added(HistoryAtomsMap<TKey, TValue> map, TKey key, TValue value)
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
        private readonly HistoryAtomsMap<TKey, TValue> _map;
        private readonly TKey _key;
        private readonly TValue _value;

        public Removed(HistoryAtomsMap<TKey, TValue> map, TKey key, TValue value)
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
        private readonly HistoryAtomsMap<TKey, TValue> _map;
        private readonly TKey _key;
        private readonly TValue _oldValue;
        private readonly TValue _newValue;

        public Replaced(HistoryAtomsMap<TKey, TValue> map, TKey key, TValue oldValue, TValue newValue)
        {
            _map = map;
            _key = key;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public object Owner => _map;

        public void Undo() => _map.SetSilently(_key, _oldValue);

        public void Redo() => _map.SetSilently(_key, _newValue);
    }

    private sealed class Swapped : IAtomEdit
    {
        private readonly HistoryAtomsMap<TKey, TValue> _map;
        private readonly KeyValuePair<TKey, TValue>[] _oldValue;
        private readonly KeyValuePair<TKey, TValue>[] _newValue;

        public Swapped(
            HistoryAtomsMap<TKey, TValue> map,
            KeyValuePair<TKey, TValue>[] oldValue,
            KeyValuePair<TKey, TValue>[] newValue)
        {
            _map = map;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public object Owner => _map;

        public void Undo() => _map.ResetSilently(_oldValue);

        public void Redo() => _map.ResetSilently(_newValue);
    }
}
