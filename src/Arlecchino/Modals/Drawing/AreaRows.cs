using System;
using System.Collections.Generic;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Widgets.Text;

namespace Arlecchino.Modals.Drawing;

/// <summary>
/// One row of a field of many lines, cut into the pieces it is drawn in: what is selected, what is not,
/// and the caret. A row longer than the box is scrolled rather than cut off.
/// </summary>
internal static class AreaRows
{
    /// <summary>Builds the pieces of one row.</summary>
    /// <param name="line">What the row says.</param>
    /// <param name="shift">Columns scrolled off the left of it.</param>
    /// <param name="width">Columns the row is drawn in.</param>
    /// <param name="selection">Where the selection starts and ends inside this row.</param>
    /// <param name="caret">Where the caret is inside this row, or <c>-1</c> when it is on another row.</param>
    /// <returns>The pieces, together filling the width.</returns>
    public static Piece[] Of(string line, int shift, int width, (int Start, int End) selection, int caret)
    {
        var head = IndexAtWidth(line, shift);
        var slice = TextWidth.Truncate(line[head..], width - 1);
        var coat = caret < 0 ? Theme.Default : Theme.Input;
        var at = caret < 0 ? -1 : Math.Clamp(caret - head, 0, slice.Length);

        List<Piece> pieces = [];
        EntryLook look = new(coat, Theme.Selection, Theme.Caret);

        EntryRuns.Of(
            slice,
            at,
            (Math.Clamp(selection.Start - head, 0, slice.Length), Math.Clamp(selection.End - head, 0, slice.Length)),
            look,
            (piece, style) => pieces.Add(new(piece, style)));

        var rows = TextWidth.Of(slice) + (at == slice.Length ? 1 : 0);
        var padding = new string(' ', Math.Max(0, width - rows));

        pieces.Add(new(padding, coat));

        return [.. pieces];
    }

    private static int IndexAtWidth(string text, int columns)
    {
        var column = 0;
        var index = 0;

        while (index < text.Length && column < columns)
        {
            var length = TextWidth.NextClusterLength(text, index);

            column += TextWidth.OfCluster(text.AsSpan(index, length));
            index += length;
        }

        return index;
    }
}
