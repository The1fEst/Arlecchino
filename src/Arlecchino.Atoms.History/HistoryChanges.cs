using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Arlecchino.Atoms.History;

/// <summary>
/// Where the atoms that keep history announce their steps and their names, and where
/// <see cref="AtomHistory"/> hears them. It carries the whole process, so one history collects it all.
/// </summary>
internal static class HistoryChanges
{
    private static readonly ConcurrentDictionary<string, (Type ValueType, IRestorableAtom Atom)> Atoms =
        new(StringComparer.Ordinal);

    /// <summary>Raised with a step, once per change of an atom that keeps history.</summary>
    public static event Action<IAtomEdit>? Recorded;

    /// <summary>
    /// Whether a history is listening. Atoms ask before describing a change, so a program that keeps no
    /// history pays nothing for the atoms that could have fed one.
    /// </summary>
    public static bool IsRecording => Recorded is not null;

    /// <summary>Announces a step.</summary>
    /// <param name="edit">The change that was made.</param>
    public static void NotifyRecorded(IAtomEdit edit) => Recorded?.Invoke(edit);

    /// <summary>
    /// Takes a named atom, so a step read from a file can find it again. The atom announced last under a
    /// name answers to it, which is what a screen's store rebuilt on the way back relies on.
    /// </summary>
    /// <param name="key">What the atom is called.</param>
    /// <param name="valueType">What the atom holds, which two atoms under one name have to agree on.</param>
    /// <param name="atom">The atom itself.</param>
    /// <exception cref="InvalidOperationException">The name is taken by an atom holding something else.</exception>
    public static void Name(string key, Type valueType, IRestorableAtom atom) =>
        Atoms.AddOrUpdate(
            key,
            (valueType, atom),
            (_, entry) => entry.ValueType == valueType
                ? (valueType, atom)
                : throw new InvalidOperationException(
                    $"'{key}' already names an atom holding {entry.ValueType.Name}, and this one holds " +
                    $"{valueType.Name}. A name stands for one piece of state, so give one of them another."));

    /// <summary>Finds the atom a name stands for.</summary>
    /// <param name="key">What the atom is called.</param>
    /// <param name="atom">The atom, when one answered to the name.</param>
    /// <returns><c>true</c> when a named atom was found.</returns>
    public static bool TryFind(string key, [NotNullWhen(true)] out IRestorableAtom? atom)
    {
        if (Atoms.TryGetValue(key, out var named))
        {
            atom = named.Atom;
            return true;
        }

        atom = null;
        return false;
    }
}
