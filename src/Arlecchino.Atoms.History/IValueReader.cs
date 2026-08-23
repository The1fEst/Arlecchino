namespace Arlecchino.Atoms.History;

/// <summary>
/// Where an atom takes back the two values a step stood between. The atom calls this, since it is the one
/// place the kind of value is known without reflection.
/// </summary>
public interface IValueReader
{
    /// <summary>Reads both values of one step.</summary>
    /// <typeparam name="T">What the atom holds, supplied by the atom.</typeparam>
    /// <param name="undone">The value undoing puts back.</param>
    /// <param name="redone">The value redoing puts back.</param>
    /// <returns><c>false</c> when this kind of value has no reader, or the step does not parse.</returns>
    bool Read<T>(out T undone, out T redone);
}
