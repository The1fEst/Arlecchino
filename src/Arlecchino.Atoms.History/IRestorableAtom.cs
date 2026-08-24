namespace Arlecchino.Atoms.History;

/// <summary>
/// An atom a step can be handed back to, which is how a <see cref="HistoryFile"/> written by an earlier
/// run reaches the atoms of this one. It answers to the name it was given.
/// </summary>
public interface IRestorableAtom
{
    /// <summary>Builds the step the reader is holding, for this atom.</summary>
    /// <param name="reader">Where the two values come from.</param>
    /// <returns>The step, or <c>null</c> when the values could not be read.</returns>
    IAtomEdit? Restore(IValueReader reader);
}
