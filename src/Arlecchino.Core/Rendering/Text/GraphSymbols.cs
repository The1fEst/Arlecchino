using System;
using Arlecchino.Atoms;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Terminals;

namespace Arlecchino.Rendering.Text;

/// <summary>
/// Which characters a graph is drawn with. The choice is about the font the terminal was given
/// rather than about taste: the denser the symbols, the more of them a font has to carry.
/// </summary>
public enum GraphSymbols
{
    /// <summary>
    /// Braille dots, four levels and two samples to a cell, which is the densest of the sets. It needs a font
    /// carrying the Braille Patterns block.
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
/// The symbols in use, reachable from anywhere that draws, the way <see cref="Theme"/> is. It is written on
/// the drawing thread and asks for a frame itself, so every graph follows on the next one.
/// </summary>
public static class Glyphs
{
    private static readonly string ChangingGraph = FrameMembers.Of(typeof(Glyphs), nameof(Graph));
    private static readonly string ChangingPicture = FrameMembers.Of(typeof(Glyphs), nameof(Picture));
    private static readonly string ChangingCellWidth = FrameMembers.Of(typeof(Glyphs), nameof(CellWidth));
    private static readonly string ChangingCellHeight = FrameMembers.Of(typeof(Glyphs), nameof(CellHeight));

    /// <summary>What graphs are drawn with when a widget does not say otherwise.</summary>
    /// <exception cref="InvalidOperationException">Assigned from off the drawing thread.</exception>
    public static GraphSymbols Graph
    {
        get;

        set
        {
            FrameThread.Verify(ChangingGraph);
            field = value;
            AtomChanges.NotifyWritten();
        }
    } = GraphSymbols.Braille;

    /// <summary>
    /// How pictures reach the terminal when a widget does not say otherwise, which is
    /// <see cref="ImageProtocol.Auto"/> by default. A terminal that cannot speak a named protocol shows the
    /// escape sequence as text.
    /// </summary>
    /// <exception cref="InvalidOperationException">Assigned from off the drawing thread.</exception>
    public static ImageProtocol Picture
    {
        get;

        set
        {
            FrameThread.Verify(ChangingPicture);
            field = value;
            AtomChanges.NotifyWritten();
        }
    } = ImageProtocol.Auto;

    /// <summary>
    /// How many pixels wide a cell is taken to be, which only <see cref="ImageProtocol.Sixel"/> needs.
    /// <see cref="TerminalProbe.Ask"/> sets it, and ten by twenty is the guess for a silent terminal.
    /// </summary>
    /// <exception cref="InvalidOperationException">Assigned from off the drawing thread.</exception>
    public static int CellWidth
    {
        get;

        set
        {
            FrameThread.Verify(ChangingCellWidth);
            field = value;
            AtomChanges.NotifyWritten();
        }
    } = 10;

    /// <summary>How many pixels tall a cell is taken to be. See <see cref="CellWidth"/>.</summary>
    /// <exception cref="InvalidOperationException">Assigned from off the drawing thread.</exception>
    public static int CellHeight
    {
        get;

        set
        {
            FrameThread.Verify(ChangingCellHeight);
            field = value;
            AtomChanges.NotifyWritten();
        }
    } = 20;
}
