using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.Tracked;

/// <summary>
/// A set whose changes go on the undo stack, picked up by <see cref="AtomHistory"/> with nothing to
/// register. Each call is one step, including one that puts several in at once.
/// </summary>
/// <typeparam name="T">What the set holds.</typeparam>
public sealed class TrackedAtomsSet<T> : AtomsSet<T>
{
    /// <summary>Creates an undoable set.</summary>
    /// <param name="initial">What it starts with; empty when omitted.</param>
    /// <param name="comparer">How items are compared; the default comparer when omitted.</param>
    public TrackedAtomsSet(IReadOnlyList<T>? initial = null, IEqualityComparer<T>? comparer = null)
        : base(initial, comparer) { }

    /// <inheritdoc />
    protected override bool RecordsHistory => true;
}
