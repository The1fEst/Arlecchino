using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.Tracked;

/// <summary>
/// A set whose changes go on the undo stack: what the user marked, the columns they turned on, the
/// tags they put on something. <see cref="AtomHistory"/> picks it up with nothing to register, and
/// each call is one step — including one that puts several in at once.
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
