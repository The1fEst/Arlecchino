using System;
using System.Collections.Generic;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Modals.Drawing;

/// <summary>
/// The box every dialog is drawn in: bordered, centered, as wide as the widest line in it, with a rule above
/// the hints along the bottom. A dialog decides what its lines say and nothing about the box.
/// </summary>
internal sealed class ModalBox
{
    private const int LeastInner = 34;
    private const int StackedRows = 1;
    private const int StackedColumns = 3;

    private readonly Surface _surface;

    /// <summary>Draws boxes on a surface.</summary>
    /// <param name="surface">The cell grid frames are built in.</param>
    public ModalBox(Surface surface) => _surface = surface;

    /// <summary>
    /// How many dialogs are already open under this one. Each is offset a row down and three columns
    /// along, so a dialog opened from a dialog reads as being on top of it rather than instead of it.
    /// </summary>
    public int Depth { get; set; }

    /// <summary>The rule that separates the body of a dialog from its hints.</summary>
    /// <param name="box">The whole box, border included.</param>
    /// <param name="insideRow">Which row inside the box it goes under.</param>
    public static void Divider(SurfaceRegion box, int insideRow) =>
        box.Write(insideRow + 1, 0, $"├{new string('─', Math.Max(0, box.Width - 2))}┤", Theme.Info);

    /// <summary>A box of the size asked for, in the middle of the screen and never off the edge of it.</summary>
    /// <param name="contentWidth">How wide what goes in it is.</param>
    /// <param name="contentHeight">How tall.</param>
    /// <returns>Where the box goes.</returns>
    public SurfaceRegion Centered(int contentWidth, int contentHeight)
    {
        var width = Math.Min(contentWidth + 4, _surface.FrameWidth - 4);
        var height = Math.Min(contentHeight + 2, _surface.FrameHeight - 2);

        var left = Math.Max(0, (_surface.FrameWidth - width) / 2) + (Depth * StackedColumns);
        var top = Math.Max(0, (_surface.FrameHeight - height) / 2) + (Depth * StackedRows);

        return new(
            _surface,
            Math.Clamp(left, 0, Math.Max(0, _surface.FrameWidth - width)),
            Math.Clamp(top, 0, Math.Max(0, _surface.FrameHeight - height)),
            width,
            height);
    }

    /// <summary>Draws a titled box holding the lines given, with the hints under a rule.</summary>
    /// <param name="title">What the dialog is called.</param>
    /// <param name="body">The lines, each a run of pieces.</param>
    /// <param name="footer">What the keys do, along the bottom.</param>
    /// <returns>The whole box, and the region inside it the lines were written in.</returns>
    public (SurfaceRegion Box, SurfaceRegion Content) Draw(string title, IReadOnlyList<Piece[]> body, string footer)
    {
        var inner = Math.Max(TextWidth.Of(title) + 4, LeastInner);

        foreach (var line in body)
        {
            inner = Math.Max(inner, LineWidth(line) + 2);
        }

        inner = Math.Max(inner, TextWidth.Of(footer) + 2);

        var box = Centered(inner, body.Count + 2);
        var content = box.Border(Theme.Info, title).Inset(new Margin(1, 0, 1, 0));

        for (var row = 0; row < body.Count; row++)
        {
            Line(content, row, body[row]);
        }

        Divider(box, body.Count);
        content.WriteLine(body.Count + 1, footer, Theme.Secondary);

        return (box, content);
    }

    private static void Line(SurfaceRegion content, int row, Piece[] pieces)
    {
        var offset = 0;

        foreach (var piece in pieces)
        {
            if (offset >= content.Width)
            {
                return;
            }

            var text = TextWidth.Truncate(piece.Text, content.Width - offset);

            content.Write(row, offset, text, piece.Style);
            offset += TextWidth.Of(text);
        }
    }

    private static int LineWidth(Piece[] line)
    {
        var width = 0;

        foreach (var piece in line)
        {
            width += TextWidth.Of(piece.Text);
        }

        return width;
    }
}
