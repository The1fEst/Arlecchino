using System;
using System.Collections.Generic;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures.Formats.Svg;

/// <summary>
/// The pixels a document is drawn into. Every shape is laid down by the coverage it has of each pixel,
/// a thin line drawn by the middle of a pixel alone being a staircase.
/// </summary>
/// <param name="width">How wide the picture is.</param>
/// <param name="height">How tall it is.</param>
internal sealed class SvgCanvas(int width, int height)
{
    private const double Half = 0.5d;

    /// <summary>How many points across and down a pixel is tested at when a shape is filled.</summary>
    private const int Samples = 3;

    /// <summary>The thinnest a line is drawn, so that one given no thickness is still there to be seen.</summary>
    private const double Thinnest = 0.1d;

    private readonly RGB[] _pixels = new RGB[width * height];

    /// <summary>The picture, row by row from the top left.</summary>
    public RGB[] Pixels => _pixels;

    /// <summary>Lays one color over the whole picture, which is how it is given a background.</summary>
    /// <param name="color">The color.</param>
    public void Wash(RGB color) => Array.Fill(_pixels, color);

    /// <summary>Draws a rectangle, filled, outlined, or both.</summary>
    /// <param name="left">Where its left edge is.</param>
    /// <param name="top">Where its top edge is.</param>
    /// <param name="right">Where its right edge is.</param>
    /// <param name="bottom">Where its bottom edge is.</param>
    /// <param name="fill">What it is filled with, or nothing for a rectangle that is not filled.</param>
    /// <param name="stroke">What it is outlined with, or nothing for one that is not outlined.</param>
    /// <param name="thickness">How thick that outline is.</param>
    public void Rectangle(
        double left,
        double top,
        double right,
        double bottom,
        SvgColor? fill,
        SvgColor? stroke,
        double thickness)
    {
        if (fill is { } inside)
        {
            Area(left, top, right, bottom, inside);
        }

        if (stroke is not { } edge)
        {
            return;
        }

        var corners = new List<double>
        {
            left, top,
            right, top,
            right, bottom,
            left, bottom,
        };

        Shape(corners, null, edge, thickness, [], closed: true);
    }

    /// <summary>Draws a run of joined lines, filled inside, outlined along, or both.</summary>
    /// <param name="points">The corners, as x and y one after another.</param>
    /// <param name="fill">What the shape is filled with, or nothing for one that is not filled.</param>
    /// <param name="stroke">What its lines are drawn with, or nothing for one with no lines.</param>
    /// <param name="thickness">How thick those lines are.</param>
    /// <param name="dashes">The dashes to draw them with, or none for a solid line.</param>
    /// <param name="closed">Whether the last corner joins the first.</param>
    public void Shape(
        List<double> points,
        SvgColor? fill,
        SvgColor? stroke,
        double thickness,
        List<double> dashes,
        bool closed)
    {
        var corners = points.Count / 2;

        if (corners == 0)
        {
            return;
        }

        if (fill is { } inside && corners > 2)
        {
            Inside(points, inside);
        }

        if (stroke is not { } edge)
        {
            return;
        }

        var pattern = new SvgDashes(dashes);

        for (var corner = 1; corner < corners; corner++)
        {
            Dashed(points, corner - 1, corner, edge, thickness, pattern);
        }

        if (closed && corners > 2)
        {
            Dashed(points, corners - 1, 0, edge, thickness, pattern);
        }

        Joined(points, corners, edge, thickness, pattern, closed);
    }

    /// <summary>
    /// Rounds off the corners of a run of lines, filling the nick that square ends leave where two lines
    /// meet. A dashed line is left alone, since the document drew no corner that falls in a gap.
    /// </summary>
    /// <param name="points">The corners, as x and y one after another.</param>
    /// <param name="corners">How many of them there are.</param>
    /// <param name="stroke">What the lines are drawn with.</param>
    /// <param name="thickness">How thick they are.</param>
    /// <param name="dashes">The pattern they are drawn with.</param>
    /// <param name="closed">Whether the last corner joins the first.</param>
    private void Joined(
        List<double> points,
        int corners,
        SvgColor stroke,
        double thickness,
        SvgDashes dashes,
        bool closed)
    {
        if (!dashes.Solid || thickness <= 1d || corners < 3)
        {
            return;
        }

        var first = closed ? 0 : 1;
        var last = closed ? corners : corners - 1;

        for (var corner = first; corner < last; corner++)
        {
            Disc(points[corner * 2], points[(corner * 2) + 1], thickness / 2d, thickness / 2d, stroke, null, 0d);
        }
    }

    /// <summary>Draws one line.</summary>
    /// <param name="fromX">Where it starts across.</param>
    /// <param name="fromY">Where it starts down.</param>
    /// <param name="toX">Where it ends across.</param>
    /// <param name="toY">Where it ends down.</param>
    /// <param name="stroke">What it is drawn with.</param>
    /// <param name="thickness">How thick it is.</param>
    /// <param name="dashes">The dashes to draw it with, or none for a solid line.</param>
    public void Line(
        double fromX,
        double fromY,
        double toX,
        double toY,
        SvgColor stroke,
        double thickness,
        List<double> dashes)
    {
        Dashed([fromX, fromY, toX, toY], 0, 1, stroke, thickness, new(dashes));
    }

    /// <summary>Draws a circle or an ellipse, filled, outlined, or both.</summary>
    /// <param name="centreX">Where its middle is across.</param>
    /// <param name="centreY">Where its middle is down.</param>
    /// <param name="radiusX">How far it reaches across.</param>
    /// <param name="radiusY">How far it reaches down.</param>
    /// <param name="fill">What it is filled with, or nothing for one that is not filled.</param>
    /// <param name="stroke">What it is outlined with, or nothing for one that is not outlined.</param>
    /// <param name="thickness">How thick that outline is.</param>
    public void Disc(
        double centreX,
        double centreY,
        double radiusX,
        double radiusY,
        SvgColor? fill,
        SvgColor? stroke,
        double thickness)
    {
        if (radiusX <= 0d || radiusY <= 0d)
        {
            return;
        }

        var reach = Math.Min(radiusX, radiusY);
        var edge = stroke is null ? 0d : (thickness / 2d) + 1d;

        for (var row = FirstRow(centreY - radiusY - edge); row <= LastRow(centreY + radiusY + edge); row++)
        {
            for (var column = FirstColumn(centreX - radiusX - edge);
                 column <= LastColumn(centreX + radiusX + edge);
                 column++)
            {
                var offX = (column + Half - centreX) / radiusX;
                var offY = (row + Half - centreY) / radiusY;
                var distance = (Math.Sqrt((offX * offX) + (offY * offY)) - 1d) * reach;

                if (fill is { } inside)
                {
                    Lay(column, row, inside, Math.Clamp(Half - distance, 0d, 1d));
                }

                if (stroke is { } outline)
                {
                    Lay(column, row, outline, Math.Clamp((thickness / 2d) + Half - Math.Abs(distance), 0d, 1d));
                }
            }
        }
    }

    /// <summary>
    /// Draws one line of a shape, in dashes where it asked for them. The pattern runs on from where the
    /// line before it left off, so an outline dashes evenly around its corners.
    /// </summary>
    /// <param name="points">The corners of the shape.</param>
    /// <param name="from">Which corner the line starts at.</param>
    /// <param name="to">Which corner it ends at.</param>
    /// <param name="stroke">What it is drawn with.</param>
    /// <param name="thickness">How thick it is.</param>
    /// <param name="dashes">Where along the pattern the line before this one left off.</param>
    private void Dashed(
        List<double> points,
        int from,
        int to,
        SvgColor stroke,
        double thickness,
        SvgDashes dashes)
    {
        var fromX = points[from * 2];
        var fromY = points[(from * 2) + 1];
        var toX = points[to * 2];
        var toY = points[(to * 2) + 1];

        if (dashes.Solid)
        {
            Straight(fromX, fromY, toX, toY, stroke, thickness);

            return;
        }

        var spanX = toX - fromX;
        var spanY = toY - fromY;
        var length = Math.Sqrt((spanX * spanX) + (spanY * spanY));
        var offset = 0d;

        while (offset < length)
        {
            var step = Math.Min(dashes.Left, length - offset);

            if (dashes.Drawn)
            {
                Straight(
                    fromX + (spanX * offset / length),
                    fromY + (spanY * offset / length),
                    fromX + (spanX * (offset + step) / length),
                    fromY + (spanY * (offset + step) / length),
                    stroke,
                    thickness);
            }

            offset += step;
            dashes.Take(step);
        }
    }

    /// <summary>Lays a line down, pixel by pixel, by how near each is to it.</summary>
    /// <param name="fromX">Where it starts across.</param>
    /// <param name="fromY">Where it starts down.</param>
    /// <param name="toX">Where it ends across.</param>
    /// <param name="toY">Where it ends down.</param>
    /// <param name="stroke">What it is drawn with.</param>
    /// <param name="thickness">How thick it is.</param>
    private void Straight(double fromX, double fromY, double toX, double toY, SvgColor stroke, double thickness)
    {
        var spanX = toX - fromX;
        var spanY = toY - fromY;
        var length = Math.Sqrt((spanX * spanX) + (spanY * spanY));

        if (length <= 0d)
        {
            return;
        }

        var wide = Math.Max(thickness, Thinnest) / 2d;
        var reach = wide + 1d;
        var alongX = spanX / length;
        var alongY = spanY / length;

        for (var row = FirstRow(Math.Min(fromY, toY) - reach); row <= LastRow(Math.Max(fromY, toY) + reach); row++)
        {
            for (var column = FirstColumn(Math.Min(fromX, toX) - reach);
                 column <= LastColumn(Math.Max(fromX, toX) + reach);
                 column++)
            {
                var offX = column + Half - fromX;
                var offY = row + Half - fromY;
                var offset = (offX * alongX) + (offY * alongY);
                var distance = Math.Abs((offX * -alongY) + (offY * alongX));
                var coverage = Math.Clamp(wide + Half - distance, 0d, 1d) *
                               Math.Clamp(Math.Min(offset, length - offset) + Half, 0d, 1d);

                Lay(column, row, stroke, coverage);
            }
        }
    }

    /// <summary>Fills an upright rectangle, each edge pixel by how much of it the rectangle covers.</summary>
    /// <param name="left">Its left edge.</param>
    /// <param name="top">Its top edge.</param>
    /// <param name="right">Its right edge.</param>
    /// <param name="bottom">Its bottom edge.</param>
    /// <param name="fill">What to fill it with.</param>
    private void Area(double left, double top, double right, double bottom, SvgColor fill)
    {
        for (var row = FirstRow(top); row <= LastRow(bottom); row++)
        {
            var coverage = Math.Clamp(Math.Min(bottom, row + 1d) - Math.Max(top, row), 0d, 1d);

            if (coverage <= 0d)
            {
                continue;
            }

            for (var column = FirstColumn(left); column <= LastColumn(right); column++)
            {
                var wide = Math.Clamp(Math.Min(right, column + 1d) - Math.Max(left, column), 0d, 1d);

                Lay(column, row, fill, coverage * wide);
            }
        }
    }

    /// <summary>
    /// Fills what a run of corners encloses, counting the crossings of nine points to the pixel. A shape
    /// whose own lines cross is filled by what it encloses, as a document means it to be.
    /// </summary>
    /// <param name="points">The corners, as x and y one after another.</param>
    /// <param name="fill">What to fill it with.</param>
    private void Inside(List<double> points, SvgColor fill)
    {
        var corners = points.Count / 2;
        var left = double.MaxValue;
        var right = double.MinValue;
        var top = double.MaxValue;
        var bottom = double.MinValue;

        for (var corner = 0; corner < corners; corner++)
        {
            left = Math.Min(left, points[corner * 2]);
            right = Math.Max(right, points[corner * 2]);
            top = Math.Min(top, points[(corner * 2) + 1]);
            bottom = Math.Max(bottom, points[(corner * 2) + 1]);
        }

        for (var row = FirstRow(top); row <= LastRow(bottom); row++)
        {
            for (var column = FirstColumn(left); column <= LastColumn(right); column++)
            {
                var hits = 0;

                for (var downSample = 0; downSample < Samples; downSample++)
                {
                    for (var acrossSample = 0; acrossSample < Samples; acrossSample++)
                    {
                        var atX = column + ((acrossSample + Half) / Samples);
                        var atY = row + ((downSample + Half) / Samples);

                        hits += Holds(points, corners, atX, atY) ? 1 : 0;
                    }
                }

                Lay(column, row, fill, (double)hits / (Samples * Samples));
            }
        }
    }

    /// <summary>Whether a run of corners encloses a point, counted by how often its edges are crossed.</summary>
    /// <param name="points">The corners.</param>
    /// <param name="corners">How many of them there are.</param>
    /// <param name="atX">The point, across.</param>
    /// <param name="atY">The point, down.</param>
    /// <returns><c>true</c> when the corners enclose it.</returns>
    private static bool Holds(List<double> points, int corners, double atX, double atY)
    {
        var inside = false;
        var last = corners - 1;

        for (var corner = 0; corner < corners; last = corner++)
        {
            var thisX = points[corner * 2];
            var thisY = points[(corner * 2) + 1];
            var lastX = points[last * 2];
            var lastY = points[(last * 2) + 1];

            if (thisY > atY != lastY > atY &&
                atX < (((lastX - thisX) * (atY - thisY) / (lastY - thisY)) + thisX))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    /// <summary>Lays a color over one pixel, as much of it as the shape covers.</summary>
    /// <param name="column">Which pixel, across.</param>
    /// <param name="row">Which pixel, down.</param>
    /// <param name="color">The color.</param>
    /// <param name="coverage">How much of the pixel it covers, from nought to one.</param>
    private void Lay(int column, int row, SvgColor color, double coverage)
    {
        var alpha = coverage * color.Alpha;

        if (alpha <= 0d || column < 0 || column >= width || row < 0 || row >= height)
        {
            return;
        }

        var at = (row * width) + column;
        var destination = _pixels[at];

        _pixels[at] = new(
            Mixed(color.Color.Red, alpha, destination.Red),
            Mixed(color.Color.Green, alpha, destination.Green),
            Mixed(color.Color.Blue, alpha, destination.Blue));
    }

    private static int FirstRow(double top) => Math.Max(0, (int)Math.Floor(top));

    private int LastRow(double bottom) => Math.Min(height - 1, (int)Math.Ceiling(bottom));

    private static int FirstColumn(double left) => Math.Max(0, (int)Math.Floor(left));

    private int LastColumn(double right) => Math.Min(width - 1, (int)Math.Ceiling(right));

    private static byte Mixed(byte source, double alpha, byte destination) =>
        (byte)Math.Clamp(Math.Round((source * alpha) + (destination * (1d - alpha))), 0d, 255d);
}
