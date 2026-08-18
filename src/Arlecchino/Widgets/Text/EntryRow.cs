using System;
using Arlecchino.Editing;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Widgets.Text;

/// <summary>How a line being typed into is written: the text itself, the part of it that is selected, and
/// the one symbol the caret stands on, which is written the other way round.</summary>
/// <param name="Text">What the line is written in.</param>
/// <param name="Selection">What the selected part of it is written in.</param>
/// <param name="Caret">What the symbol under the caret is written in.</param>
public readonly record struct EntryLook(
    IArlecchinoColor Text,
    IArlecchinoColor Selection,
    IArlecchinoColor Caret);

/// <summary>
/// A line being typed into, drawn on one row of the screen: a filter, a search, a field an application draws
/// for itself. Text longer than the room it is given scrolls, so the caret is always on the screen.
/// </summary>
public static class EntryRow
{
    private const string ScrollMarker = "…";

    /// <summary>Draws the line, scrolled to keep the caret in view.</summary>
    /// <param name="region">The region to draw on.</param>
    /// <param name="row">Which row of it.</param>
    /// <param name="column">Which column the text starts at.</param>
    /// <param name="width">How many columns it is drawn in, the caret included.</param>
    /// <param name="entry">The line being edited.</param>
    /// <param name="look">The colors to write it in.</param>
    /// <returns>How many columns were written.</returns>
    public static int Draw(
        SurfaceRegion region,
        int row,
        int column,
        int width,
        ITextEntry entry,
        EntryLook look) =>
        Draw(
            region,
            row,
            column,
            width,
            entry.Text,
            TextWidth.SnapToCluster(entry.Text, entry.Caret),
            TextEditing.Selection(entry),
            look);

    /// <summary>
    /// Draws a line that is written as something other than itself, which is what a secret comes to: the
    /// dots, the caret and the selection are all counted in what is shown rather than in what was typed.
    /// </summary>
    /// <param name="region">The region to draw on.</param>
    /// <param name="row">Which row of it.</param>
    /// <param name="column">Which column the text starts at.</param>
    /// <param name="width">How many columns it is drawn in, the caret included.</param>
    /// <param name="text">What to write.</param>
    /// <param name="caret">Where the caret is in it.</param>
    /// <param name="selection">Where the selection starts and ends in it.</param>
    /// <param name="look">The colors to write it in.</param>
    /// <returns>How many columns were written.</returns>
    public static int Draw(
        SurfaceRegion region,
        int row,
        int column,
        int width,
        string text,
        int caret,
        (int Start, int End) selection,
        EntryLook look)
    {
        if (width <= 0)
        {
            return 0;
        }

        var stands = Math.Clamp(caret, 0, text.Length);
        var (before, after) = LineWindow.Around(text, stands, width);
        var shownText = before + after;
        var head = stands - before.Length;
        var start = column + Written(region, row, column, head > 0 ? ScrollMarker : "", look.Text);
        var run = 0;

        EntryRuns.Of(
            shownText,
            caret < 0 ? -1 : before.Length,
            (Math.Clamp(selection.Start - head, 0, shownText.Length),
                Math.Clamp(selection.End - head, 0, shownText.Length)),
            look,
            (piece, style) => run += Written(region, row, start + run, piece, style));

        var tail = start + run;

        return tail -
               column +
               Written(region, row, tail, stands + after.Length < text.Length ? ScrollMarker : "", look.Text);
    }

    private static int Written(SurfaceRegion region, int row, int column, string text, IArlecchinoColor style)
    {
        if (text.Length == 0)
        {
            return 0;
        }

        region.Write(row, column, text, style);

        return TextWidth.Of(text);
    }
}
