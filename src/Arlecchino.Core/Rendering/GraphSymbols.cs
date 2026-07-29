namespace Arlecchino.Rendering;

/// <summary>
/// Which characters a graph is drawn with. The choice is about the font the terminal was given
/// rather than about taste: the denser the symbols, the more of them a font has to carry.
/// </summary>
public enum GraphSymbols
{
    /// <summary>
    /// Braille dots, four levels and two samples to a cell — the densest, and what a graph looks
    /// best in. Needs a font carrying the Braille Patterns block, or a terminal that falls back to
    /// one that does; Windows Terminal does, the classic console host does not.
    /// </summary>
    Braille,

    /// <summary>
    /// Quadrant blocks, two levels and two samples to a cell — half the height of braille, and in
    /// nearly every monospace font there is.
    /// </summary>
    Blocks,

    /// <summary>
    /// Shaded blocks, three levels and one sample to a cell. The plainest of the three, for a
    /// console whose font carries little more than ASCII.
    /// </summary>
    Tty,
}

/// <summary>
/// The symbols in use, reachable from anywhere that draws — the same arrangement as
/// <see cref="Theme"/>, and for the same reason: a widget picks the look up rather than being told
/// it. Assigned from <c>ArlecchinoOptions</c> when the container resolves them; set it directly when
/// drawing without a host.
///
/// It is process-wide and settable, so an application can offer the choice in its own settings and
/// have every graph follow on the next frame. A change made outside the input path should ask for a
/// frame with <c>Repaint.Request()</c>, since nothing else will.
/// </summary>
public static class Glyphs
{
    /// <summary>What graphs are drawn with when a widget does not say otherwise.</summary>
    public static GraphSymbols Graph { get; set; } = GraphSymbols.Braille;

    /// <summary>
    /// How pictures reach the terminal when a widget does not say otherwise. Cells by default, since
    /// they work everywhere; a graphics protocol is asked for, because a terminal that cannot speak it
    /// shows the escape sequence as text rather than failing quietly.
    /// </summary>
    public static ImageProtocol Picture { get; set; } = ImageProtocol.Blocks;

    /// <summary>
    /// How many pixels wide a cell is taken to be. Only <see cref="ImageProtocol.Sixel"/> needs it,
    /// because sixel is measured in pixels and knows nothing of cells: a picture is resampled to
    /// however many pixels the cells it was given come to.
    ///
    /// There is no asking the terminal yet, so this is a guess an application can correct — ten by
    /// twenty is what a terminal at a common font size tends to be, and a wrong guess shows as a
    /// picture that does not quite fill its pane rather than as a broken one.
    /// </summary>
    public static int CellWidth { get; set; } = 10;

    /// <summary>How many pixels tall a cell is taken to be. See <see cref="CellWidth"/>.</summary>
    public static int CellHeight { get; set; } = 20;
}
