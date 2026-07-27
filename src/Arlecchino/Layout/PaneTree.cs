using System;
using Arlecchino.Rendering;
using Arlecchino.Widgets;

namespace Arlecchino.Layout;

/// <summary>
/// A screen described once, by binary space partitioning: every branch hands its region to two halves,
/// and every leaf is what goes in that half. Where a chain of <see cref="SurfaceRegion.SplitLeft"/>
/// and <see cref="SurfaceRegion.SplitTop"/> calls spreads the shape of a screen through the whole of
/// <c>Draw</c>, a tree states it in one place — and the view then draws itself in a line.
///
/// Two members build it, so a tree reads as a tree: <see cref="Branch(PaneTree, PaneTree)"/> and
/// <see cref="Leaf(IArlecchinoWidget)"/>. Only the two halves of a branch are ever required — say
/// which way it cuts, or how much the first half takes, only where it matters:
///
/// <code>
/// _layout = Branch(Rows, 3,
///     Leaf(_toolbar),
///     Branch(Columns, 0.25,
///         Leaf(_tree),
///         Branch(Leaf(_editor), Leaf(_log)))).Gaps(inner: 1);
///
/// public void Draw() => _layout.Draw(_surface.Content);
/// </code>
///
/// A <c>using static</c> of this type and of <see cref="PaneSplit"/> is what lets it read that way.
///
/// The tree holds what it draws, so it is built where the widgets are — in the view's constructor —
/// and lives as long as the view does. Sizes are worked out per frame, which is what lets one tree
/// fit any terminal; a region too small for what it holds leaves the panes that did not fit empty
/// rather than overlapping them.
/// </summary>
public sealed class PaneTree
{
    private const int CellAspect = 2;

    private readonly PaneTree? _first;
    private readonly PaneTree? _second;
    private readonly Action<SurfaceRegion>? _draw;
    private readonly PaneSize _size;
    private readonly PaneSplit? _split;

    private PaneTree(Action<SurfaceRegion> draw)
    {
        _draw = draw;
        Count = 1;
    }

    private PaneTree(PaneSplit? split, PaneSize size, PaneTree first, PaneTree second)
    {
        _split = split;
        _size = size;
        _first = first;
        _second = second;

        Count = first.Count + second.Count;
    }

    /// <summary>How many panes the tree draws.</summary>
    public int Count { get; }

    /// <summary>Cells left empty between the two halves of every branch. Set by <see cref="Gaps"/>.</summary>
    public int InnerGap { get; private set; }

    /// <summary>Cells left empty around the whole layout. Set by <see cref="Gaps"/>.</summary>
    public int OuterGap { get; private set; }

    /// <summary>A pane holding a widget, drawn into whatever region the tree gives it.</summary>
    /// <param name="widget">What goes in the pane.</param>
    /// <returns>The leaf.</returns>
    public static PaneTree Leaf(IArlecchinoWidget widget)
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
    public static PaneTree Leaf(Action<SurfaceRegion> draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        return new(draw);
    }

    /// <summary>A pane that draws nothing, for space deliberately left blank.</summary>
    /// <returns>The leaf.</returns>
    public static PaneTree Leaf() => new(static _ => { });

    /// <summary>
    /// A branch that decides everything itself: it cuts along the longer side of whatever region it is
    /// given and halves it. The longer side is measured in what the eye sees rather than in cells —
    /// a cell is about twice as tall as it is wide, so 80×24 is a wide region and gets two columns.
    ///
    /// Because the side is measured per frame, such a branch can turn from columns into rows when the
    /// terminal is resized. That is what makes it right for panes of equal standing, and wrong for
    /// chrome, which should be pinned with a <see cref="PaneSplit"/> of its own.
    /// </summary>
    /// <param name="first">The upper half, or the left one.</param>
    /// <param name="second">The lower half, or the right one.</param>
    /// <returns>The branch.</returns>
    public static PaneTree Branch(PaneTree first, PaneTree second) =>
        Build(null, PaneSize.Fraction(0.5), first, second);

    /// <summary>A branch that cuts the way it is told and halves the space.</summary>
    /// <param name="split">Which way to cut.</param>
    /// <param name="first">The upper half, or the left one.</param>
    /// <param name="second">The lower half, or the right one.</param>
    /// <returns>The branch.</returns>
    public static PaneTree Branch(PaneSplit split, PaneTree first, PaneTree second) =>
        Build(split, PaneSize.Fraction(0.5), first, second);

    /// <summary>
    /// A branch of a given size that still cuts along the longer side, for a split that is uneven but
    /// has no reason to prefer an axis.
    /// </summary>
    /// <param name="size">How much of it the first half takes; the second half takes the rest.</param>
    /// <param name="first">The upper half, or the left one.</param>
    /// <param name="second">The lower half, or the right one.</param>
    /// <returns>The branch.</returns>
    public static PaneTree Branch(PaneSize size, PaneTree first, PaneTree second) =>
        Build(null, size, first, second);

    /// <summary>
    /// A branch that says both: the space is cut the given way and each half goes to a subtree, which
    /// is itself either a branch or a leaf. Three bands is therefore a branch inside a branch.
    /// </summary>
    /// <param name="split">Which way to cut.</param>
    /// <param name="size">How much of it the first half takes; the second half takes the rest.</param>
    /// <param name="first">The upper half, or the left one.</param>
    /// <param name="second">The lower half, or the right one.</param>
    /// <returns>The branch.</returns>
    public static PaneTree Branch(PaneSplit split, PaneSize size, PaneTree first, PaneTree second) =>
        Build(split, size, first, second);

    /// <summary>
    /// Sets the spacing of the whole layout, rather than of one branch, so a screen is loosened or
    /// tightened in one place. The names are the ones a tiling window manager uses.
    /// </summary>
    /// <param name="inner">Cells left empty between the two halves of every branch.</param>
    /// <param name="outer">Cells left empty around everything, inside the region handed to <see cref="Draw"/>.</param>
    /// <returns>The same tree, so the call finishes the expression that built it.</returns>
    public PaneTree Gaps(int inner, int outer = 0)
    {
        InnerGap = Math.Max(0, inner);
        OuterGap = Math.Max(0, outer);

        return this;
    }

    /// <summary>
    /// Draws every pane where the branches put it. This is the whole of a view's <c>Draw</c> when the
    /// screen is a tree.
    /// </summary>
    /// <param name="region">The space to fill, usually <c>surface.Content</c>.</param>
    public void Draw(SurfaceRegion region) =>
        Place(OuterGap > 0 ? region.Inset(OuterGap) : region, InnerGap);

    private static PaneTree Build(PaneSplit? split, PaneSize size, PaneTree first, PaneTree second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        return new(split, size, first, second);
    }

    private void Place(SurfaceRegion region, int gap)
    {
        if (_draw is not null)
        {
            _draw(region);
            return;
        }

        if (SplitOf(region) == PaneSplit.Columns)
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

    private PaneSplit SplitOf(SurfaceRegion region) =>
        _split ?? (region.Width >= region.Height * CellAspect ? PaneSplit.Columns : PaneSplit.Rows);
}
