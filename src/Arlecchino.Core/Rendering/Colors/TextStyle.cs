using System;

namespace Arlecchino.Rendering.Colors;

/// <summary>
/// Text attributes a style carries on top of its colors. Combine them with <c>|</c>; a terminal
/// that does not support one simply ignores it.
/// </summary>
[Flags]
public enum TextStyle : byte
{
    /// <summary>No attributes.</summary>
    None = 0,

    /// <summary>Bold, which some terminals render as a brighter color instead.</summary>
    Bold = 1 << 0,

    /// <summary>Italic, the least widely supported of the four.</summary>
    Italic = 1 << 1,

    /// <summary>Underlined.</summary>
    Underline = 1 << 2,

    /// <summary>Dim, the opposite of <see cref="Bold"/>.</summary>
    Dim = 1 << 3,
}
