using Arlecchino.Rendering.Text;

namespace Arlecchino.Widgets.Readouts;

/// <summary>
/// One set of characters a graph is drawn with, and how many samples and levels a cell of it holds. A chart
/// asks for a character rather than working out which set it has.
/// </summary>
internal abstract class GraphGlyphs
{
    /// <summary>How many samples are drawn side by side in one cell.</summary>
    public abstract int PerCell { get; }

    /// <summary>How many levels of height a cell can show, above empty.</summary>
    public abstract int Levels { get; }

    /// <summary>Picks the character for a pair of levels.</summary>
    /// <param name="left">Level of the left sample, from nought to <see cref="Levels"/>.</param>
    /// <param name="right">
    /// Level of the right sample. Meaningless where a cell holds one sample, and ignored there.
    /// </param>
    /// <param name="inverted">Whether the chart hangs from the top rather than standing on the bottom.</param>
    /// <returns>The character to write.</returns>
    public abstract char Of(int left, int right, bool inverted);

    /// <summary>Picks the set that was asked for.</summary>
    /// <param name="symbols">Which set the chart and the application settled on.</param>
    /// <returns>The one that draws it.</returns>
    public static GraphGlyphs Chosen(GraphSymbols symbols) => symbols switch
    {
        GraphSymbols.Braille => BrailleGlyphs.Instance,
        GraphSymbols.Blocks => BlockGlyphs.Instance,
        _ => ShadeGlyphs.Instance,
    };
}

/// <summary>
/// Braille dots: four levels and two samples to a cell. The characters are computed from bits rather
/// than tabulated, since the block is laid out so that each dot is a bit.
/// </summary>
internal sealed class BrailleGlyphs : GraphGlyphs
{
    private const int BrailleBase = 0x2800;

    public static readonly BrailleGlyphs Instance = new();

    /// <inheritdoc/>
    public override int PerCell => 2;

    /// <inheritdoc/>
    public override int Levels => 4;

    /// <inheritdoc/>
    public override char Of(int left, int right, bool inverted)
    {
        var bits = 0;

        for (var dot = 0; dot < 4; dot++)
        {
            var glyph = inverted ? dot : 3 - dot;

            if (dot < left)
            {
                bits |= glyph switch { 0 => 0x01, 1 => 0x02, 2 => 0x04, _ => 0x40 };
            }

            if (dot < right)
            {
                bits |= glyph switch { 0 => 0x08, 1 => 0x10, 2 => 0x20, _ => 0x80 };
            }
        }

        return (char)(BrailleBase + bits);
    }
}

/// <summary>Quadrant blocks: two levels and two samples to a cell.</summary>
internal sealed class BlockGlyphs : GraphGlyphs
{
    private static readonly char[] Up = [' ', '▗', '▐', '▖', '▄', '▟', '▌', '▙', '█'];
    private static readonly char[] Down = [' ', '▝', '▐', '▘', '▀', '▜', '▌', '▛', '█'];

    public static readonly BlockGlyphs Instance = new();

    /// <inheritdoc/>
    public override int PerCell => 2;

    /// <inheritdoc/>
    public override int Levels => 2;

    /// <inheritdoc/>
    public override char Of(int left, int right, bool inverted) => (inverted ? Down : Up)[(left * 3) + right];
}

/// <summary>Shaded blocks: three levels and one sample to a cell, so the right level is not looked at.</summary>
internal sealed class ShadeGlyphs : GraphGlyphs
{
    private static readonly char[] Shades = [' ', '░', '▒', '█'];

    public static readonly ShadeGlyphs Instance = new();

    /// <inheritdoc/>
    public override int PerCell => 1;

    /// <inheritdoc/>
    public override int Levels => 3;

    /// <inheritdoc/>
    public override char Of(int left, int right, bool inverted) => Shades[left];
}
