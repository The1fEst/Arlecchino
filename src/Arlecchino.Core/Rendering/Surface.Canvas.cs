using System;

namespace Arlecchino.Rendering;

public partial class Surface
{
    /// <summary>
    /// Restyles a whole row and writes text at the horizontal padding, ignoring the flow cursor.
    /// </summary>
    /// <param name="row">Row in frame coordinates.</param>
    /// <param name="line">Text to write.</param>
    /// <param name="style">Style for the row; the default role when omitted.</param>
    public void WriteLineAt(int row, string line, ITermColor? style = null)
    {
        if (row < 0 || row >= _height)
        {
            return;
        }

        style ??= Theme.Default;

        var cells = _cells[row];
        var styles = _styles[row];

        for (var col = 0; col < _width; col++)
        {
            cells[col] = BlankCell;
            styles[col] = style;
        }

        WriteAt(row, HorizontalPadding, TextWidth.Truncate(line, Math.Max(0, _width - HorizontalPadding * 2)), style);
    }

    /// <summary>Draws a rule across the content width on a given row.</summary>
    /// <param name="row">Row in frame coordinates.</param>
    /// <param name="style">Style for the rule; the default role when omitted.</param>
    public void FillLineAt(int row, ITermColor? style = null)
    {
        WriteLineAt(row, new('-', Math.Max(0, _width - HorizontalPadding * 2)), style);
    }

    /// <summary>
    /// Writes at an exact cell, clipped to the frame. A wide symbol takes two cells; writing over
    /// either half clears the other, and one that would be split by the right edge is dropped.
    /// </summary>
    /// <param name="row">Row in frame coordinates.</param>
    /// <param name="column">Column in frame coordinates.</param>
    /// <param name="text">Text to write.</param>
    /// <param name="style">Style for the text.</param>
    public void WriteAt(int row, int column, string text, ITermColor style)
    {
        if (row < 0 || row >= _height)
        {
            return;
        }

        var index = 0;
        var target = column;

        while (index < text.Length && target < _width)
        {
            var length = TextWidth.NextClusterLength(text, index);
            var cluster = text.AsSpan(index, length);
            var clusterWidth = TextWidth.OfCluster(cluster);

            index += length;

            if (clusterWidth == 0)
            {
                continue;
            }

            if (target < 0)
            {
                target += clusterWidth;
                continue;
            }

            if (clusterWidth == 2 && target + 1 >= _width)
            {
                SetCell(row, target, BlankCell, 1, style);
                return;
            }

            SetCell(row, target, CellOf(cluster), clusterWidth, style);
            target += clusterWidth;
        }
    }
}
