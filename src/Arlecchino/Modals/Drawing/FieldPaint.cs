using System;
using System.Collections.Generic;
using Arlecchino.Editing;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Widgets.Lists;
using Arlecchino.Widgets.Text;
using Arlecchino.Modals.Asking;

namespace Arlecchino.Modals.Drawing;

/// <summary>
/// What a field being typed into looks like: the text, the caret, and a complaint under it. A value longer
/// than the box scrolls, so the caret stays on screen.
/// </summary>
internal sealed class FieldPaint
{
    private const int BoxChromeColumns = 8;
    private const int SmallestFieldColumns = 12;
    private const string ScrollMarker = "…";

    private readonly Surface _surface;
    private readonly ArlecchinoStrings _strings;
    private readonly ModalBox _box;

    /// <summary>
    /// The colors a line being typed into is written in, read afresh every frame so that swapping the
    /// palette restyles the fields along with everything else.
    /// </summary>
    private static EntryLook Look => new(Theme.Input, Theme.Selected, Theme.Caret);

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
        var (before, after) = LineWindow.Around(shown, caret, room);
        var (start, end) = Selection(modal);
        var window = before + after;
        var head = caret - before.Length;

        List<Piece> line =
        [
            new(modal.Prefix, Theme.Muted),
            new(head > 0 ? ScrollMarker : "", Theme.Muted),
        ];

        EntryRuns.Of(
            window,
            before.Length,
            (Math.Clamp(start - head, 0, window.Length), Math.Clamp(end - head, 0, window.Length)),
            Look,
            (piece, style) => line.Add(new(piece, style)));

        line.Add(new(caret + after.Length < shown.Length ? ScrollMarker : "", Theme.Muted));
        line.Add(new(modal.Suffix, Theme.Muted));

        List<Piece[]> body = [[.. line]];

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
        var (from, to) = TextEditing.Selection(modal);

        for (var offset = 0; offset < window.Count; offset++)
        {
            var index = window.First + offset;
            var line = modal.Lines[index];
            var start = modal.StartOf(index);
            var caret = index == modal.Row ? modal.Column : -1;

            body.Add(AreaRows.Of(
                line,
                caret < 0 ? 0 : Shift(line, caret, width),
                width,
                (Math.Clamp(from - start, 0, line.Length), Math.Clamp(to - start, 0, line.Length)),
                caret));
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
    /// What is selected, counted the way the field is drawn: a masked field shows one dot per symbol, so
    /// the places the selection is at are counted in symbols there rather than in characters.
    /// </summary>
    /// <param name="modal">The dialog.</param>
    /// <returns>Where the selection starts and where it ends.</returns>
    private static (int Start, int End) Selection(ITextEntryModal modal)
    {
        var (start, end) = TextEditing.Selection(modal);

        return modal.Masked
            ? (TextWidth.CountClusters(modal.Text[..start]), TextWidth.CountClusters(modal.Text[..end]))
            : (start, end);
    }

    private static string Dots(string text) => new('•', TextWidth.CountClusters(text));

    /// <summary>
    /// How far the row the caret is on has to be scrolled to keep the caret in the box. Only that row is
    /// scrolled: the surrounding rows are read rather than typed on.
    /// </summary>
    /// <param name="line">What the row says.</param>
    /// <param name="column">Where the caret is inside it.</param>
    /// <param name="width">Columns the row is drawn in.</param>
    /// <returns>Columns to scroll off the left.</returns>
    private static int Shift(string line, int column, int width) =>
        Math.Max(0, TextWidth.Of(line[..Math.Clamp(column, 0, line.Length)]) - width + 2);
}
