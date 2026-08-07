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
/// <see cref="Theme"/>, and for the same reason: a widget reads the look rather than being told
/// it. Assigned from <c>ArlecchinoOptions</c> when the container resolves them; set it directly when
/// drawing without a host.
///
/// It is process-wide and settable, so an application can offer the choice in its own settings and
/// have every graph follow on the next frame. A frame reads all of it, so all of it is written on the
/// drawing thread and asks for a frame by itself; hand the change over with
/// <see cref="FrameThread.Post"/> from anywhere else.
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
    /// How pictures reach the terminal when a widget does not say otherwise.
    /// <see cref="ImageProtocol.Auto"/> by default, which is the best of what the terminal admitted to
    /// when it was asked and cells when it admitted to nothing. Name a protocol to decide yourself — a
    /// terminal that cannot speak the one you name shows the escape sequence as text.
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
    /// How many pixels wide a cell is taken to be. Only <see cref="ImageProtocol.Sixel"/> needs it,
    /// because sixel is measured in pixels and knows nothing of cells: a picture is resampled to
    /// however many pixels the cells it was given come to.
    ///
    /// <see cref="TerminalProbe.Ask"/> sets it from what the terminal reports. Ten by twenty is the
    /// standing guess for a terminal that does not answer, and a wrong guess shows as a picture that
    /// does not quite fill its pane rather than as a broken picture.
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
