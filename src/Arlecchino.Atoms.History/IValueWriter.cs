using System.Collections.Generic;

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

    /// <summary>
    /// Takes what a collection held either side of one edit, in full rather than as the difference it
    /// made. A later run holds something else, and only the contents say where the step lands.
    /// </summary>
    /// <typeparam name="T">What the collection holds, supplied by the edit rather than looked up.</typeparam>
    /// <param name="undone">The contents undoing lands on.</param>
    /// <param name="redone">The contents redoing lands on.</param>
    /// <returns><c>false</c> when this kind of item cannot be written, which abandons the whole step.</returns>
    bool WriteItems<T>(T[] undone, T[] redone);

    /// <summary>Takes what a map held either side of one edit, in full, for the same reason.</summary>
    /// <typeparam name="TKey">What the entries are looked up by, supplied by the edit.</typeparam>
    /// <typeparam name="TValue">What is kept against each key, supplied by the edit.</typeparam>
    /// <param name="undone">The entries undoing lands on.</param>
    /// <param name="redone">The entries redoing lands on.</param>
    /// <returns><c>false</c> when either half of an entry cannot be written, which abandons the whole step.</returns>
    bool WriteEntries<TKey, TValue>(KeyValuePair<TKey, TValue>[] undone, KeyValuePair<TKey, TValue>[] redone);
}
