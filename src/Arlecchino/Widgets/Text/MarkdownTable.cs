using System;
using System.Collections.Generic;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Widgets.Text;

/// <summary>
/// A table read into rows already padded to their columns. A column is as wide as the widest thing in it
/// once the marks are gone: a cell saying <c>**no**</c> is two columns wide and six in the file.
/// </summary>
internal static class MarkdownTable
{
    private const string Separator = " │ ";
    private const string Joint = "─┼─";

    /// <summary>
    /// Reads the run of rows starting at a line, if they are a table at all. A table is a row of headings
    /// with a row of dashes under it; rows that are not that are read as prose.
    /// </summary>
    /// <param name="source">Every line of the document.</param>
    /// <param name="index">The first row.</param>
    /// <param name="lines">Where the rows go.</param>
    /// <returns>How many lines were read, none where they were not a table.</returns>
    public static int Read(string[] source, int index, List<MarkdownLine> lines)
    {
        var rows = new List<string>();
        var at = index;

        while (at < source.Length && source[at].TrimStart().StartsWith('|'))
        {
            rows.Add(source[at].Trim());
            at++;
        }

        if (rows.Count < 2 || !Dashes(rows[1]))
        {
            return 0;
        }

        var alignments = Alignments(Cells(rows[1]));
        var cells = new List<IReadOnlyList<MarkdownRun>[]>();

        for (var row = 0; row < rows.Count; row++)
        {
            if (row != 1)
            {
                cells.Add(Runs(Cells(rows[row]), alignments.Length, row == 0));
            }
        }

        var widths = Widths(cells, alignments.Length);

        lines.Add(Padded(cells[0], widths, alignments));
        lines.Add(Rule(widths));

        for (var row = 1; row < cells.Count; row++)
        {
            lines.Add(Padded(cells[row], widths, alignments));
        }

        return rows.Count;
    }

    /// <summary>The cells of a row, without the bars that hold them.</summary>
    /// <param name="row">The row as it was written.</param>
    /// <returns>What each cell says.</returns>
    private static string[] Cells(string row) => row.Trim('|').Split('|');

    /// <summary>Whether a row is the one made of dashes, which is what says a table is a table.</summary>
    /// <param name="row">The row as it was written.</param>
    /// <returns><c>true</c> when it is that row.</returns>
    private static bool Dashes(string row)
    {
        var dashes = 0;

        foreach (var symbol in row)
        {
            if (symbol == '-')
            {
                dashes++;
            }
            else if (symbol is not ('|' or ':' or ' '))
            {
                return false;
            }
        }

        return dashes > 0;
    }

    /// <summary>Which way each column is read, which the colons in the row of dashes say.</summary>
    /// <param name="cells">That row, cell by cell.</param>
    /// <returns>One alignment per column.</returns>
    private static Align[] Alignments(string[] cells)
    {
        var alignments = new Align[cells.Length];

        for (var column = 0; column < cells.Length; column++)
        {
            var cell = cells[column].Trim();
            var left = cell.StartsWith(':');
            var right = cell.EndsWith(':');

            alignments[column] = (left, right) switch
            {
                (true, true) => Align.Center,
                (false, true) => Align.Right,
                _ => Align.Left,
            };
        }

        return alignments;
    }

    /// <summary>One row read into runs, short of a column where the document left one out.</summary>
    /// <param name="cells">What each cell says.</param>
    /// <param name="columns">How many columns the table has.</param>
    /// <param name="heading">Whether this is the row of headings.</param>
    /// <returns>The runs of each column.</returns>
    private static IReadOnlyList<MarkdownRun>[] Runs(string[] cells, int columns, bool heading)
    {
        var ink = heading ? MarkdownInk.TableHeader : MarkdownInk.Text;
        var runs = new IReadOnlyList<MarkdownRun>[columns];

        for (var column = 0; column < columns; column++)
        {
            runs[column] = column < cells.Length
                ? MarkdownSpans.Of(cells[column].Trim(), ink)
                : [];
        }

        return runs;
    }

    /// <summary>How wide each column is drawn, which is as wide as the widest cell in it.</summary>
    /// <param name="rows">Every row but the dashes.</param>
    /// <param name="columns">How many columns the table has.</param>
    /// <returns>One width per column.</returns>
    private static int[] Widths(List<IReadOnlyList<MarkdownRun>[]> rows, int columns)
    {
        var widths = new int[columns];

        foreach (var row in rows)
        {
            for (var column = 0; column < columns; column++)
            {
                widths[column] = Math.Max(widths[column], Wide(row[column]));
            }
        }

        return widths;
    }

    /// <summary>How many columns a cell takes once it is drawn.</summary>
    /// <param name="runs">The cell.</param>
    /// <returns>Its width.</returns>
    private static int Wide(IReadOnlyList<MarkdownRun> runs)
    {
        var width = 0;

        foreach (var run in runs)
        {
            width += TextWidth.Of(run.Text);
        }

        return width;
    }

    /// <summary>One row, padded to the columns and with the bars between them.</summary>
    /// <param name="cells">The runs of each column.</param>
    /// <param name="widths">How wide each column is drawn.</param>
    /// <param name="alignments">Which way each column is read.</param>
    /// <returns>The line.</returns>
    private static MarkdownLine Padded(
        IReadOnlyList<MarkdownRun>[] cells,
        int[] widths,
        Align[] alignments)
    {
        var runs = new List<MarkdownRun>();

        for (var column = 0; column < cells.Length; column++)
        {
            if (column > 0)
            {
                runs.Add(new(Separator, MarkdownInk.Rule));
            }

            var room = widths[column] - Wide(cells[column]);
            var left = alignments[column] switch
            {
                Align.Right => room,
                Align.Center => room / 2,
                _ => 0,
            };

            Space(runs, left);
            runs.AddRange(cells[column]);
            Space(runs, room - left);
        }

        return new() { Shape = MarkdownShape.Table, Runs = runs };
    }

    /// <summary>The line of dashes under the headings, joined where the columns meet.</summary>
    /// <param name="widths">How wide each column is drawn.</param>
    /// <returns>The line.</returns>
    private static MarkdownLine Rule(int[] widths)
    {
        var runs = new List<MarkdownRun>();

        for (var column = 0; column < widths.Length; column++)
        {
            if (column > 0)
            {
                runs.Add(new(Joint, MarkdownInk.Rule));
            }

            runs.Add(new(new('─', widths[column]), MarkdownInk.Rule));
        }

        return new() { Shape = MarkdownShape.Table, Runs = runs };
    }

    /// <summary>Pads a cell out to its column, where it is short of it.</summary>
    /// <param name="runs">The row being built.</param>
    /// <param name="room">How many columns to fill.</param>
    private static void Space(List<MarkdownRun> runs, int room)
    {
        if (room > 0)
        {
            runs.Add(new(new(' ', room), MarkdownInk.Text));
        }
    }
}
