using System;
using System.Collections.Generic;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Widgets.Readouts;

/// <summary>
/// A series of numbers as one row of blocks, tallest for the largest of them. It says nothing about what the
/// numbers are: no axis, no scale, no grid. That is what lets it sit in a status bar, a table cell or a corner
/// of a pane and still be read at a glance, since the shape of the line is the point.
///
/// The newest value is the rightmost, and only the last of them fit the row, so a widening terminal
/// shows more history rather than a wider drawing of the same history.
/// </summary>
public sealed class Sparkline : IArlecchinoWidget
{
    private const string Blocks = "▁▂▃▄▅▆▇█";

    /// <summary>
    /// The numbers to draw, the oldest first. Nothing is copied, so a ring buffer the application appends
    /// to between frames is exactly the right thing to hand over.
    /// </summary>
    public IReadOnlyList<decimal> Values { get; set; } = [];

    /// <summary>
    /// The value the lowest block stands for. The smallest of the drawn values when left alone, which
    /// makes the line fill the row and answer "how does it move"; pinning it answers "how big is it"
    /// instead, and keeps the line still when the numbers barely change.
    /// </summary>
    public decimal? Minimum { get; init; }

    /// <summary>The value the tallest block stands for. The largest of the drawn values when left alone.</summary>
    public decimal? Maximum { get; init; }

    /// <summary>
    /// Builds the readout drawn after the line, given the newest value. Supplied as a delegate so the
    /// wording and units stay with the application rather than the widget.
    /// </summary>
    public Func<decimal, string>? Caption { get; init; }

    /// <summary>Color of the line. The theme's active color when left alone.</summary>
    public IArlecchinoColor? Style { get; init; }

    /// <summary>
    /// Draws the line across the first row of the region, leaving room for the caption when there is
    /// one, and returns the rows below it. A series with no spread at all — every number the same, or
    /// one number on its own — draws as the lowest block rather than as a full row.
    /// </summary>
    /// <param name="region">Where to draw; only its first row is used.</param>
    /// <returns>The region below the line.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (region.IsEmpty)
        {
            return region;
        }

        var caption = Values.Count > 0 ? Caption?.Invoke(Values[^1]) ?? "" : "";
        var width = Math.Max(0, region.Width - (caption.Length == 0 ? 0 : TextWidth.Of(caption) + 1));
        var first = Math.Max(0, Values.Count - width);
        var (low, span) = Scale(first);
        var line = new char[Math.Max(0, Math.Min(width, Values.Count - first))];

        for (var index = 0; index < line.Length; index++)
        {
            var share = span > 0 ? (Values[first + index] - low) / span : 0m;
            var block = (int)Math.Round(Math.Clamp(share, 0m, 1m) * (Blocks.Length - 1));

            line[index] = Blocks[block];
        }

        region.Write(0, 0, new(line), Style ?? Theme.Active);

        if (caption.Length > 0)
        {
            region.Write(0, width + 1, caption, Theme.Accent);
        }

        return region.Rows(1, region.Height - 1);
    }

    private (decimal Low, decimal Span) Scale(int first)
    {
        if (first >= Values.Count)
        {
            return (0m, 0m);
        }

        var low = Minimum ?? Values[first];
        var high = Maximum ?? Values[first];

        for (var index = first + 1; index < Values.Count; index++)
        {
            if (Minimum is null)
            {
                low = Math.Min(low, Values[index]);
            }

            if (Maximum is null)
            {
                high = Math.Max(high, Values[index]);
            }
        }

        return (low, Math.Max(0m, high - low));
    }
}

/// <summary>
/// A series drawn as a filled area over as many rows as it is given — the shape a system monitor shows. Where
/// <see cref="Sparkline"/> fits a row and reads at a glance, this one fills a pane and is meant to be looked
/// at. The newest value is at the right, the fill climbs with the value, and the color comes from how high it
/// climbed rather than from anything the view works out.
///
/// A series with no spread at all — every number the same — draws as the lowest level along the bottom rather
/// than as nothing, the way a <see cref="Sparkline"/> does.
///
/// The resolution is in the characters. A cell carries two samples side by side and several levels of height,
/// so a chart eight rows tall has thirty-two levels between empty and full and holds twice the history a row
/// of blocks would. See <see cref="GraphSymbols"/> for what each set costs in font support.
/// </summary>
public sealed class AreaChart : IArlecchinoWidget
{
    /// <summary>
    /// The numbers to draw, the oldest first. Nothing is copied, so a ring buffer the application
    /// appends to between frames is exactly the right thing to hand over.
    /// </summary>
    public IReadOnlyList<decimal> Values { get; set; } = [];

    /// <summary>The value an empty chart stands for. The smallest of the drawn values when left alone.</summary>
    public decimal? Minimum { get; init; }

    /// <summary>The value a full chart stands for. The largest of the drawn values when left alone.</summary>
    public decimal? Maximum { get; init; }

    /// <summary>
    /// What to draw with. The application's own setting — <see cref="Glyphs.Graph"/> — when left
    /// alone, so one chart can differ without every other one being told.
    /// </summary>
    public GraphSymbols? Symbols { get; set; }

    /// <summary>
    /// Where the color changes as the fill climbs, in the same units as the values and in ascending
    /// order. A terminal with truecolor blends between them, one without takes the nearest, and a
    /// chart given none is drawn in <see cref="Style"/> throughout.
    /// </summary>
    public IReadOnlyList<GaugeBand> Bands { get; init; } = [];

    /// <summary>Color of the fill outside every band. The theme's active color when left alone.</summary>
    public IArlecchinoColor? Style { get; init; }

    /// <summary>
    /// Draws it hanging from the top rather than standing on the bottom, for the second half of a
    /// mirrored pair — what comes in above, what goes out below.
    /// </summary>
    public bool Invert { get; init; }

    /// <summary>
    /// Draws the chart over every row of the region and returns what is left, which is nothing. A chart fills
    /// what it is given, so hand over the pane it belongs in.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <returns>An empty region.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (region.IsEmpty)
        {
            return region;
        }

        var glyphs = GraphGlyphs.Chosen(Symbols ?? Glyphs.Graph);
        var perCell = glyphs.PerCell;
        var levels = glyphs.Levels;

        var samples = region.Width * perCell;
        var first = Math.Max(0, Values.Count - samples);
        var oldest = Values.Count - samples;
        var (low, span) = Scale(first);
        var row = new char[region.Width];

        for (var line = 0; line < region.Height; line++)
        {
            var band = Invert ? line : region.Height - 1 - line;
            var floor = (decimal)band / region.Height;
            var ceiling = (decimal)(band + 1) / region.Height;

            for (var cell = 0; cell < region.Width; cell++)
            {
                var slot = oldest + (cell * perCell);

                row[cell] = glyphs.Of(
                    LevelAt(slot, floor, ceiling, low, span, levels),
                    LevelAt(slot + 1, floor, ceiling, low, span, levels),
                    Invert);
            }

            region.Write(line, 0, new(row), ColourOf(ceiling, low, span));
        }

        return region.Rows(region.Height, 0);
    }

    private int LevelAt(int index, decimal floor, decimal ceiling, decimal low, decimal span, int levels)
    {
        if (index < 0 || index >= Values.Count)
        {
            return 0;
        }

        if (span <= 0)
        {
            return floor == 0 ? 1 : 0;
        }

        var share = Math.Clamp((Values[index] - low) / span, 0m, 1m);

        if (share >= ceiling)
        {
            return levels;
        }

        if (share <= floor)
        {
            return 0;
        }

        var within = (share - floor) / (ceiling - floor) * levels;

        return Math.Clamp((int)Math.Ceiling(within), 1, levels);
    }

    private IArlecchinoColor ColourOf(decimal ceiling, decimal low, decimal span)
    {
        var fallback = Style ?? Theme.Active;

        if (Bands.Count == 0)
        {
            return fallback;
        }

        var value = low + (span * ceiling);
        var found = -1;

        for (var index = 0; index < Bands.Count && Bands[index].From <= value; index++)
        {
            found = index;
        }

        if (found < 0)
        {
            return fallback;
        }

        var below = Bands[found];

        if (found == Bands.Count - 1 ||
            below.Style is not TermColor from ||
            Bands[found + 1].Style is not TermColor to)
        {
            return below.Style;
        }

        var above = Bands[found + 1];
        var reached = above.From > below.From ? (value - below.From) / (above.From - below.From) : 0m;

        return Blend(from, to, Math.Clamp(reached, 0m, 1m));
    }

    private static TermColor Blend(TermColor from, TermColor to, decimal reached) =>
        from.ExactForeground is { } start && to.ExactForeground is { } end
            ? new()
            {
                Foreground = reached < 0.5m ? from.Foreground : to.Foreground,
                ExactForeground = new(
                    (byte)(start.Red + ((end.Red - start.Red) * reached)),
                    (byte)(start.Green + ((end.Green - start.Green) * reached)),
                    (byte)(start.Blue + ((end.Blue - start.Blue) * reached))),
            }
            : from;

    private (decimal Low, decimal Span) Scale(int first)
    {
        if (first >= Values.Count)
        {
            return (0m, 0m);
        }

        var low = Minimum ?? Values[first];
        var high = Maximum ?? Values[first];

        for (var index = first + 1; index < Values.Count; index++)
        {
            if (Minimum is null)
            {
                low = Math.Min(low, Values[index]);
            }

            if (Maximum is null)
            {
                high = Math.Max(high, Values[index]);
            }
        }

        return (low, Math.Max(0m, high - low));
    }
}

/// <summary>
/// One bar per item, laid out down the region: the label in front, the bar across the middle, the
/// readout behind. Bars are measured against the largest item unless told otherwise, so a chart of
/// things that are all small still fills the pane instead of drawing four invisible stubs.
/// </summary>
/// <typeparam name="T">What the chart draws a bar for.</typeparam>
public sealed class BarChart<T> : IArlecchinoWidget
{
    private const char FilledCell = '█';
    private const char EmptyCell = '░';
    private const int LabelShare = 3;

    /// <summary>Turns an item into the label in front of its bar. Longer labels are truncated by column.</summary>
    public required Func<T, string> Render { get; init; }

    /// <summary>The number the length of the bar stands for. Anything below zero draws as an empty bar.</summary>
    public required Func<T, decimal> Value { get; init; }

    /// <summary>Colors one bar, for charts where a row means something — over budget, offline, picked.</summary>
    public Func<T, IArlecchinoColor>? ItemStyle { get; set; }

    /// <summary>
    /// Builds the readout drawn after each bar, given that bar's value. The readouts share one column,
    /// as wide as the longest of them, so the numbers line up under one another.
    /// </summary>
    public Func<decimal, string>? Caption { get; init; }

    /// <summary>What to chart, one bar per row. Replacing it between frames is a normal thing to do.</summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>
    /// The value at which a bar is full. The largest of the items when left alone; pin it to compare
    /// one frame against the next, or to keep a percentage chart honest when nothing has reached 100
    /// yet.
    /// </summary>
    public decimal? Maximum { get; init; }

    /// <summary>
    /// Columns kept for the labels. The widest label when left alone, up to a third of the region so a
    /// long name cannot squeeze the bars out of the pane.
    /// </summary>
    public int LabelWidth { get; init; }

    /// <summary>
    /// Draws a bar for every item that fits and returns the rows below them, so a chart shorter than
    /// its pane leaves the rest to whatever comes next. Items past the bottom of the region are not
    /// drawn: the chart does not scroll, which is what keeps it readable without the focus.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <returns>The region below the bars.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        var rows = Math.Max(0, Math.Min(Items.Count, region.Height));

        if (region.IsEmpty || rows == 0)
        {
            return region;
        }

        var labels = new string[rows];
        var captions = new string[rows];
        var values = new decimal[rows];
        var widest = 0;
        var readout = 0;
        var highest = Maximum ?? 0m;

        for (var row = 0; row < rows; row++)
        {
            var item = Items[row];

            values[row] = Value(item);
            labels[row] = Render(item);
            captions[row] = Caption?.Invoke(values[row]) ?? "";
            widest = Math.Max(widest, TextWidth.Of(labels[row]));
            readout = Math.Max(readout, TextWidth.Of(captions[row]));

            if (Maximum is null)
            {
                highest = Math.Max(highest, values[row]);
            }
        }

        var labelled = LabelWidth > 0
            ? LabelWidth
            : Math.Min(widest, Math.Max(1, region.Width / LabelShare));
        var track = Math.Max(0, region.Width - labelled - 1 - (readout == 0 ? 0 : readout + 1));

        for (var row = 0; row < rows; row++)
        {
            var share = highest > 0 ? Math.Clamp(values[row] / highest, 0m, 1m) : 0m;
            var filled = (int)Math.Round(share * track);

            region.Write(row, 0, TextWidth.PadRight(TextWidth.Truncate(labels[row], labelled), labelled), Theme.Default);
            region.Write(row, labelled + 1, new(FilledCell, filled), ItemStyle?.Invoke(Items[row]) ?? Theme.Active);
            region.Write(row, labelled + 1 + filled, new(EmptyCell, track - filled), Theme.Muted);

            if (readout > 0)
            {
                region.Write(row, labelled + 1 + track + 1, TextWidth.PadLeft(captions[row], readout), Theme.Accent);
            }
        }

        return region.Rows(rows, region.Height - rows);
    }
}

/// <summary>
/// Where a band of a <see cref="Gauge"/> starts and how it is drawn. A band runs from
/// <see cref="From"/> up to the start of the next one, so the bands are given in order and the first
/// of them decides the color of everything below it.
/// </summary>
/// <param name="From">The value the band starts at.</param>
/// <param name="Style">How the part of the track inside the band is drawn.</param>
public readonly record struct GaugeBand(decimal From, IArlecchinoColor Style);

/// <summary>
/// One value against a range that means something, drawn as a bar whose color changes as it crosses the bands
/// it was given. The fill turns amber where the load is worth watching and red where it is not. Each part
/// keeps the color of the band it lies in, so the tail of the bar shows how long it has been past the line.
///
/// A <see cref="ProgressBar"/> answers "how far along", and this answers "how bad is it now" — the
/// difference being the bands, and a range that need not start at zero.
/// </summary>
public sealed class Gauge : IArlecchinoWidget
{
    private const char FilledCell = '█';
    private const char EmptyCell = '░';

    /// <summary>Value at which the gauge reads empty.</summary>
    public decimal Minimum { get; init; }

    /// <summary>Value at which the gauge reads full.</summary>
    public decimal Maximum { get; init; } = 100;

    /// <summary>What it reads now. Anything outside the range draws as an empty or a full gauge.</summary>
    public decimal Value { get; set; }

    /// <summary>
    /// The bands the track is colored by, in ascending order of <see cref="GaugeBand.From"/>. Without
    /// them the whole fill takes <see cref="Style"/>, which makes the gauge a bar with a range.
    /// </summary>
    public IReadOnlyList<GaugeBand> Bands { get; init; } = [];

    /// <summary>Builds the text drawn after the gauge, given the value.</summary>
    public Func<decimal, string>? Caption { get; init; }

    /// <summary>Color of the fill outside every band. The theme's active color when left alone.</summary>
    public IArlecchinoColor? Style { get; init; }

    /// <summary>How full the gauge is, from <c>0</c> to <c>1</c>. An empty range reads as <c>0</c>.</summary>
    public decimal Fraction => Maximum > Minimum
        ? Math.Clamp((Value - Minimum) / (Maximum - Minimum), 0m, 1m)
        : 0m;

    /// <summary>
    /// How a value of the range is drawn: the style of the last band at or below it, and
    /// <see cref="Style"/> when it is under every band. Useful for coloring a label the same way the
    /// gauge under it is colored.
    /// </summary>
    /// <param name="value">The value to look up.</param>
    /// <returns>The style that part of the track takes.</returns>
    public IArlecchinoColor StyleAt(decimal value) => BandAt(value) is var band && band >= 0
        ? Bands[band].Style
        : Style ?? Theme.Active;

    /// <summary>
    /// Draws the gauge across the first row of the region, leaving room for the caption when there is
    /// one, and returns the rows below it.
    /// </summary>
    /// <param name="region">Where to draw; only its first row is used.</param>
    /// <returns>The region below the gauge.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (region.IsEmpty)
        {
            return region;
        }

        var caption = Caption?.Invoke(Value) ?? "";
        var track = Math.Max(0, region.Width - (caption.Length == 0 ? 0 : TextWidth.Of(caption) + 1));
        var filled = (int)Math.Round(Fraction * track);
        var written = 0;

        while (written < filled)
        {
            var band = BandAt(ValueAt(written, track));
            var run = written + 1;

            while (run < filled && BandAt(ValueAt(run, track)) == band)
            {
                run++;
            }

            region.Write(0, written, new(FilledCell, run - written), band >= 0 ? Bands[band].Style : Style ?? Theme.Active);
            written = run;
        }

        region.Write(0, filled, new(EmptyCell, Math.Max(0, track - filled)), Theme.Muted);

        if (caption.Length > 0)
        {
            region.Write(0, track + 1, caption, StyleAt(Value));
        }

        return region.Rows(1, region.Height - 1);
    }

    private decimal ValueAt(int cell, int track) =>
        track > 0 ? Minimum + ((Maximum - Minimum) * cell / track) : Minimum;

    private int BandAt(decimal value)
    {
        var found = -1;

        for (var index = 0; index < Bands.Count && Bands[index].From <= value; index++)
        {
            found = index;
        }

        return found;
    }
}
