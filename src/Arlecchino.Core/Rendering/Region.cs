using System;

namespace Arlecchino.Rendering;

/// <summary>
/// A rectangle on a <see cref="Surface"/> with its own coordinates and its own clipping: writing
/// outside it is dropped rather than spilled onto a neighbour. Split a frame into regions instead of
/// counting columns by hand, and the same geometry answers "was this click inside".
/// </summary>
/// <param name="Surface">The surface drawn into.</param>
/// <param name="Left">Frame column of the left edge.</param>
/// <param name="Top">Frame row of the top edge.</param>
/// <param name="Width">Width in cells.</param>
/// <param name="Height">Height in rows.</param>
public readonly record struct Region(Surface Surface, int Left, int Top, int Width, int Height)
{
    /// <summary>Frame column just past the right edge.</summary>
    public int Right => Left + Width;

    /// <summary>Frame row just past the bottom edge.</summary>
    public int Bottom => Top + Height;

    /// <summary>Whether the region has no room to draw in.</summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>Whether a frame cell falls inside this region — the hit test for mouse events.</summary>
    /// <param name="frameRow">Row in frame coordinates.</param>
    /// <param name="frameColumn">Column in frame coordinates.</param>
    /// <returns><c>true</c> when the cell is inside.</returns>
    public bool Contains(int frameRow, int frameColumn) =>
        frameRow >= Top && frameRow < Bottom && frameColumn >= Left && frameColumn < Right;

    /// <summary>Converts a frame cell into coordinates local to this region.</summary>
    /// <param name="frameRow">Row in frame coordinates.</param>
    /// <param name="frameColumn">Column in frame coordinates.</param>
    /// <returns>The same cell as a row and column inside the region.</returns>
    public (int Row, int Column) ToLocal(int frameRow, int frameColumn) =>
        (frameRow - Top, frameColumn - Left);

    /// <summary>A smaller region inside this one.</summary>
    /// <param name="margin">Space to keep free on each side.</param>
    /// <returns>The region that is left.</returns>
    public Region Inset(Margin margin) => new(
        Surface,
        Left + margin.Left,
        Top + margin.Top,
        Math.Max(0, Width - margin.Left - margin.Right),
        Math.Max(0, Height - margin.Top - margin.Bottom));

    /// <summary>A smaller region with the same space kept free on every side.</summary>
    /// <param name="all">Cells to keep free.</param>
    /// <returns>The region that is left.</returns>
    public Region Inset(int all) => Inset(new Margin(all));

    /// <summary>Cuts a column off the left. The split is clamped to what the region actually has.</summary>
    /// <param name="width">Cells to give to the left part.</param>
    /// <returns>The left part and the rest.</returns>
    public (Region Left, Region Right) SplitLeft(int width)
    {
        var taken = Math.Clamp(width, 0, Width);
        return (this with { Width = taken },
            this with { Left = Left + taken, Width = Width - taken });
    }

    /// <summary>Cuts a band off the top. The split is clamped to what the region actually has.</summary>
    /// <param name="height">Rows to give to the top part.</param>
    /// <returns>The top part and the rest.</returns>
    public (Region Top, Region Bottom) SplitTop(int height)
    {
        var taken = Math.Clamp(height, 0, Height);
        return (this with { Height = taken },
            this with { Top = Top + taken, Height = Height - taken });
    }

    /// <summary>A horizontal band of this region, clamped to its bounds.</summary>
    /// <param name="row">First row, local to this region.</param>
    /// <param name="count">How many rows to take.</param>
    /// <returns>The band.</returns>
    public Region Rows(int row, int count) => new(
        Surface,
        Left,
        Top + Math.Clamp(row, 0, Height),
        Width,
        Math.Clamp(count, 0, Math.Max(0, Height - Math.Clamp(row, 0, Height))));

    /// <summary>
    /// Writes text in region coordinates, clipped to the region. A negative column starts the text
    /// off the left edge and shows what fits.
    /// </summary>
    /// <param name="row">Row inside the region.</param>
    /// <param name="column">Column inside the region.</param>
    /// <param name="text">Text to draw.</param>
    /// <param name="style">Style for the text.</param>
    public void Write(int row, int column, string text, ITermColor style)
    {
        if (row < 0 || row >= Height || column >= Width)
        {
            return;
        }

        var visible = column < 0
            ? TextWidth.Truncate(SkipColumns(text, -column), Width)
            : TextWidth.Truncate(text, Width - column);

        Surface.WriteAt(Top + row, Left + Math.Max(0, column), visible, style);
    }

    /// <summary>Writes a whole line, aligned inside the region and clipped to its width.</summary>
    /// <param name="row">Row inside the region.</param>
    /// <param name="text">Text to draw.</param>
    /// <param name="style">Style for the text.</param>
    /// <param name="align">Horizontal alignment inside the region.</param>
    public void WriteLine(int row, string text, ITermColor style, Align align = Align.Left)
    {
        var clipped = TextWidth.Truncate(text, Width);
        var column = align.HasFlag(Align.Center)
            ? Math.Max(0, (Width - TextWidth.Of(clipped)) / 2)
            : align.HasFlag(Align.Right)
                ? Math.Max(0, Width - TextWidth.Of(clipped))
                : 0;

        Write(row, column, clipped, style);
    }

    /// <summary>Paints every cell of the region.</summary>
    /// <param name="style">Style to paint with.</param>
    /// <param name="character">Character to fill with; a space by default.</param>
    public void Fill(ITermColor style, char character = ' ')
    {
        var line = new string(character, Width);
        for (var row = 0; row < Height; row++)
        {
            Write(row, 0, line, style);
        }
    }

    /// <summary>
    /// Draws a box around the region and hands back the space inside it, so panes and dialogs are
    /// one call rather than four loops.
    /// </summary>
    /// <param name="style">Style for the frame.</param>
    /// <param name="title">Optional title written into the top edge.</param>
    /// <returns>The region inside the frame, or this region when it is too small for one.</returns>
    public Region Border(ITermColor style, string title = "")
    {
        if (Width < 2 || Height < 2)
        {
            return this;
        }

        var inner = Width - 2;
        var head = title.Length == 0
            ? new('─', inner)
            : BuildTitleBar(title, inner);

        Write(0, 0, $"╭{head}╮", style);

        var side = new string(' ', inner);
        for (var row = 1; row < Height - 1; row++)
        {
            Write(row, 0, "│", style);
            Write(row, 1, side, style);
            Write(row, Width - 1, "│", style);
        }

        Write(Height - 1, 0, $"╰{new string('─', inner)}╯", style);

        return Inset(1);
    }

    private static string BuildTitleBar(string title, int inner)
    {
        var clipped = TextWidth.Truncate(title, Math.Max(0, inner - 3));
        var used = TextWidth.Of(clipped) + 3;
        return $"─ {clipped} {new string('─', Math.Max(0, inner - used))}";
    }

    private static string SkipColumns(string text, int columns)
    {
        var skipped = 0;
        var index = 0;

        while (index < text.Length && skipped < columns)
        {
            var length = TextWidth.NextClusterLength(text, index);
            skipped += TextWidth.OfCluster(text.AsSpan(index, length));
            index += length;
        }

        return text[index..];
    }
}
