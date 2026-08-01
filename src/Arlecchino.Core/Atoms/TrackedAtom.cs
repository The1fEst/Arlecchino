using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// An atom whose edits go on the undo stack: the draft being edited, a setting, the selected item —
/// anything a user changed and may want back. <see cref="AtomHistory"/> picks it up with nothing to
/// register.
/// </summary>
/// <typeparam name="T">Type of the value held.</typeparam>
public sealed class TrackedAtom<T> : Atom<T>
{
    /// <summary>Creates an undoable atom holding a starting value.</summary>
    /// <param name="initial">The value to start with.</param>
    /// <param name="comparer">
    /// How to decide that a write changed nothing; the default comparer for <typeparamref name="T"/>
    /// is used when omitted.
    /// </param>
    public TrackedAtom(T initial, IEqualityComparer<T>? comparer = null)
        : base(initial, comparer) { }

    /// <inheritdoc />
    protected override bool RecordsHistory => true;
}
