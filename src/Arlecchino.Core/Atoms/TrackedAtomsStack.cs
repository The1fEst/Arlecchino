using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// A stack whose changes go on the undo stack of their own: the steps the user piled up, a draft
/// being unwound. <see cref="AtomHistory"/> picks it up with nothing to register, and each call is
/// one step.
/// </summary>
/// <typeparam name="T">What the stack holds.</typeparam>
public sealed class TrackedAtomsStack<T> : AtomsStack<T>
{
    /// <summary>Creates an undoable stack.</summary>
    /// <param name="initial">What is already on it, top first; empty when omitted.</param>
    public TrackedAtomsStack(IReadOnlyList<T>? initial = null)
        : base(initial) { }

    /// <inheritdoc />
    protected override bool RecordsHistory => true;
}
