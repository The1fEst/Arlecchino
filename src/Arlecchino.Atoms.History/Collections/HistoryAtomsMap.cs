using System;
using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// A map whose changes go on the undo stack. A plain <see cref="AtomsMap{TKey, TValue}"/> is the one whose
/// changes do not.
/// </summary>
/// <typeparam name="TKey">What the entries are looked up by.</typeparam>
/// <typeparam name="TValue">What is kept against each key.</typeparam>
public sealed class HistoryAtomsMap<TKey, TValue>
    : AtomsMap<TKey, TValue>, IRestorableAtom, IHistoryCollection<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    /// <summary>What the base compares keys by, kept here as well because it does not hand its own out.</summary>
    private readonly IEqualityComparer<TKey> _comparer;

    /// <summary>Creates a map that keeps history.</summary>
    /// <param name="key">What the map is called; see <see cref="Key"/>.</param>
    /// <param name="initial">What it starts with; empty when omitted. It is copied, not held.</param>
    /// <param name="comparer">How keys are compared.</param>
    /// <exception cref="InvalidOperationException">The name is taken by an atom holding something else.</exception>
    public HistoryAtomsMap(
        string key,
        IReadOnlyDictionary<TKey, TValue>? initial = null,
        IEqualityComparer<TKey>? comparer = null)
        : base(initial, comparer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _comparer = comparer ?? EqualityComparer<TKey>.Default;

        Key = key;

        HistoryChanges.Name(key, typeof(KeyValuePair<TKey, TValue>[]), this);
    }

    /// <summary>
    /// What the map is called in a <see cref="HistoryFile"/>, written in full — usually the store and the
    /// property — so that it outlives a rename.
    /// </summary>
    public string Key { get; }

    /// <inheritdoc />
    public IAtomEdit? Restore(IValueReader reader) =>
        reader.ReadEntries<TKey, TValue>(out var undone, out var redone)
            ? new RestoredContents<KeyValuePair<TKey, TValue>>(this, undone, redone)
            : null;

    /// <inheritdoc />
    protected override bool IsObserved => HistoryChanges.IsRecording;

    /// <summary>
    /// Makes the map hold what a step says it held, by the smallest change that gets there: an entry both
    /// agree on is left alone, and only what the two do not share moves.
    /// </summary>
    /// <param name="target">The entries to land on.</param>
    void IHistoryCollection<KeyValuePair<TKey, TValue>>.Land(KeyValuePair<TKey, TValue>[] target)
    {
        var contents = Value;
        var wantedEntries = new Dictionary<TKey, TValue>(target.Length, _comparer);

        foreach (var entry in target)
        {
            wantedEntries[entry.Key] = entry.Value;
        }

        var removedKeys = new List<TKey>();

        foreach (var key in contents.Keys)
        {
            if (!wantedEntries.ContainsKey(key))
            {
                removedKeys.Add(key);
            }
        }

        foreach (var key in removedKeys)
        {
            RemoveSilently(key);
        }

        var values = EqualityComparer<TValue>.Default;

        foreach (var entry in wantedEntries)
        {
            if (!contents.TryGetValue(entry.Key, out var value) || !values.Equals(value, entry.Value))
            {
                SetSilently(entry.Key, entry.Value);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnAdded(TKey key, TValue value)
    {
        var redone = Contents.Copied(Value);

        HistoryChanges.NotifyRecorded(new Added(this, key, value, Without(redone, key), redone));
    }

    /// <inheritdoc />
    protected override void OnRemoved(TKey key, TValue value)
    {
        var redone = Contents.Copied(Value);

        HistoryChanges.NotifyRecorded(new Removed(this, key, value, With(redone, key, value), redone));
    }

    /// <inheritdoc />
    protected override void OnReplaced(TKey key, TValue previous, TValue current)
    {
        var redone = Contents.Copied(Value);

        HistoryChanges.NotifyRecorded(
            new Replaced(this, key, previous, current, With(redone, key, previous), redone));
    }

    /// <inheritdoc />
    protected override void OnReset(KeyValuePair<TKey, TValue>[] previous, KeyValuePair<TKey, TValue>[] current) =>
        HistoryChanges.NotifyRecorded(new Swapped(this, previous, current));

    /// <summary>The same entries without one key, which is what they were before it was put in.</summary>
    /// <param name="entries">What the map holds now, or nothing when it was not copied.</param>
    /// <param name="key">The key to leave out.</param>
    /// <returns>The entries without it, or <c>null</c> when there were none to work from.</returns>
    private KeyValuePair<TKey, TValue>[]? Without(KeyValuePair<TKey, TValue>[]? entries, TKey key)
    {
        if (entries is null)
        {
            return null;
        }

        var keptEntries = new List<KeyValuePair<TKey, TValue>>(entries.Length);

        foreach (var entry in entries)
        {
            if (!_comparer.Equals(entry.Key, key))
            {
                keptEntries.Add(entry);
            }
        }

        return [.. keptEntries];
    }

    /// <summary>The same entries with one key kept as it was, whether it was taken out or written over.</summary>
    /// <param name="entries">What the map holds now, or nothing when it was not copied.</param>
    /// <param name="key">The key to put back.</param>
    /// <param name="value">What it was against.</param>
    /// <returns>The entries with it, or <c>null</c> when there were none to work from.</returns>
    private KeyValuePair<TKey, TValue>[]? With(KeyValuePair<TKey, TValue>[]? entries, TKey key, TValue value)
    {
        if (entries is null)
        {
            return null;
        }

        var keptEntries = new List<KeyValuePair<TKey, TValue>>(entries.Length + 1);
        var found = false;

        foreach (var entry in entries)
        {
            if (_comparer.Equals(entry.Key, key))
            {
                found = true;
                keptEntries.Add(new(key, value));
            }
            else
            {
                keptEntries.Add(entry);
            }
        }

        if (!found)
        {
            keptEntries.Add(new(key, value));
        }

        return [.. keptEntries];
    }

    /// <summary>
    /// What every recorded change of the map has in common, which is what a collection's has apart from
    /// the entries going to the file as entries rather than as items.
    /// </summary>
    private abstract class Edit : ContentsEdit<HistoryAtomsMap<TKey, TValue>, KeyValuePair<TKey, TValue>>
    {
        protected Edit(
            HistoryAtomsMap<TKey, TValue> map,
            KeyValuePair<TKey, TValue>[]? undone,
            KeyValuePair<TKey, TValue>[]? redone)
            : base(map, undone, redone) { }

        protected override bool Write(
            IValueWriter writer,
            KeyValuePair<TKey, TValue>[] undone,
            KeyValuePair<TKey, TValue>[] redone) =>
            writer.WriteEntries(undone, redone);
    }

    private sealed class Added : Edit
    {
        private readonly TKey _key;
        private readonly TValue _value;

        public Added(
            HistoryAtomsMap<TKey, TValue> map,
            TKey key,
            TValue value,
            KeyValuePair<TKey, TValue>[]? undone,
            KeyValuePair<TKey, TValue>[]? redone)
            : base(map, undone, redone)
        {
            _key = key;
            _value = value;
        }

        public override void Undo() => Collection.RemoveSilently(_key);

        public override void Redo() => Collection.SetSilently(_key, _value);
    }

    private sealed class Removed : Edit
    {
        private readonly TKey _key;
        private readonly TValue _value;

        public Removed(
            HistoryAtomsMap<TKey, TValue> map,
            TKey key,
            TValue value,
            KeyValuePair<TKey, TValue>[]? undone,
            KeyValuePair<TKey, TValue>[]? redone)
            : base(map, undone, redone)
        {
            _key = key;
            _value = value;
        }

        public override void Undo() => Collection.SetSilently(_key, _value);

        public override void Redo() => Collection.RemoveSilently(_key);
    }

    private sealed class Replaced : Edit
    {
        private readonly TKey _key;
        private readonly TValue _oldValue;
        private readonly TValue _newValue;

        public Replaced(
            HistoryAtomsMap<TKey, TValue> map,
            TKey key,
            TValue oldValue,
            TValue newValue,
            KeyValuePair<TKey, TValue>[]? undone,
            KeyValuePair<TKey, TValue>[]? redone)
            : base(map, undone, redone)
        {
            _key = key;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override void Undo() => Collection.SetSilently(_key, _oldValue);

        public override void Redo() => Collection.SetSilently(_key, _newValue);
    }

    private sealed class Swapped : Edit
    {
        private readonly KeyValuePair<TKey, TValue>[] _oldValue;
        private readonly KeyValuePair<TKey, TValue>[] _newValue;

        public Swapped(
            HistoryAtomsMap<TKey, TValue> map,
            KeyValuePair<TKey, TValue>[] oldValue,
            KeyValuePair<TKey, TValue>[] newValue)
            : base(map, oldValue, newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override void Undo() => Collection.ResetSilently(_oldValue);

        public override void Redo() => Collection.ResetSilently(_newValue);
    }
}
