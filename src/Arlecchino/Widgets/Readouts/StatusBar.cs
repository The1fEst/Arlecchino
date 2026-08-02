using System;
using System.Collections.Generic;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Widgets.Readouts;

/// <summary>
/// A line of short readouts pinned to an edge. Items are delegates because a status line is redrawn
/// every frame and is expected to show what is true now, not what was true when it was built.
/// </summary>
public sealed class StatusBar : IArlecchinoWidget
{
    private const string ItemSeparator = "   ";

    /// <summary>Items shown from the left edge. Empty results are skipped, separators and all.</summary>
    public IReadOnlyList<Func<string>> Left { get; init; } = [];

    /// <summary>Items shown from the right edge.</summary>
    public IReadOnlyList<Func<string>> Right { get; init; } = [];

    /// <summary>Colour to draw in. The muted theme colour when left alone.</summary>
    public IArlecchinoColor? Style { get; init; }

    /// <summary>
    /// Draws both groups on the first row and returns the rows below. The left side is truncated to
    /// fit, and the right side is dropped entirely when the two would collide, so the bar never
    /// overlaps itself.
    /// </summary>
    /// <param name="region">Where to draw; only its first row is used.</param>
    /// <returns>The region below the bar.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (region.IsEmpty)
        {
            return region;
        }

        var painted = Style ?? Theme.Muted;
        var left = Join(Left);
        var right = Join(Right);

        region.Write(0, 0, TextWidth.Truncate(left, region.Width), painted);

        var column = region.Width - TextWidth.Of(right);
        if (column > TextWidth.Of(left) + ItemSeparator.Length)
        {
            region.Write(0, column, right, painted);
        }

        return region.Rows(1, region.Height - 1);
    }

    private static string Join(IReadOnlyList<Func<string>> parts)
    {
        var pieces = new List<string>(parts.Count);
        foreach (var part in parts)
        {
            var text = part();
            if (text.Length > 0)
            {
                pieces.Add(text);
            }
        }

        return string.Join(ItemSeparator, pieces);
    }
}
