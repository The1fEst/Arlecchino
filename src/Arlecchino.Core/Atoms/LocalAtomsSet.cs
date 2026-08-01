using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// A set the undo history never sees: the rows expanded, the folders already walked, the hosts that
/// answered. It notifies and asks for a frame exactly as a <see cref="TrackedAtomsSet{T}"/> does.
/// </summary>
/// <typeparam name="T">What the set holds.</typeparam>
public sealed class LocalAtomsSet<T> : AtomsSet<T>
{
    /// <summary>Creates a set outside the undo history.</summary>
    /// <param name="initial">What it starts with; empty when omitted.</param>
    /// <param name="comparer">How items are compared; the default comparer when omitted.</param>
    public LocalAtomsSet(IReadOnlyList<T>? initial = null, IEqualityComparer<T>? comparer = null)
        : base(initial, comparer) { }

    /// <inheritdoc />
    protected override bool RecordsHistory => false;
}
