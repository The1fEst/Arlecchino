using System.Collections.Generic;
using Arlecchino.Atoms.Tracked;
using Arlecchino.Atoms.Collections;

namespace Arlecchino.Atoms.Local;

/// <summary>
/// A stack the undo history never sees: where the user has been, the folders walked into, the
/// screens over one another. It notifies and asks for a frame exactly as a
/// <see cref="TrackedAtomsStack{T}"/> does.
/// </summary>
/// <typeparam name="T">What the stack holds.</typeparam>
public sealed class LocalAtomsStack<T> : AtomsStack<T>
{
    /// <summary>Creates a stack outside the undo history.</summary>
    /// <param name="initial">What is already on it, top first; empty when omitted.</param>
    public LocalAtomsStack(IReadOnlyList<T>? initial = null)
        : base(initial) { }

    /// <inheritdoc />
    protected override bool RecordsHistory => false;
}
