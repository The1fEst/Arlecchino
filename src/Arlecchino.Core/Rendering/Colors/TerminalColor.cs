namespace Arlecchino.Rendering.Colors;

/// <summary>
/// The sixteen ANSI colors plus the terminal's own default. Exact shades belong to the terminal theme, so
/// chrome picks a role from <see cref="Theme"/> rather than a color here.
/// </summary>
public enum TerminalColor : byte
{
    /// <summary>Whatever the terminal uses when no color is set.</summary>
    Default,

    /// <summary>Black.</summary>
    Black,

    /// <summary>Red.</summary>
    Red,

    /// <summary>Green.</summary>
    Green,

    /// <summary>Yellow.</summary>
    Yellow,

    /// <summary>Blue.</summary>
    Blue,

    /// <summary>Magenta.</summary>
    Magenta,

    /// <summary>Cyan.</summary>
    Cyan,

    /// <summary>White.</summary>
    White,

    /// <summary>Bright black, usually rendered as gray.</summary>
    BrightBlack,

    /// <summary>Bright red.</summary>
    BrightRed,

    /// <summary>Bright green.</summary>
    BrightGreen,

    /// <summary>Bright yellow.</summary>
    BrightYellow,

    /// <summary>Bright blue.</summary>
    BrightBlue,

    /// <summary>Bright magenta.</summary>
    BrightMagenta,

    /// <summary>Bright cyan.</summary>
    BrightCyan,

    /// <summary>Bright white.</summary>
    BrightWhite,
}
