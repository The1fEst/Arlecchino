using System.Collections.Generic;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.Tracked;

/// <summary>
/// A queue whose changes go on the undo stack, picked up by <see cref="AtomHistory"/> with nothing to
/// register. Each call is one step, including one that puts several in at once.
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
