using System.Collections.Generic;

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

    /// <summary>Reads what a collection held either side of one step.</summary>
    /// <typeparam name="T">What the collection holds, supplied by the collection.</typeparam>
    /// <param name="undone">The contents undoing lands on.</param>
    /// <param name="redone">The contents redoing lands on.</param>
    /// <returns><c>false</c> when this kind of item has no reader, or the step does not parse.</returns>
    bool ReadItems<T>(out T[] undone, out T[] redone);

    /// <summary>Reads what a map held either side of one step.</summary>
    /// <typeparam name="TKey">What the entries are looked up by, supplied by the map.</typeparam>
    /// <typeparam name="TValue">What is kept against each key, supplied by the map.</typeparam>
    /// <param name="undone">The entries undoing lands on.</param>
    /// <param name="redone">The entries redoing lands on.</param>
    /// <returns><c>false</c> when either half of an entry has no reader, or the step does not parse.</returns>
    bool ReadEntries<TKey, TValue>(
        out KeyValuePair<TKey, TValue>[] undone,
        out KeyValuePair<TKey, TValue>[] redone);
}
