using System;
using System.Collections.Generic;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Rendering.Text;

/// <summary>
/// Lines that know about one another. Boxes and rules are recorded first and painted at the end, so a cell
/// two of them share becomes the glyph that joins them rather than one line drawn over the other.
///
/// <code>
/// var joinery = new Joinery();
///
/// var files = joinery.Box(left, Theme.Info, "files");
/// var log = joinery.Box(right, Theme.Active, "log");
///
/// joinery.Draw(surface.Content, Theme.Info);
/// </code>
/// </summary>
public sealed class Joinery
{
    private const int ToTop = 1;
    private const int ToRight = 2;
    private const int ToBottom = 4;
    private const int ToLeft = 8;

    private static readonly char[] Glyphs =
    [
        ' ', '│', '─', '╰', '│', '│', '╭', '├', '─', '╯', '─', '┴', '╮', '┤', '┬', '┼',
    ];

    private readonly Dictionary<(int Row, int Column), Joint> _joints = [];
    private readonly List<(int Row, int Column, string Title, IArlecchinoColor? Style)> _titles = [];

    /// <summary>How many cells carry a line so far. Nothing has reached the surface yet.</summary>
    public int Count => _joints.Count;

    /// <summary>
    /// Records the four edges of a region and hands back the room inside them, the way
    /// <see cref="SurfaceRegion.Border"/> does.
    /// </summary>
    /// <param name="region">What to draw a box around.</param>
    /// <param name="style">How its lines are drawn; the style given to <see cref="Draw"/> when omitted.</param>
    /// <param name="title">What to write into the top edge, or nothing.</param>
    /// <returns>The region inside the box.</returns>
    public SurfaceRegion Box(SurfaceRegion region, IArlecchinoColor? style = null, string title = "")
    {
        if (region.Width < 2 || region.Height < 2)
        {
            return region;
        }

        var right = region.Right - 1;
        var bottom = region.Bottom - 1;

        Horizontal(region.Top, region.Left, right, style);
        Horizontal(bottom, region.Left, right, style);
        Vertical(region.Left, region.Top, bottom, style);
        Vertical(right, region.Top, bottom, style);

        if (title.Length > 0)
        {
            _titles.Add((region.Top, region.Left + 2, title, style));
        }

        return new(region.Surface, region.Left + 1, region.Top + 1, region.Width - 2, region.Height - 2);
    }

    /// <summary>Records a rule across a region, for a divider that should join the surrounding box.</summary>
    /// <param name="region">The region to cross.</param>
    /// <param name="row">Which of its rows, counted from its top.</param>
    /// <param name="style">How it is drawn; the style given to <see cref="Draw"/> when omitted.</param>
    public void Across(SurfaceRegion region, int row, IArlecchinoColor? style = null) =>
        Horizontal(region.Top + row, region.Left, region.Right - 1, style);

    /// <summary>Records a rule down a region.</summary>
    /// <param name="region">The region to cross.</param>
    /// <param name="column">Which of its columns, counted from its left.</param>
    /// <param name="style">How it is drawn; the style given to <see cref="Draw"/> when omitted.</param>
    public void Down(SurfaceRegion region, int column, IArlecchinoColor? style = null) =>
        Vertical(region.Left + column, region.Top, region.Bottom - 1, style);

    /// <summary>
    /// Paints everything recorded, resolving each cell into the glyph its neighbors ask for, and
    /// then writes the titles over the top edges they belong to. Anything falling outside the region
    /// is left undrawn rather than clamped into it.
    /// </summary>
    /// <param name="into">Where to paint; coordinates recorded are the surface's own.</param>
    /// <param name="style">How lines recorded without a style of their own are drawn.</param>
    public void Draw(SurfaceRegion into, IArlecchinoColor style)
    {
        ArgumentNullException.ThrowIfNull(style);

        foreach (var ((row, column), joint) in _joints)
        {
            var local = (Row: row - into.Top, Column: column - into.Left);

            if (local.Row < 0 || local.Row >= into.Height || local.Column < 0 || local.Column >= into.Width)
            {
                continue;
            }

            into.Write(local.Row, local.Column, Glyphs[joint.Flags].ToString(), joint.Style ?? style);
        }

        foreach (var (row, column, title, titled) in _titles)
        {
            var local = (Row: row - into.Top, Column: column - into.Left);

            if (local.Row < 0 || local.Row >= into.Height || local.Column < 0 || local.Column >= into.Width)
            {
                continue;
            }

            var room = into.Width - local.Column - 2;

            if (room > 0)
            {
                into.Write(local.Row, local.Column, $" {TextWidth.Truncate(title, room)} ", titled ?? style);
            }
        }
    }

    private void Horizontal(int row, int from, int to, IArlecchinoColor? style)
    {
        for (var column = from; column <= to; column++)
        {
            var flags = 0;

            if (column > from)
            {
                flags |= ToLeft;
            }

            if (column < to)
            {
                flags |= ToRight;
            }

            Mark(row, column, flags, style);
        }
    }

    private void Vertical(int column, int from, int to, IArlecchinoColor? style)
    {
        for (var row = from; row <= to; row++)
        {
            var flags = 0;

            if (row > from)
            {
                flags |= ToTop;
            }

            if (row < to)
            {
                flags |= ToBottom;
            }

            Mark(row, column, flags, style);
        }
    }

    private void Mark(int row, int column, int flags, IArlecchinoColor? style)
    {
        var at = (row, column);

        _joints[at] = _joints.TryGetValue(at, out var held)
            ? new(held.Flags | flags, style ?? held.Style)
            : new(flags, style);
    }

    private readonly record struct Joint(int Flags, IArlecchinoColor? Style);
}
