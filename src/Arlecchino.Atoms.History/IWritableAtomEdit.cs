namespace Arlecchino.Atoms.History;

/// <summary>
/// An edit a <see cref="HistoryFile"/> can keep for longer than the run that made it. An edit of an atom
/// that was never named cannot be written, and says so from <see cref="WriteTo"/>.
/// </summary>
public interface IWritableAtomEdit : IAtomEdit
{
    /// <summary>What the atom is called in the file. Read only once <see cref="WriteTo"/> has agreed.</summary>
    string Key { get; }

    /// <summary>Hands both values over, typed.</summary>
    /// <param name="writer">Where the values go.</param>
    /// <returns><c>false</c> when the writer could not take them.</returns>
    bool WriteTo(IValueWriter writer);
}
