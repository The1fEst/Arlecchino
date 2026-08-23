using System;

namespace Arlecchino.Atoms;

/// <summary>
/// Where atoms announce that something changed, which is what marks the frame stale. Applications rarely
/// touch this directly.
/// </summary>
internal static class AtomChanges
{
    /// <summary>Raised after any atom changed.</summary>
    public static event Action? Written;

    /// <summary>Announces that an atom changed.</summary>
    public static void NotifyWritten() => Written?.Invoke();
}
