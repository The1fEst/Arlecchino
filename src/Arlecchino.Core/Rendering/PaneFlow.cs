using System;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Rendering;

/// <summary>
/// A flow cursor inside one region: it writes the next line and remembers where the one after it goes,
/// clipped to the region.
///
/// <code>
/// var flow = region.Flow();
///
/// flow.AppendLine("PLAYERS", Theme.TableHeader);
///
/// foreach (var player in players)
/// {
///     flow.AppendLine(player.Name, Theme.Default);
/// }
/// </code>
/// </summary>
public sealed class PaneFlow
{
    private readonly SurfaceRegion _region;

    internal PaneFlow(SurfaceRegion region)
    {
        _region = region;
    }

    /// <summary>The row the next line goes on, counted from the top of the region.</summary>
    public int Row { get; private set; }

    /// <summary>How many rows are left. Zero once the region is full.</summary>
    public int FreeLines => Math.Max(0, _region.Height - Row);

    /// <summary>Whether there is any room left to write in.</summary>
    public bool IsFull => FreeLines <= 0;

    /// <summary>The region being written into.</summary>
    public SurfaceRegion Region => _region;

    /// <summary>
    /// Writes one line at the cursor and moves it down. Once the region is full the call does nothing,
    /// so a loop over more rows than fit needs no bound of its own.
    /// </summary>
    /// <param name="line">Text to write.</param>
    /// <param name="style">Style for the line; the default role when omitted.</param>
    /// <param name="align">Horizontal alignment inside the region.</param>
    public void AppendLine(string line, IArlecchinoColor? style = null, Align align = Align.Left)
    {
        if (IsFull)
        {
            return;
        }

        _region.WriteLine(Row, line, style ?? Theme.Default, align);
        Row++;
    }

    /// <summary>Leaves the next row blank.</summary>
    public void SkipLine() => Skip(1);

    /// <summary>Leaves several rows blank.</summary>
    /// <param name="rows">How many to leave.</param>
    public void Skip(int rows) => Row = Math.Min(_region.Height, Row + Math.Max(0, rows));

    /// <summary>Draws a rule of <c>-</c> across the region and moves the cursor down.</summary>
    /// <param name="style">Style for the rule; the default role when omitted.</param>
    public void FillLine(IArlecchinoColor? style = null) =>
        AppendLine(new('-', Math.Max(0, _region.Width)), style);

    /// <summary>Puts the cursor back on the first row of the region.</summary>
    public void Rewind() => Row = 0;

    /// <summary>
    /// The rows the cursor has not reached yet, as a region of their own — for handing what is left of
    /// a pane to a widget once the lines above it are written.
    /// </summary>
    /// <returns>The region below the cursor, empty when there is nothing left.</returns>
    public SurfaceRegion Rest() => _region.Rows(Row, FreeLines);
}
