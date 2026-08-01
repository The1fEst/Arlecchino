using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// A map whose changes go on the undo stack: what the user set per profile, the notes kept against
/// each entry, the overrides of a configuration. <see cref="AtomHistory"/> picks it up with nothing
/// to register, and each call is one step.
/// </summary>
/// <typeparam name="TKey">What the entries are looked up by.</typeparam>
/// <typeparam name="TValue">What is kept against each key.</typeparam>
public sealed class TrackedAtomsMap<TKey, TValue> : AtomsMap<TKey, TValue>
    where TKey : notnull
{
    /// <summary>Creates an undoable map.</summary>
    /// <param name="initial">What it starts with; empty when omitted.</param>
    /// <param name="comparer">How keys are compared; the default comparer when omitted.</param>
    public TrackedAtomsMap(
        IReadOnlyDictionary<TKey, TValue>? initial = null,
        IEqualityComparer<TKey>? comparer = null)
        : base(initial, comparer) { }

    /// <inheritdoc />
    protected override bool RecordsHistory => true;
}
