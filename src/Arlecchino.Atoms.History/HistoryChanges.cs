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
    private static readonly ConcurrentDictionary<string, IRestorableAtom> Atoms = new(StringComparer.Ordinal);

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
    /// name is the one that answers to it.
    /// </summary>
    /// <param name="key">What the atom is called.</param>
    /// <param name="atom">The atom itself.</param>
    public static void Name(string key, IRestorableAtom atom) => Atoms[key] = atom;

    /// <summary>Finds the atom a name stands for.</summary>
    /// <param name="key">What the atom is called.</param>
    /// <param name="atom">The atom, when one answered to the name.</param>
    /// <returns><c>true</c> when a named atom was found.</returns>
    public static bool TryFind(string key, [NotNullWhen(true)] out IRestorableAtom? atom) =>
        Atoms.TryGetValue(key, out atom);
}
