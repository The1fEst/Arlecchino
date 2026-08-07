using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Widgets;

namespace Arlecchino.Layout;

/// <summary>
/// A screen described once, by binary space partitioning: every branch hands its region to two halves,
/// and every leaf is what goes in that half. Where a chain of <see cref="SurfaceRegion.SplitLeft"/>
/// and <see cref="SurfaceRegion.SplitTop"/> calls spreads the shape of a screen through the whole of
/// <c>Draw</c>, a tree states it in one place — and the view then draws itself in a line.
///
/// Two members build it, so a tree reads as a tree: <see cref="Branch(PaneTree, PaneTree)"/> and
/// <see cref="Leaf(IArlecchinoWidget)"/>. Only the two halves of a
/// branch are ever required — say which way it cuts, or how much the first half takes, only where it
/// matters:
///
/// <code>
/// _layout = Branch(Rows, 3,
///     Leaf(_toolbar),
///     Branch(Columns, 0.25,
///         Leaf(_tree, () => "files"),
///         Branch(Leaf(_editor, () => "editor"), Leaf(_log, () => "log")))).Gaps(inner: 1);
///
/// _focus = new FocusRing(options.Keymap);
/// _focus.AddAll(_layout.Focusables);
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

    private static readonly IArlecchinoWidget[] NoWidgets = [];

    private readonly PaneTree? _first;
    private readonly PaneTree? _second;
    private readonly Action<SurfaceRegion>? _content;
    private readonly Func<string>? _title;
    private readonly IArlecchinoWidget? _widget;
    private readonly IArlecchinoWidget[] _widgets;
    private readonly PaneSize _size;
    private readonly PaneSplit? _split;

    private PaneTree(Action<SurfaceRegion> content, IArlecchinoWidget? widget, Func<string>? title = null)
    {
        _content = content;
        _title = title;
        _widget = widget;
        _widgets = widget is null ? NoWidgets : [widget];

        Count = 1;
    }

    private PaneTree(PaneSplit? split, PaneSize size, PaneTree first, PaneTree second)
    {
        _split = split;
        _size = size;
        _first = first;
        _second = second;
        _widgets = [.. first._widgets, .. second._widgets];

        Count = first.Count + second.Count;
    }

    /// <summary>How many panes the tree draws.</summary>
    public int Count { get; }

    /// <summary>Cells left empty between the two halves of every branch. Set by <see cref="Gaps"/>.</summary>
    public int InnerGap { get; private set; }

    /// <summary>Cells left empty around the whole layout. Set by <see cref="Gaps"/>.</summary>
    public int OuterGap { get; private set; }

    /// <summary>
    /// Builds the focus ring of the screen from the tree: every widget of it that takes the focus, in
    /// the order the branches lay them out — left before right, top before bottom. <c>Tab</c> then
    /// walks the screen the way it looks, and there is no second list to keep in step with the first.
    ///
    /// <code>
    /// _focus = _layout.AsFocusRing(options.Keymap);
    /// </code>
    ///
    /// What comes back is an ordinary <see cref="FocusRing"/>, so anything focusable that lives
    /// outside the tree is added to it afterward, and lands at the end of the walk.
    /// </summary>
    /// <param name="keymap">Where the keys that move the focus come from.</param>
    /// <returns>A ring holding the panes that take the focus.</returns>
    public FocusRing AsFocusRing(ArlecchinoKeymap keymap)
    {
        var ring = new FocusRing(keymap);

        foreach (var widget in _widgets)
        {
            if (widget is IArlecchinoFocusable focusable)
            {
                ring.Add(focusable);
            }
        }

        return ring;
    }

    /// <summary>A pane holding a widget, drawn into whatever region the tree gives it.</summary>
    /// <param name="widget">What goes in the pane.</param>
    /// <returns>The leaf.</returns>
    public static PaneTree Leaf(IArlecchinoWidget widget)
    {
        ArgumentNullException.ThrowIfNull(widget);

        return new(region => widget.Draw(region), widget);
    }

    /// <summary>
    /// A pane holding a widget, in a box with a title. The widget is drawn in the room left inside the box.
    /// The box itself is drawn <c>Theme.Active</c> while the widget holds the focus and <c>Theme.Info</c> while
    /// it does not, so a screen of panes shows where the cursor is without the view saying anything about it.
    /// </summary>
    /// <param name="widget">What goes in the pane.</param>
    /// <param name="title">
    /// What to write in the top border. A delegate rather than a string, like every other piece of
    /// user-visible text in the framework, so a translated application translates it too.
    /// </param>
    /// <returns>The leaf.</returns>
    public static PaneTree Leaf(IArlecchinoWidget widget, Func<string> title)
    {
        ArgumentNullException.ThrowIfNull(widget);
        ArgumentNullException.ThrowIfNull(title);

        return new(region => widget.Draw(region), widget, title);
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

        return new(draw, null);
    }

    /// <summary>A pane the view draws itself, in a box with a title.</summary>
    /// <param name="draw">What to draw, given the room left inside the box.</param>
    /// <param name="title">What to write in the top border.</param>
    /// <returns>The leaf.</returns>
    public static PaneTree Leaf(Action<SurfaceRegion> draw, Func<string> title)
    {
        ArgumentNullException.ThrowIfNull(draw);
        ArgumentNullException.ThrowIfNull(title);

        return new(draw, null, title);
    }

    /// <summary>A pane that draws nothing, for space deliberately left blank.</summary>
    /// <returns>The leaf.</returns>
    public static PaneTree Leaf() => new(static _ => { }, null);

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
    /// <param name="size">
    /// How much of it the first half takes; the second half takes the rest. A count of cells when
    /// written as an <c>int</c>, a share of the space when written with a decimal point — <c>3</c> is
    /// three rows or columns, <c>0.3</c> is three tenths. See <see cref="PaneSize"/>.
    /// </param>
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
    /// <param name="size">
    /// How much of it the first half takes; the second half takes the rest. A count of cells when
    /// written as an <c>int</c>, a share of the space when written with a decimal point — <c>3</c> is
    /// three rows or columns, <c>0.3</c> is three tenths. See <see cref="PaneSize"/>.
    /// </param>
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
    public void Draw(SurfaceRegion region)
    {
        var placed = new List<(PaneTree Pane, SurfaceRegion Region)>(Count);

        Place(OuterGap > 0 ? region.Inset(OuterGap) : region, InnerGap, placed);

        if (InnerGap == 0)
        {
            Share(placed);
        }

        var joinery = new Joinery();
        var inside = new SurfaceRegion[placed.Count];

        for (var pass = 0; pass < 2; pass++)
        {
            for (var index = 0; index < placed.Count; index++)
            {
                var (pane, area) = placed[index];

                if (pane._title is null)
                {
                    inside[index] = area;
                    continue;
                }

                if (pane.IsFocused == (pass == 1))
                {
                    inside[index] = joinery.Box(area, BorderOf(pane._widget), pane._title());
                }
            }
        }

        for (var index = 0; index < placed.Count; index++)
        {
            placed[index].Pane._content?.Invoke(inside[index]);
        }

        joinery.Draw(region, Theme.Info);
    }

    private bool IsFocused => _widget is IArlecchinoFocusable { IsFocused: true };

    /// <summary>
    /// Pulls every boxed pane onto the edge of the boxed pane before it, so that two of them touching
    /// share one line rather than drawing two side by side. Only panes in a box take part: one without
    /// would lose a column of what it draws to a neighbor's border.
    /// </summary>
    /// <param name="placed">Where each pane landed, in the order the tree lays them out.</param>
    private static void Share(List<(PaneTree Pane, SurfaceRegion Region)> placed)
    {
        for (var index = 1; index < placed.Count; index++)
        {
            var (pane, region) = placed[index];

            if (pane._title is null)
            {
                continue;
            }

            for (var before = 0; before < index; before++)
            {
                var (neighbour, taken) = placed[before];

                if (neighbour._title is null)
                {
                    continue;
                }

                if (region.Left == taken.Right && region.Top < taken.Bottom && taken.Top < region.Bottom)
                {
                    region = new(region.Surface, region.Left - 1, region.Top, region.Width + 1, region.Height);
                }

                if (region.Top == taken.Bottom && region.Left < taken.Right && taken.Left < region.Right)
                {
                    region = new(region.Surface, region.Left, region.Top - 1, region.Width, region.Height + 1);
                }
            }

            placed[index] = (pane, region);
        }
    }

    private static TermColor BorderOf(IArlecchinoWidget? widget) =>
        widget is IArlecchinoFocusable { IsFocused: true } ? Theme.Active : Theme.Info;

    private static PaneTree Build(PaneSplit? split, PaneSize size, PaneTree first, PaneTree second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        foreach (var widget in first._widgets)
        {
            foreach (var taken in second._widgets)
            {
                if (ReferenceEquals(widget, taken))
                {
                    throw new ArgumentException(
                        $"{widget.GetType().Name} is already a pane of this layout. A widget draws into " +
                        "one region and remembers it, so the same instance in two panes would draw twice " +
                        "and answer clicks for one of them only.",
                        nameof(second));
                }
            }
        }

        return new(split, size, first, second);
    }

    private void Place(SurfaceRegion region, int gap, List<(PaneTree Pane, SurfaceRegion Region)> placed)
    {
        if (_content is not null)
        {
            placed.Add((this, region));
            return;
        }

        if (_first is null || _second is null)
        {
            return;
        }

        if (SplitOf(region) == PaneSplit.Columns)
        {
            var usable = Math.Max(0, region.Width - gap);
            var (left, rest) = region.SplitLeft(_size.Of(usable));
            var (_, right) = rest.SplitLeft(Math.Min(gap, rest.Width));

            _first.Place(left, gap, placed);
            _second.Place(right, gap, placed);

            return;
        }

        var rows = Math.Max(0, region.Height - gap);
        var (top, below) = region.SplitTop(_size.Of(rows));
        var (_, bottom) = below.SplitTop(Math.Min(gap, below.Height));

        _first.Place(top, gap, placed);
        _second.Place(bottom, gap, placed);
    }

    private PaneSplit SplitOf(SurfaceRegion region) =>
        _split ?? (region.Width >= region.Height * CellAspect ? PaneSplit.Columns : PaneSplit.Rows);
}
