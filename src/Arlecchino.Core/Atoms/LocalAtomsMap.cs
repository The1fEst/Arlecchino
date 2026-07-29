using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// A map the undo stack never sees: what a scan found per folder, the sizes worked out so far, the
/// state of each connection. It notifies and asks for a frame exactly as a
/// <see cref="TrackedAtomsMap{TKey, TValue}"/> does.
/// </summary>
/// <typeparam name="TKey">What the entries are looked up by.</typeparam>
/// <typeparam name="TValue">What is kept against each key.</typeparam>
public sealed class LocalAtomsMap<TKey, TValue> : AtomsMap<TKey, TValue>
    where TKey : notnull
{
    /// <summary>Creates a map outside the undo history.</summary>
    /// <param name="initial">What it starts with; empty when omitted.</param>
    /// <param name="comparer">How keys are compared; the default comparer when omitted.</param>
    public LocalAtomsMap(
        IReadOnlyDictionary<TKey, TValue>? initial = null,
        IEqualityComparer<TKey>? comparer = null)
        : base(initial, comparer)
    {
    }

    /// <inheritdoc />
    protected override bool RecordsHistory => false;
}
