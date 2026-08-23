using System;
using System.Collections.Generic;

namespace Arlecchino.Atoms.History;

/// <summary>
/// An atom whose writes go on the undo stack: a draft, a setting, the selected item. A plain
/// <see cref="Atom{T}"/> is the one whose writes do not.
/// </summary>
/// <typeparam name="T">The kind of value held.</typeparam>
public sealed class HistoryAtom<T> : Atom<T>, IRestorableAtom
{
    /// <summary>Creates an atom that keeps history.</summary>
    /// <param name="initial">The value to start with.</param>
    /// <param name="key">What the atom is called; see <see cref="Key"/>.</param>
    /// <param name="comparer">
    /// How to decide that writing to it changed nothing; the default comparer for <typeparamref name="T"/>
    /// is used when omitted.
    /// </param>
    /// <exception cref="ArgumentException">The name is empty.</exception>
    /// <exception cref="InvalidOperationException">The name is taken by an atom holding something else.</exception>
    public HistoryAtom(T initial, string key, IEqualityComparer<T>? comparer = null)
        : base(initial, comparer)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        Key = key;

        HistoryChanges.Name(key, typeof(T), this);
    }

    /// <summary>
    /// What the atom is called in a <see cref="HistoryFile"/>, written in full rather than worked out:
    /// it is the name a later run finds its atom by, so it outlives any rename. Usually the store and the
    /// property, as <c>"SettingsStore.Server"</c>.
    /// </summary>
    public string Key { get; }

    /// <inheritdoc />
    public IAtomEdit? Restore(IValueReader reader) =>
        reader.Read<T>(out var undone, out var redone) ? new Edit(this, undone, redone) : null;

    /// <inheritdoc />
    protected override void OnWritten(T previous, T current)
    {
        if (HistoryChanges.IsRecording)
        {
            HistoryChanges.NotifyRecorded(new Edit(this, previous, current));
        }
    }

    private sealed class Edit : IWritableAtomEdit
    {
        private readonly HistoryAtom<T> _atom;
        private readonly T _oldValue;
        private readonly T _newValue;

        public Edit(HistoryAtom<T> atom, T oldValue, T newValue)
        {
            _atom = atom;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public object Owner => _atom;

        public string Key => _atom.Key;

        public bool WriteTo(IValueWriter writer) => writer.Write(_oldValue, _newValue);

        public void Undo() => _atom.WriteSilently(_oldValue);

        public void Redo() => _atom.WriteSilently(_newValue);
    }
}
