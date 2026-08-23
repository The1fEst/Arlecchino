namespace Arlecchino.Atoms.History;

/// <summary>
/// An edit a <see cref="HistoryFile"/> can keep for longer than the run that made it. Edits of the
/// collection atoms are a position and a span rather than a value, so they do not implement it.
/// </summary>
public interface IWritableAtomEdit : IAtomEdit
{
    /// <summary>What the atom is called in the file.</summary>
    string Key { get; }

    /// <summary>Hands both values over, typed.</summary>
    /// <param name="writer">Where the values go.</param>
    /// <returns><c>false</c> when the writer could not take them.</returns>
    bool WriteTo(IValueWriter writer);
}
