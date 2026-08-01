using System;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>
/// The bar down the side of a list that shows how much of it is on screen and where. Drawn only when
/// there is more than fits, so a short list keeps its full width.
/// </summary>
public static class ScrollBar
{
    private const char TrackCell = '│';
    private const char ThumbCell = '█';

    /// <summary>
    /// Whether a list of this length needs a bar at all, which is also whether a column has to be kept
    /// free for it.
    /// </summary>
    /// <param name="total">How many items there are.</param>
    /// <param name="rows">How many rows they are drawn into.</param>
    /// <returns><c>true</c> when some of the list is off screen.</returns>
    public static bool IsNeeded(int total, int rows) => rows > 0 && total > rows;

    /// <summary>
    /// Draws the bar down the last column of a region. The thumb is at least one cell tall however long
    /// the list is, and it only touches the ends when the list does, so "near the end" never looks the
    /// same as "at the end".
    /// </summary>
    /// <param name="region">Where the rows were drawn; the last column is used.</param>
    /// <param name="first">Index of the first item on screen.</param>
    /// <param name="total">How many items there are.</param>
    /// <param name="style">Colour of the thumb. Defaults to the theme's active colour.</param>
    public static void Draw(SurfaceRegion region, int first, int total, IArlecchinoColor? style = null)
    {
        var rows = region.Height;
        if (region.IsEmpty || !IsNeeded(total, rows))
        {
            return;
        }

        var column = region.Width - 1;
        var thumbRows = Math.Max(1, rows * rows / total);
        var lastStart = rows - thumbRows;
        var scrolled = total - rows;
        var thumbStart = Math.Clamp(first * lastStart / scrolled, 0, lastStart);

        for (var row = 0; row < rows; row++)
        {
            var onThumb = row >= thumbStart && row < thumbStart + thumbRows;
            region.Write(row,
                column,
                onThumb ? ThumbCell.ToString() : TrackCell.ToString(),
                onThumb ? style ?? Theme.Active : Theme.Muted);
        }
    }
}
