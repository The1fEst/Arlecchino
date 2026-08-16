using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Modals.Drawing;
using Arlecchino.Modals.Reading;
using Arlecchino.State;

namespace Arlecchino.Modals;

/// <summary>One run of a line inside a dialog, with the color it is written in.</summary>
/// <param name="Text">The words.</param>
/// <param name="Style">How they are written.</param>
public readonly record struct Piece(string Text, IArlecchinoColor Style);

/// <summary>
/// Everything a dialog needs from the application: where to draw, the words to draw in, the keys to obey, and
/// how to close. A dialog is a value, so it is given these when asked to act.
/// </summary>
public sealed class ModalFrame
{
    private readonly Surface _surface;
    private readonly ArlecchinoState _state;
    private readonly IArlecchinoTerminal _terminal;
    private readonly ModalBox _box;

    /// <summary>Gathers what a dialog is allowed to reach.</summary>
    /// <param name="surface">The cell grid frames are built in.</param>
    /// <param name="state">Where the stack of dialogs lives.</param>
    /// <param name="terminal">Reached for the clipboard.</param>
    /// <param name="options">The keys and the words this application was started with.</param>
    internal ModalFrame(
        Surface surface,
        ArlecchinoState state,
        IArlecchinoTerminal terminal,
        ArlecchinoOptions options)
    {
        _surface = surface;
        _state = state;
        _terminal = terminal;
        _box = new(surface);

        Keymap = options.Keymap;
        Keys = KeyText.For(options.TextInput);
        Strings = options.Strings;

        Steps = new(state, Keymap);
        Fields = new(state, Keymap, Keys, terminal, Strings, Steps);
        Areas = new(state, Keymap, Keys, terminal);
        Choices = new(state, Keymap, Keys, terminal);
        Paint = new(surface, Strings, _box);
        Values = new(Strings, _box);
        Lists = new(surface, Strings, _box);
        Tells = new(surface, Strings, _box);
    }

    /// <summary>How the dialogs that step a value read their keys.</summary>
    internal StepKeys Steps { get; }

    /// <summary>How the dialogs with a field of one line read theirs.</summary>
    internal FieldKeys Fields { get; }

    /// <summary>How the field of many lines reads its own.</summary>
    internal TextAreaKeys Areas { get; }

    /// <summary>How the lists read theirs.</summary>
    internal ListKeys Choices { get; }

    /// <summary>How a field of text is drawn.</summary>
    internal FieldPaint Paint { get; }

    /// <summary>How a slider, a toggle, a color and a segmented field are drawn.</summary>
    internal ValuePaint Values { get; }

    /// <summary>How a list is drawn.</summary>
    internal ListPaint Lists { get; }

    /// <summary>How a message and an opened notification are drawn.</summary>
    internal TellPaint Tells { get; }

    /// <summary>The whole screen, for a dialog that would rather place itself.</summary>
    public SurfaceRegion Screen => _surface.Content;

    /// <summary>How many cells wide the frame is.</summary>
    public int Width => _surface.FrameWidth;

    /// <summary>How many rows tall.</summary>
    public int Height => _surface.FrameHeight;

    /// <summary>The words the application says things in.</summary>
    public ArlecchinoStrings Strings { get; }

    /// <summary>Keys to obey.</summary>
    public ArlecchinoKeymap Keymap { get; }

    /// <summary>Turns a key press into the character it stands for.</summary>
    public KeyText Keys { get; }

    /// <summary>
    /// How many dialogs are already open under this one. Each is offset a row down and three columns
    /// along, so a dialog opened from a dialog reads as being on top of it rather than instead of it.
    /// </summary>
    public int Depth
    {
        get => _box.Depth;
        internal set => _box.Depth = value;
    }

    /// <summary>Closes the dialog on top, which while it is being handled is this one.</summary>
    public void Close() => _state.CloseModal();

    /// <summary>Puts text on the clipboard, for the dialogs that offer copying.</summary>
    /// <param name="text">What to copy.</param>
    public void Copy(string text) => _terminal.CopyToClipboard(text);

    /// <summary>The rule that separates the body of a dialog from its hints.</summary>
    /// <param name="box">The whole box, border included.</param>
    /// <param name="insideRow">Which row inside the box it goes under.</param>
    public static void Divider(SurfaceRegion box, int insideRow) => ModalBox.Divider(box, insideRow);

    /// <summary>A box of the size asked for, in the middle of the screen and never off the edge of it.</summary>
    /// <param name="contentWidth">How wide what goes in it is.</param>
    /// <param name="contentHeight">How tall.</param>
    /// <returns>Where the box goes.</returns>
    public SurfaceRegion Centered(int contentWidth, int contentHeight) =>
        _box.Centered(contentWidth, contentHeight);

    /// <summary>
    /// Draws a titled box holding the lines given, with the hints under a rule. Every dialog the framework
    /// brings is drawn through this.
    /// </summary>
    /// <param name="title">What the dialog is called.</param>
    /// <param name="body">The lines, each a run of pieces.</param>
    /// <param name="footer">What the keys do, along the bottom.</param>
    /// <returns>The whole box, and the region inside it the lines were written in.</returns>
    public (SurfaceRegion Box, SurfaceRegion Content) Box(
        string title,
        IReadOnlyList<Piece[]> body,
        string footer) => _box.Draw(title, body, footer);
}
