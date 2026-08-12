using System.Collections.Generic;
using Arlecchino.Atoms.Tracked;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.Local;

/// <summary>
/// A list the undo stack never sees, for contents the user did not author: a log, search results, the rows
/// of a scan. It notifies and asks for a frame as a <see cref="TrackedAtomsList{T}"/> does.
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
