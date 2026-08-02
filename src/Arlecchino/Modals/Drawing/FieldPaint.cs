using System;
using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Widgets.Lists;

using Arlecchino.Modals.Asking;

namespace Arlecchino.Modals.Drawing;

/// <summary>
/// What a field being typed into looks like: the text, the caret, and a complaint under it when there
/// is one. A value longer than the box is scrolled rather than cut off, with an ellipsis at whichever
/// end has more of it — the caret has to stay on screen wherever in the text it is.
/// </summary>
internal sealed class FieldPaint
{
    private const int BoxChromeColumns = 8;
    private const int SmallestFieldColumns = 12;
    private const string ScrollMarker = "…";
    private const string Caret = "▏";

    private readonly Surface _surface;
    private readonly ArlecchinoStrings _strings;
    private readonly ModalBox _box;

    /// <summary>Draws the fields.</summary>
    /// <param name="surface">The cell grid frames are built in.</param>
    /// <param name="strings">The words the application says things in.</param>
    /// <param name="box">The box they are drawn in.</param>
    public FieldPaint(Surface surface, ArlecchinoStrings strings, ModalBox box)
    {
        _surface = surface;
        _strings = strings;
        _box = box;
    }

    /// <summary>Draws a field of one line.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="title">What it is called.</param>
    /// <param name="hints">What the keys do.</param>
    public void Entry(ITextEntryModal modal, string title, string hints)
    {
        var caretIndex = TextWidth.SnapToCluster(modal.Text, modal.Caret);
        var shown = modal.Masked ? Dots(modal.Text) : modal.Text;
        var caret = modal.Masked ? TextWidth.CountClusters(modal.Text[..caretIndex]) : caretIndex;

        var room = Math.Max(
            SmallestFieldColumns,
            _surface.FrameWidth - BoxChromeColumns - TextWidth.Of(modal.Prefix) - TextWidth.Of(modal.Suffix));
        var (before, after) = Window(shown, caret, room);

        List<Piece[]> body =
        [
            [
                new(modal.Prefix, Theme.Muted),
                new(before.Length < caret ? ScrollMarker : "", Theme.Muted),
                new(before, Theme.Input),
                new(Caret, Theme.Accent),
                new(after, Theme.Input),
                new(caret + after.Length < shown.Length ? ScrollMarker : "", Theme.Muted),
                new(modal.Suffix, Theme.Muted),
            ],
        ];

        if (!string.IsNullOrEmpty(modal.Message))
        {
            body.Add([new(modal.Message, Theme.Error)]);
        }

        _box.Draw(title, body, hints);
    }

    /// <summary>Draws a field of many lines, scrolled to keep the row the caret is on in view.</summary>
    /// <param name="modal">The dialog.</param>
    public void Area(TextAreaModal modal)
    {
        var width = Math.Max(SmallestFieldColumns, _surface.FrameWidth / 2);
        var rows = Math.Clamp(modal.VisibleRows, 1, Math.Max(1, _surface.FrameHeight - 6));
        var window = ScrollWindow.Around(modal.Row, modal.Lines.Count, rows);

        modal.FirstVisible = window.First;

        var body = new List<Piece[]>();

        for (var offset = 0; offset < window.Count; offset++)
        {
            var index = window.First + offset;
            var line = modal.Lines[index];

            body.Add(index == modal.Row
                ? [new(CaretLine(line, modal.Column, width), Theme.Input)]
                : [new(TextWidth.PadRight(TextWidth.Truncate(line, width), width), Theme.Default)]);
        }

        if (modal.Message.Length > 0)
        {
            body.Add([new(modal.Message, Theme.Error)]);
        }

        var (box, inside) = _box.Draw(modal.Title, body, _strings.ModalTextAreaHints());

        modal.Box = box;
        modal.Rows = inside.Rows(0, window.Count);
    }

    /// <summary>
    /// The slice of a value that fits beside the caret. A value longer than the terminal is scrolled
    /// rather than cut off, so the caret stays on screen wherever it is in the text.
    /// </summary>
    /// <param name="text">The whole value.</param>
    /// <param name="caret">Where the caret is in it.</param>
    /// <param name="room">How many cells there are.</param>
    /// <returns>What goes before the caret and what goes after.</returns>
    private static (string Before, string After) Window(string text, int caret, int room)
    {
        var before = text[..caret];
        var after = text[caret..];

        if (TextWidth.Of(text) < room)
        {
            return (before, after);
        }

        var forCaretAndMarkers = 3;
        var visible = Math.Max(1, room - forCaretAndMarkers);
        var trailing = TextWidth.Truncate(after, visible / 2);

        return (TextWidth.TruncateStart(before, visible - TextWidth.Of(trailing)), trailing);
    }

    private static string Dots(string text) => new('•', TextWidth.CountClusters(text));

    private static string CaretLine(string line, int column, int width)
    {
        var caret = TextWidth.Of(line[..Math.Clamp(column, 0, line.Length)]);
        var shift = Math.Max(0, caret - width + 2);
        var visible = TextWidth.Truncate(SkipColumnsOf(line, shift), width - 1);
        var before = IndexAtWidth(visible, Math.Max(0, caret - shift));

        return TextWidth.PadRight($"{visible[..before]}{Caret}{visible[before..]}", width);
    }

    private static string SkipColumnsOf(string text, int columns)
    {
        var skipped = 0;
        var index = 0;

        while (index < text.Length && skipped < columns)
        {
            var length = TextWidth.NextClusterLength(text, index);

            skipped += TextWidth.OfCluster(text.AsSpan(index, length));
            index += length;
        }

        return text[index..];
    }

    private static int IndexAtWidth(string text, int columns)
    {
        var walked = 0;
        var index = 0;

        while (index < text.Length && walked < columns)
        {
            var length = TextWidth.NextClusterLength(text, index);

            walked += TextWidth.OfCluster(text.AsSpan(index, length));
            index += length;
        }

        return index;
    }
}
