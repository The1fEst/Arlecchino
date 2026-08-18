using System;
using System.Collections.Generic;
using System.Text;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Rendering;

public partial class Surface
{
    /// <summary>
    /// Writes one line at the flow cursor and moves it down. Stops silently once the frame is full,
    /// so a view never has to bound its own output.
    /// </summary>
    /// <param name="line">Text to write.</param>
    /// <param name="style">Style for the line; the default role when omitted.</param>
    /// <param name="align">Horizontal alignment inside the content width.</param>
    /// <param name="margin">Extra space around the line.</param>
    public void AppendLine(string line, IArlecchinoColor? style = null, Align align = Align.Left, Margin margin = default)
    {
        style ??= Theme.Default;

        for (var i = 0; i < margin.Top; i++)
        {
            SkipLine();
        }

        if (FreeLines <= 0)
        {
            return;
        }

        var left = HorizontalPadding + margin.Left;
        var contentWidth = Math.Max(0, _width - left - HorizontalPadding - margin.Right);
        var clippedLine = TextWidth.Truncate(line, contentWidth);
        var offset = 0;

        if (align.HasFlag(Align.Center))
        {
            offset = Math.Max(0, (contentWidth - TextWidth.Of(clippedLine)) / 2);
        }
        else if (align.HasFlag(Align.Right))
        {
            offset = Math.Max(0, contentWidth - TextWidth.Of(clippedLine));
        }

        var row = _lines++;
        WriteLineAt(row, "", style);
        WriteAt(row, left + offset, clippedLine, style);

        for (var i = 0; i < margin.Bottom; i++)
        {
            SkipLine();
        }
    }

    /// <summary>Writes a row of padded columns at the flow cursor.</summary>
    /// <param name="strings">Cell texts, in column order.</param>
    /// <param name="widths">
    /// Column widths: a positive width right-aligns the cell, a negative one left-aligns it.
    /// </param>
    /// <param name="style">Style for the row.</param>
    /// <param name="prefix">Text placed before the first column, such as a marker.</param>
    public void WriteTableRow(string[] strings, int[] widths, IArlecchinoColor style, string prefix = "")
    {
        var line = new StringBuilder(prefix);
        for (var i = 0; i < strings.Length; i++)
        {
            var text = strings[i];
            var width = i < widths.Length ? widths[i] : 0;
            var pad = Math.Abs(width) - TextWidth.Of(text);

            if (pad > 0 && width > 0)
            {
                line.Append(' ', pad);
            }

            line.Append(text);

            if (pad > 0 && width < 0)
            {
                line.Append(' ', pad);
            }
        }

        AppendLine(line.ToString(), style);
    }

    /// <summary>
    /// Places a block of prepared lines as a unit, ignoring the flow cursor. Vertical alignment
    /// flags work here, which is how the hints box is anchored to a corner.
    /// </summary>
    /// <param name="lines">Lines of the block.</param>
    /// <param name="style">Style for the block.</param>
    /// <param name="align">Horizontal and vertical alignment against the frame.</param>
    /// <param name="margin">Space kept free from the edges it is aligned to.</param>
    public void WriteBlock(IReadOnlyList<string> lines,
        IArlecchinoColor style,
        Align align = Align.Left | Align.Top,
        Margin margin = default)
    {
        if (lines.Count == 0)
        {
            return;
        }

        var blockWidth = 0;
        foreach (var line in lines)
        {
            blockWidth = Math.Max(blockWidth, TextWidth.Of(line));
        }

        var column = margin.Left;
        if (align.HasFlag(Align.Center))
        {
            column = Math.Max(0, (_width - blockWidth) / 2);
        }
        else if (align.HasFlag(Align.Right))
        {
            column = Math.Max(0, _width - blockWidth - margin.Right);
        }

        var row = margin.Top;
        if (align.HasFlag(Align.Middle))
        {
            row = Math.Max(0, (_height - lines.Count) / 2);
        }
        else if (align.HasFlag(Align.Bottom))
        {
            row = Math.Max(0, _height - lines.Count - margin.Bottom);
        }

        for (var i = 0; i < lines.Count; i++)
        {
            WriteAt(row + i, column, lines[i], style);
        }
    }

    /// <summary>Draws a rule across the content width at the flow cursor.</summary>
    public void FillLine()
    {
        if (FreeLines <= 0)
        {
            return;
        }

        FillLineAt(_lines++);
    }

    /// <summary>Leaves a blank line at the flow cursor.</summary>
    public void SkipLine()
    {
        if (FreeLines <= 0)
        {
            return;
        }

        _lines++;
    }
}
