using System;

namespace Arlecchino.State;

/// <summary>
/// Where atoms announce themselves. The repaint signal listens to <see cref="Written"/> and the undo
/// history to <see cref="Recorded"/>; applications rarely touch this directly.
/// </summary>
public static class StateChanges
{
    /// <summary>Raised after any atom changed, which is what marks the frame stale.</summary>
    public static event Action? Written;

    /// <summary>Raised with an undoable edit, once per change of an atom that records history.</summary>
    public static event Action<IStateEdit>? Recorded;

    /// <summary>
    /// Whether anybody is collecting undo steps. Atoms check this before building an edit, so a
    /// program without a history pays nothing.
    /// </summary>
    public static bool IsRecording => Recorded is not null;

    /// <summary>Announces that an atom changed.</summary>
    public static void NotifyWritten() => Written?.Invoke();

    /// <summary>Announces an undoable edit.</summary>
    /// <param name="edit">The change that was made.</param>
    public static void NotifyRecorded(IStateEdit edit) => Recorded?.Invoke(edit);
}
