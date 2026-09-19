using System;
using System.Collections.Generic;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Widgets.Text;

/// <summary>One row of a document as it is drawn, which is one line of a pane.</summary>
/// <param name="Shape">What the line it came from was.</param>
/// <param name="Indent">Which column it starts at.</param>
/// <param name="Runs">What it says, left to right.</param>
internal readonly record struct MarkdownRow(MarkdownShape Shape, int Indent, IReadOnlyList<MarkdownRun> Runs);

/// <summary>
/// Where the pictures of one line go: the rows kept clear for them, and which pictures they are for.
/// Several of them share the band, side by side, taking a share of its width each.
/// </summary>
/// <param name="Row">The first row they take.</param>
/// <param name="Rows">How many they take.</param>
/// <param name="Addresses">Which pictures, left to right.</param>
internal readonly record struct MarkdownPlate(int Row, int Rows, IReadOnlyList<string> Addresses);

/// <summary>
/// A read document laid out for a width: prose broken between words, and a quote's bar written again on
/// every row it takes. Code and tables are left whole, a line broken in either saying what it did not.
/// </summary>
internal static class MarkdownWrap
{
    /// <summary>Lays a document out.</summary>
    /// <param name="lines">The document as it was read.</param>
    /// <param name="width">How many columns there are to draw in.</param>
    /// <param name="pictures">
    /// How many rows a picture takes in the columns it is given, asked of whoever can draw one. A picture
    /// nothing can draw takes none, and the words that stand for it are written instead.
    /// </param>
    /// <returns>The rows to draw, in order, and where the pictures among them go.</returns>
    public static (List<MarkdownRow> Rows, List<MarkdownPlate> Plates) Of(
        IReadOnlyList<MarkdownLine> lines,
        int width,
        Func<string, int, int>? pictures = null)
    {
        var rows = new List<MarkdownRow>();
        var plates = new List<MarkdownPlate>();

        foreach (var line in lines)
        {
            var lead = Wide(line.Lead);
            var marker = Wide(line.Marker);
            var left = line.Indent + lead;

            if (line is { Shape: MarkdownShape.Picture, Pictures.Count: > 0 })
            {
                var columns = Math.Max(1, (width - left) / line.Pictures.Count);
                var band = 0;

                foreach (var address in line.Pictures)
                {
                    band = Math.Max(band, pictures?.Invoke(address, columns) ?? 0);
                }

                if (band > 0)
                {
                    plates.Add(new(rows.Count, band, line.Pictures));

                    for (var row = 0; row < band; row++)
                    {
                        rows.Add(new(line.Shape, line.Indent, line.Lead));
                    }

                    continue;
                }
            }

            if (line.Shape == MarkdownShape.Rule)
            {
                var rule = new MarkdownRun(new('─', Math.Max(1, width - left)), MarkdownInk.Rule);

                rows.Add(new(line.Shape, line.Indent, [.. line.Lead, rule]));
                continue;
            }

            if (line.Shape is MarkdownShape.Code or MarkdownShape.Table)
            {
                rows.Add(new(line.Shape, line.Indent, [.. line.Lead, .. line.Marker, .. line.Runs]));
                continue;
            }

            var brokenRows = Broken(line.Runs, Math.Max(1, width - left - marker));

            if (brokenRows.Count == 0)
            {
                rows.Add(new(line.Shape, line.Indent, line.Lead));
                continue;
            }

            for (var row = 0; row < brokenRows.Count; row++)
            {
                rows.Add(row == 0
                    ? new(line.Shape, line.Indent, [.. line.Lead, .. line.Marker, .. brokenRows[row]])
                    : new(line.Shape, line.Indent, [.. line.Lead, Blank(marker), .. brokenRows[row]]));
            }
        }

        return (rows, plates);
    }

    /// <summary>What stands where a bullet stood on the rows after the first one.</summary>
    /// <param name="marker">How wide that bullet was.</param>
    /// <returns>A run of that many spaces.</returns>
    private static MarkdownRun Blank(int marker) => new(new(' ', marker), MarkdownInk.Text);

    /// <summary>How many columns a stretch of runs takes.</summary>
    /// <param name="runs">The runs.</param>
    /// <returns>Their width.</returns>
    private static int Wide(IReadOnlyList<MarkdownRun> runs)
    {
        var width = 0;

        foreach (var run in runs)
        {
            width += TextWidth.Of(run.Text);
        }

        return width;
    }

    /// <summary>
    /// Breaks runs between words to a width, breaking inside a word too wide for a row of its own. The
    /// space a run ends on is carried to the run after it, the two being drawn end to end.
    /// </summary>
    /// <param name="runs">What the line says.</param>
    /// <param name="room">How many columns each row has.</param>
    /// <returns>The runs of each row.</returns>
    private static List<List<MarkdownRun>> Broken(IReadOnlyList<MarkdownRun> runs, int room)
    {
        var rows = new List<List<MarkdownRun>>();
        var row = new List<MarkdownRun>();
        var used = 0;
        var spaced = false;

        foreach (var run in runs)
        {
            var words = run.Text.Split(' ');

            for (var index = 0; index < words.Length; index++)
            {
                spaced |= index > 0;

                var word = words[index];

                if (word.Length == 0)
                {
                    continue;
                }

                if (used > 0 && used + TextWidth.Of(word) + (spaced ? 1 : 0) > room)
                {
                    rows.Add(row);
                    row = [];
                    used = 0;
                }

                if (spaced && used > 0)
                {
                    row.Add(run with { Text = " " });
                    used++;
                }

                spaced = false;

                var rest = word;

                while (TextWidth.Of(rest) > room - used)
                {
                    var head = TextWidth.Truncate(rest, room - used);

                    if (head.Length == 0)
                    {
                        break;
                    }

                    row.Add(run with { Text = head });
                    rest = rest[head.Length..];
                    rows.Add(row);
                    row = [];
                    used = 0;
                }

                if (rest.Length == 0)
                {
                    continue;
                }

                row.Add(run with { Text = rest });
                used += TextWidth.Of(rest);
            }
        }

        if (row.Count > 0)
        {
            rows.Add(row);
        }

        return rows;
    }
}
