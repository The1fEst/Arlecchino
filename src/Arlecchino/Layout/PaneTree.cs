using System;
using Arlecchino.Rendering;
using Arlecchino.Widgets;

namespace Arlecchino.Layout;

/// <summary>
/// A screen described once, by binary space partitioning: every split hands its region to two halves,
/// and every leaf is what goes in that half. Where a chain of <see cref="SurfaceRegion.SplitLeft"/>
/// and <see cref="SurfaceRegion.SplitTop"/> calls spreads the shape of a screen through the whole of
/// <c>Draw</c>, a tree states it in one place — and the view then draws itself in a line:
///
/// <code>
/// _layout = PaneTree.Rows(3,
///     PaneTree.Pane(_toolbar),
///     PaneTree.Columns(0.25,
///         PaneTree.Pane(_tree),
///         PaneTree.Rows(0.7, PaneTree.Pane(_editor), PaneTree.Pane(_log))));
///
/// public void Draw() => _layout.Draw(_surface.Content);
/// </code>
///
/// The tree holds what it draws, so it is built where the widgets are — in the view's constructor —
/// and lives as long as the view does. Sizes are worked out per frame, which is what lets one tree
/// fit any terminal; a region too small for what it holds leaves the panes that did not fit empty
/// rather than overlapping them.
/// </summary>
public sealed class PaneTree
{
    private readonly PaneTree? _first;
    private readonly PaneTree? _second;
    private readonly Action<SurfaceRegion>? _draw;
    private readonly PaneSize _size;
    private readonly bool _sideBySide;

    private PaneTree(Action<SurfaceRegion> draw)
    {
        _draw = draw;
        Count = 1;
    }

    private PaneTree(PaneSize size, bool sideBySide, PaneTree first, PaneTree second)
    {
        _size = size;
        _sideBySide = sideBySide;
        _first = first;
        _second = second;

        Count = first.Count + second.Count;
    }

    /// <summary>How many panes the tree draws.</summary>
    public int Count { get; }

    /// <summary>A pane holding a widget, drawn into whatever region the tree gives it.</summary>
    /// <param name="widget">What goes in the pane.</param>
    /// <returns>The leaf.</returns>
    public static PaneTree Pane(IArlecchinoWidget widget)
    {
        ArgumentNullException.ThrowIfNull(widget);

        return new(region => widget.Draw(region));
    }

    /// <summary>
    /// A pane the view draws itself, for the parts of a screen that are not a widget — a title, a box,
    /// a row of readouts.
    /// </summary>
    /// <param name="draw">What to draw, given the region the pane was allotted.</param>
    /// <returns>The leaf.</returns>
    public static PaneTree Pane(Action<SurfaceRegion> draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        return new(draw);
    }

    /// <summary>A pane that draws nothing, for space deliberately left blank.</summary>
    /// <returns>The leaf.</returns>
    public static PaneTree Empty() => new(static _ => { });

    /// <summary>Two halves side by side.</summary>
    /// <param name="left">How much of the width the left half takes.</param>
    /// <param name="first">The left half.</param>
    /// <param name="second">The right half.</param>
    /// <returns>The split.</returns>
    public static PaneTree Columns(PaneSize left, PaneTree first, PaneTree second) =>
        Split(left, sideBySide: true, first, second);

    /// <summary>Two halves stacked.</summary>
    /// <param name="top">How much of the height the top half takes.</param>
    /// <param name="first">The top half.</param>
    /// <param name="second">The bottom half.</param>
    /// <returns>The split.</returns>
    public static PaneTree Rows(PaneSize top, PaneTree first, PaneTree second) =>
        Split(top, sideBySide: false, first, second);

    /// <summary>
    /// Draws every pane where the splits put it. This is the whole of a view's <c>Draw</c> when the
    /// screen is a tree.
    /// </summary>
    /// <param name="region">The space to fill, usually <c>surface.Content</c>.</param>
    /// <param name="gap">Cells left empty between two halves.</param>
    public void Draw(SurfaceRegion region, int gap = 0) => Place(region, Math.Max(0, gap));

    private static PaneTree Split(PaneSize size, bool sideBySide, PaneTree first, PaneTree second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        return new(size, sideBySide, first, second);
    }

    private void Place(SurfaceRegion region, int gap)
    {
        if (_draw is not null)
        {
            _draw(region);
            return;
        }

        if (_sideBySide)
        {
            var usable = Math.Max(0, region.Width - gap);
            var (left, rest) = region.SplitLeft(_size.Of(usable));
            var (_, right) = rest.SplitLeft(Math.Min(gap, rest.Width));

            _first!.Place(left, gap);
            _second!.Place(right, gap);

            return;
        }

        var rows = Math.Max(0, region.Height - gap);
        var (top, below) = region.SplitTop(_size.Of(rows));
        var (_, bottom) = below.SplitTop(Math.Min(gap, below.Height));

        _first!.Place(top, gap);
        _second!.Place(bottom, gap);
    }
}
