using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.Tracked;

/// <summary>
/// A list whose changes go on the undo stack, picked up by <see cref="AtomHistory"/> with nothing to
/// register. Each call is one step, so a page added at once comes back as a page.
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
