using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// A queue the undo stack never sees: files still to copy, requests waiting for an answer, work a
/// background task will pick up. It notifies and asks for a frame exactly as a
/// <see cref="TrackedAtomsQueue{T}"/> does.
/// </summary>
/// <typeparam name="T">What waits in the queue.</typeparam>
public sealed class LocalAtomsQueue<T> : AtomsQueue<T>
{
    /// <summary>Creates a queue outside the undo history.</summary>
    /// <param name="initial">What is already waiting, front first; empty when omitted.</param>
    public LocalAtomsQueue(IReadOnlyList<T>? initial = null)
        : base(initial)
    {
    }

    /// <inheritdoc />
    protected override bool RecordsHistory => false;
}
