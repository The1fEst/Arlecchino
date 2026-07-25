using System.Collections.Generic;

namespace Arlecchino.State;

/// <summary>
/// An atom the undo stack never sees: a filter, a cursor, a load in progress, a selection — state the
/// user did not author and would not expect to travel back through. It notifies and repaints exactly
/// as a <see cref="TrackedState{T}"/> does.
/// </summary>
/// <typeparam name="T">Type of the value held.</typeparam>
public sealed class LocalState<T> : State<T>
{
    /// <summary>Creates an atom holding a starting value, outside the undo history.</summary>
    /// <param name="initial">The value to start with.</param>
    /// <param name="comparer">
    /// How to decide that a write changed nothing; the default comparer for <typeparamref name="T"/>
    /// is used when omitted.
    /// </param>
    public LocalState(T initial, IEqualityComparer<T>? comparer = null)
        : base(initial, comparer)
    {
    }

    /// <inheritdoc />
    protected override bool RecordsHistory => false;
}
