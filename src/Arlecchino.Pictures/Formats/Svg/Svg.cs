using System;
using System.Collections.Generic;
using System.Text;

namespace Arlecchino.Pictures.Formats.Svg;

/// <summary>
/// Reads the flat <c>SVG</c> a chart is written as: rectangles, lines, runs of lines, polygons, circles
/// and ellipses. A file carrying a path, text, a transform, a gradient or a style sheet is refused whole.
/// </summary>
public sealed class Svg : IPictureFormat
{
    private const int Longest = 4096;
    private const double Sharpest = 8d;
    private const double Smallest = 0.1d;

    private static readonly string[] RefusedElements =
    [
        "path", "text", "tspan", "textpath", "image", "use", "style", "foreignobject", "marker", "mask",
        "clippath", "pattern", "lineargradient", "radialgradient", "filter", "symbol", "switch", "animate",
    ];

    private static readonly string[] IgnoredElements = ["svg", "g", "defs", "title", "desc", "metadata"];

    /// <inheritdoc />
    public string Name => "svg";

    /// <inheritdoc />
    public bool Starts(ReadOnlySpan<byte> bytes)
    {
        var at = 0;

        while (at < bytes.Length && Spare(bytes[at]))
        {
            at++;
        }

        if (at >= bytes.Length || bytes[at] != (byte)'<')
        {
            return false;
        }

        var head = bytes[at..Math.Min(bytes.Length, at + Longest)];

        return head.IndexOf("<svg"u8) >= 0;
    }

    /// <inheritdoc />
    public Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits)
    {
        if (!Starts(bytes))
        {
            return null;
        }

        var text = Encoding.UTF8.GetString(bytes);
        var tags = new SvgTags(text);
        var shapes = new List<Action<SvgCanvas, double>>();
        var size = (Width: 0d, Height: 0d);
        var read = false;

        while (tags.Next(out var name, out var attributes))
        {
            if (Array.IndexOf(RefusedElements, name) >= 0 || attributes.ContainsKey("transform"))
            {
                return null;
            }

            if (name == "svg" && !read)
            {
                size = Measured(attributes);
                read = true;
            }

            if (Array.IndexOf(IgnoredElements, name) >= 0)
            {
                continue;
            }

            if (Shaped(name, attributes) is not { } shape)
            {
                return null;
            }

            shapes.Add(shape);
        }

        return shapes.Count == 0 ? null : Drawn(shapes, size, limits);
    }

    /// <summary>
    /// Draws every shape at the size the caller has a use for. A picture drawn from shapes has no size of
    /// its own to speak of, so the one the file names is what it is scaled from.
    /// </summary>
    /// <param name="shapes">What to draw, in the order the file wrote them.</param>
    /// <param name="size">The size the file names.</param>
    /// <param name="limits">What the caller will hold and what it has a use for.</param>
    /// <returns>The pixels, or <c>null</c> where the size is one nothing can be drawn at.</returns>
    private static Raster? Drawn(
        List<Action<SvgCanvas, double>> shapes,
        (double Width, double Height) size,
        PictureLimits limits)
    {
        if (size.Width <= 0d || size.Height <= 0d)
        {
            return null;
        }

        var natural = size.Width * size.Height;
        var pixels = limits.EnoughPixels > 0 ? limits.EnoughPixels : natural;
        var room = Math.Min(pixels, limits.MostPixels);
        var scale = Math.Clamp(Math.Sqrt(room / natural), Smallest, Sharpest);
        var width = Math.Max(1, (int)Math.Round(size.Width * scale));
        var height = Math.Max(1, (int)Math.Round(size.Height * scale));

        if ((long)width * height > limits.MostPixels)
        {
            return null;
        }

        var canvas = new SvgCanvas(width, height);

        canvas.Wash(new(0xFF, 0xFF, 0xFF));

        foreach (var shape in shapes)
        {
            shape(canvas, scale);
        }

        return new(canvas.Pixels, width, height);
    }

    /// <summary>
    /// How large the picture is: what the document says it is, or else what it says it holds. A size given
    /// as a share of something — the page in a browser — says nothing here, and the box is taken instead.
    /// </summary>
    /// <param name="attributes">What the <c>svg</c> element was written with.</param>
    /// <returns>The size.</returns>
    private static (double Width, double Height) Measured(Dictionary<string, string> attributes)
    {
        var box = SvgPaint.Numbers(attributes.GetValueOrDefault("viewbox"));
        var saidWidth = attributes.GetValueOrDefault("width");
        var saidHeight = attributes.GetValueOrDefault("height");
        var width = Shared(saidWidth) ? 0d : SvgPaint.Number(saidWidth);
        var height = Shared(saidHeight) ? 0d : SvgPaint.Number(saidHeight);

        if (box.Count == 4 && (width <= 0d || height <= 0d))
        {
            return (box[2], box[3]);
        }

        return (width, height);
    }

    /// <summary>Whether a size is given as a share of whatever the picture is drawn into.</summary>
    /// <param name="value">What the attribute said.</param>
    /// <returns><c>true</c> when it is a share rather than a length.</returns>
    private static bool Shared(string? value) =>
        value is not null && value.Contains('%', StringComparison.Ordinal);

    /// <summary>Whether a byte is one that may stand before the document starts.</summary>
    /// <param name="value">The byte.</param>
    /// <returns><c>true</c> for white space and for the mark a file may open with.</returns>
    private static bool Spare(byte value) =>
        value is (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n' or 0xEF or 0xBB or 0xBF;

    /// <summary>
    /// What one element draws, scaled to the size the picture is read at. The colors are read here rather
    /// than where it is drawn, so a file naming one nothing can read is refused before anything is drawn.
    /// </summary>
    /// <param name="name">What the element is called.</param>
    /// <param name="attributes">What it was written with.</param>
    /// <returns>What draws it, or <c>null</c> where it is an element or a color this cannot read.</returns>
    private static Action<SvgCanvas, double>? Shaped(string name, Dictionary<string, string> attributes)
    {
        if (!SvgPaint.Read(attributes.GetValueOrDefault("fill"), SvgPaint.Black, out var fill) ||
            !SvgPaint.Read(attributes.GetValueOrDefault("stroke"), null, out var stroke))
        {
            return null;
        }

        fill = Faded(fill, attributes, "fill-opacity");
        stroke = Faded(stroke, attributes, "stroke-opacity");

        var thickness = SvgPaint.Number(attributes.GetValueOrDefault("stroke-width"), 1d);
        var dashes = SvgPaint.Numbers(attributes.GetValueOrDefault("stroke-dasharray"));

        return name switch
        {
            "rect" => (canvas, scale) => canvas.Rectangle(
                SvgPaint.Number(attributes.GetValueOrDefault("x")) * scale,
                SvgPaint.Number(attributes.GetValueOrDefault("y")) * scale,
                (SvgPaint.Number(attributes.GetValueOrDefault("x")) +
                 SvgPaint.Number(attributes.GetValueOrDefault("width"))) *
                scale,
                (SvgPaint.Number(attributes.GetValueOrDefault("y")) +
                 SvgPaint.Number(attributes.GetValueOrDefault("height"))) *
                scale,
                fill,
                stroke,
                thickness * scale),
            "line" => (canvas, scale) => canvas.Line(
                SvgPaint.Number(attributes.GetValueOrDefault("x1")) * scale,
                SvgPaint.Number(attributes.GetValueOrDefault("y1")) * scale,
                SvgPaint.Number(attributes.GetValueOrDefault("x2")) * scale,
                SvgPaint.Number(attributes.GetValueOrDefault("y2")) * scale,
                stroke ?? SvgPaint.Black,
                thickness * scale,
                Scaled(dashes, scale)),
            "polyline" or "polygon" => (canvas, scale) => canvas.Shape(
                Scaled(SvgPaint.Numbers(attributes.GetValueOrDefault("points")), scale),
                name == "polygon" ? fill : NotFilled(attributes, fill),
                stroke,
                thickness * scale,
                Scaled(dashes, scale),
                name == "polygon"),
            "circle" or "ellipse" => (canvas, scale) => canvas.Disc(
                SvgPaint.Number(attributes.GetValueOrDefault("cx")) * scale,
                SvgPaint.Number(attributes.GetValueOrDefault("cy")) * scale,
                Across(attributes) * scale,
                Down(attributes) * scale,
                fill,
                stroke,
                thickness * scale),
            _ => null,
        };
    }

    /// <summary>
    /// What a run of lines is filled with. A polyline is a line rather than a shape, and a document that
    /// says nothing about filling one means it to be left open, whatever a lone shape would default to.
    /// </summary>
    /// <param name="attributes">What the element was written with.</param>
    /// <param name="fill">What it would be filled with.</param>
    /// <returns>That, or nothing where the document said nothing.</returns>
    private static SvgColor? NotFilled(Dictionary<string, string> attributes, SvgColor? fill) =>
        attributes.ContainsKey("fill") ? fill : null;

    /// <summary>One color with what the document said about how much of it there is.</summary>
    /// <param name="color">The color, or nothing where the shape asked for none.</param>
    /// <param name="attributes">What the element was written with.</param>
    /// <param name="name">Which of the two opacities to read.</param>
    /// <returns>The color, faded.</returns>
    private static SvgColor? Faded(SvgColor? color, Dictionary<string, string> attributes, string name)
    {
        if (color is not { } said)
        {
            return null;
        }

        var alpha = SvgPaint.Number(attributes.GetValueOrDefault(name), 1d) *
                    SvgPaint.Number(attributes.GetValueOrDefault("opacity"), 1d);

        return said with { Alpha = Math.Clamp(alpha, 0d, 1d) };
    }

    private static double Across(Dictionary<string, string> attributes) =>
        attributes.ContainsKey("rx")
            ? SvgPaint.Number(attributes.GetValueOrDefault("rx"))
            : SvgPaint.Number(attributes.GetValueOrDefault("r"));

    private static double Down(Dictionary<string, string> attributes) =>
        attributes.ContainsKey("ry")
            ? SvgPaint.Number(attributes.GetValueOrDefault("ry"))
            : SvgPaint.Number(attributes.GetValueOrDefault("r"));

    /// <summary>Every number of a list at the size the picture is read at.</summary>
    /// <param name="numbers">The numbers as the document wrote them.</param>
    /// <param name="scale">How much larger the picture is being drawn.</param>
    /// <returns>The numbers, scaled.</returns>
    private static List<double> Scaled(List<double> numbers, double scale)
    {
        var scaledNumbers = new List<double>(numbers.Count);

        foreach (var number in numbers)
        {
            scaledNumbers.Add(number * scale);
        }

        return scaledNumbers;
    }
}
