namespace Arlecchino.Atoms.History;

/// <summary>
/// Where an edit hands over the two values it stands between. The edit calls this, since it is the one
/// place the kind of value is known without reflection.
/// </summary>
public interface IValueWriter
{
    /// <summary>Takes both values of one edit.</summary>
    /// <typeparam name="T">What the atom holds, supplied by the edit rather than looked up.</typeparam>
    /// <param name="undone">The value undoing puts back.</param>
    /// <param name="redone">The value redoing puts back.</param>
    /// <returns><c>false</c> when this kind of value cannot be written, which abandons the whole step.</returns>
    bool Write<T>(T undone, T redone);
}
