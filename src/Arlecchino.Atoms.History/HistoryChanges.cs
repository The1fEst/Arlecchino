using System;

namespace Arlecchino.Atoms.History;

/// <summary>
/// Where the atoms that keep history announce their steps, and where <see cref="AtomHistory"/> hears
/// them. It carries the steps of the whole process, so one history collects them all.
/// </summary>
internal static class HistoryChanges
{
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
}
