using Arlecchino.Rendering.Text;

namespace Arlecchino.Rendering.Terminals;

/// <summary>
/// How a picture reaches the terminal. Like <see cref="GraphSymbols"/>, this is a question of what the
/// terminal can do rather than of taste.
/// </summary>
public enum ImageProtocol
{
    /// <summary>
    /// Cells, two pixels to each: the color of the upper half block and the background behind it. It needs
    /// nothing of the terminal beyond color and goes through the ordinary frame diff.
    /// </summary>
    Blocks,

    /// <summary>
    /// The kitty graphics protocol: the pixels themselves, spoken by kitty, WezTerm and Ghostty. A terminal
    /// that does not speak it shows the escape sequence as text.
    /// </summary>
    Kitty,

    /// <summary>
    /// Sixel: the older protocol, spoken by Windows Terminal, xterm, foot and WezTerm. Color comes down to a
    /// cube of 216, and <see cref="Glyphs.CellWidth"/> says how large a cell is taken to be.
    /// </summary>
    Sixel,

    /// <summary>
    /// The best of what the terminal admitted to when <see cref="TerminalProbe.Ask"/> asked, preferring kitty
    /// over sixel, and <see cref="Blocks"/> when it admitted to nothing. It is the default.
    /// </summary>
    Auto,
}
