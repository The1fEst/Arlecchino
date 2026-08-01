using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// A queue whose changes go on the undo stack: the steps of a plan the user arranged, the batch they
/// lined up. <see cref="AtomHistory"/> picks it up with nothing to register, and each call is one
/// step — including one that puts several in at once.
/// </summary>
/// <typeparam name="T">What waits in the queue.</typeparam>
public sealed class TrackedAtomsQueue<T> : AtomsQueue<T>
{
    /// <summary>Creates an undoable queue.</summary>
    /// <param name="initial">What is already waiting, front first; empty when omitted.</param>
    public TrackedAtomsQueue(IReadOnlyList<T>? initial = null)
        : base(initial) { }

    /// <inheritdoc />
    protected override bool RecordsHistory => true;
}
