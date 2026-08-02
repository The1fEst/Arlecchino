using Arlecchino.Rendering.Terminals;

namespace Arlecchino.Rendering.Colors;

/// <summary>
/// How much colour the terminal can show. Detected once at startup by
/// <see cref="TerminalCapabilities.DetectColor()"/> and used by every style when it builds its
/// escape sequence.
/// </summary>
public enum ColorSupport : byte
{
    /// <summary>
    /// No colour at all: styles emit nothing, not even the per-line reset. Chosen for
    /// <c>NO_COLOR</c>, <c>TERM=dumb</c>, or a Windows console that refused virtual terminal mode.
    /// </summary>
    None,

    /// <summary>The sixteen ANSI colours; a <see cref="Rgb"/> is mapped to the nearest of them.</summary>
    Palette,

    /// <summary>Full 24-bit colour, so <see cref="RgbTermColor"/> emits exact values.</summary>
    TrueColor,
}
