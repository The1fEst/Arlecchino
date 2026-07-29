namespace Arlecchino.Rendering;

/// <summary>
/// How a picture reaches the terminal. Like <see cref="GraphSymbols"/>, this is a question of what the
/// terminal can do rather than of taste.
/// </summary>
public enum ImageProtocol
{
    /// <summary>
    /// Cells, two pixels to each: the colour of the upper half block and the background behind it.
    /// Coarse, but it needs nothing of the terminal beyond the colour it already draws in, works
    /// through the ordinary frame diff, and leaves nothing behind to clean up.
    /// </summary>
    Blocks,

    /// <summary>
    /// The kitty graphics protocol: the pixels themselves, sent as they are. Kitty, WezTerm and Ghostty
    /// speak it; a terminal that does not will show the escape sequence as text, which is why this is
    /// asked for rather than assumed.
    /// </summary>
    Kitty,

    /// <summary>
    /// Sixel: the older protocol, and the one Windows Terminal, xterm, foot and WezTerm speak. Colour
    /// comes down to a fixed cube of 216, which is what the format keeps registers for, and the picture
    /// is measured in pixels rather than cells — so <see cref="Glyphs.CellWidth"/> and
    /// <see cref="Glyphs.CellHeight"/> say how large a cell is taken to be.
    /// </summary>
    Sixel,
}
