using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// A list the undo stack never sees: a log, search results, the rows a background scan found, the
/// notifications on screen — contents the user did not author and would not expect to travel back
/// through. It notifies and asks for a frame exactly as a <see cref="TrackedAtomsList{T}"/> does.
/// </summary>
/// <typeparam name="T">What the list holds.</typeparam>
public sealed class LocalAtomsList<T> : AtomsList<T>
{
    /// <summary>Creates a list outside the undo history.</summary>
    /// <param name="initial">What it starts with; empty when omitted.</param>
    /// <param name="comparer">How items are compared; the default comparer when omitted.</param>
    public LocalAtomsList(IReadOnlyList<T>? initial = null, IEqualityComparer<T>? comparer = null)
        : base(initial, comparer) { }

    /// <inheritdoc />
    protected override bool RecordsHistory => false;
}
