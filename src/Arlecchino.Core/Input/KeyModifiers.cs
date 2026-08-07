using System;

namespace Arlecchino.Input;

/// <summary>
/// Modifiers held with a key. The three the console knows about keep the values
/// <see cref="ConsoleModifiers"/> gives them, so the two agree bit by bit; <see cref="Super"/> is the
/// one the console has no room for — Command on a Mac, the Windows key elsewhere.
/// </summary>
[Flags]
public enum KeyModifiers
{
    /// <summary>Nothing held.</summary>
    None = 0,

    /// <summary>Alt, or Option on a Mac.</summary>
    Alt = 1,

    /// <summary>Shift.</summary>
    Shift = 2,

    /// <summary>Control.</summary>
    Control = 4,

    /// <summary>
    /// Command on a Mac, the Windows key elsewhere. Terminals report it in the same modifier field as
    /// the rest, one bit further up; the Windows console never reports it at all, because the key is
    /// taken by the system before an application sees it.
    /// </summary>
    Super = 8,
}
