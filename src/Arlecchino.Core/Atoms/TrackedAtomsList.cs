using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// A list whose changes go on the undo stack: the rows of the document being edited, the tasks of a
/// plan, the marked files. <see cref="AtomHistory"/> picks it up with nothing to register, and each
/// call is one step — a page added with <see cref="AtomsList{T}.Add(IReadOnlyList{T})"/> comes back
/// as a page rather than row by row.
/// </summary>
/// <typeparam name="T">What the list holds.</typeparam>
public sealed class TrackedAtomsList<T> : AtomsList<T>
{
    /// <summary>Creates an undoable list.</summary>
    /// <param name="initial">What it starts with; empty when omitted.</param>
    /// <param name="comparer">How items are compared; the default comparer when omitted.</param>
    public TrackedAtomsList(IReadOnlyList<T>? initial = null, IEqualityComparer<T>? comparer = null)
        : base(initial, comparer) { }

    /// <inheritdoc />
    protected override bool RecordsHistory => true;
}
