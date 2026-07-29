using System;
using System.Text;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>
/// An image drawn in cells. Each cell carries two pixels — the upper half block is painted in the
/// colour of the pixel above and its background in the colour of the pixel below — so a cell, which
/// is about twice as tall as it is wide, comes out roughly square per pixel.
///
/// That is the default because it needs nothing of the terminal but the colour it already draws in:
/// no protocol, no state left behind, nothing to clean up when the picture goes away. Where the
/// terminal speaks a graphics protocol, <see cref="Protocol"/> sends the pixels themselves instead and
/// the picture is as sharp as the screen allows.
///
/// The pixels are handed over rather than read from a file: decoding PNG or JPEG belongs to the
/// application, which knows what it wants to depend on, while the framework only draws what it is
/// given.
///
/// <code>
/// private readonly Picture _preview = new();
///
/// _preview.Show(pixels, width, height);
/// _preview.Draw(region);
/// </code>
/// </summary>
public sealed class Picture : IArlecchinoWidget
{
    private const char UpperHalf = '▀';
    private const int PixelsPerCell = 2;

    private Rgb[] _pixels = [];
    private string _payload = "";
    private (ImageProtocol Protocol, int Columns, int Rows, int Width, int Height, int Version) _made;
    private int _version;

    /// <summary>How wide the picture is, in pixels.</summary>
    public int PixelWidth { get; private set; }

    /// <summary>How tall the picture is, in pixels.</summary>
    public int PixelHeight { get; private set; }

    /// <summary>Whether there is anything to draw.</summary>
    public bool IsEmpty => _pixels.Length == 0;

    /// <summary>
    /// What to draw behind the picture where the region is wider or taller than the picture fits.
    /// The terminal's own background when left alone.
    /// </summary>
    public IArlecchinoColor? Background { get; init; }

    /// <summary>
    /// How the picture reaches the terminal. The application's own setting —
    /// <see cref="Glyphs.Picture"/> — when left alone, so one pane can differ without every other one
    /// being told.
    /// </summary>
    public ImageProtocol? Protocol { get; set; }

    /// <summary>
    /// Hands over the pixels to draw, row by row from the top left. They are copied, so the caller is
    /// free to reuse its buffer.
    /// </summary>
    /// <param name="pixels">The pixels, <paramref name="width"/> × <paramref name="height"/> of them.</param>
    /// <param name="width">How wide the picture is.</param>
    /// <param name="height">How tall the picture is.</param>
    public void Show(ReadOnlySpan<Rgb> pixels, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);

        if (pixels.Length < width * height)
        {
            throw new ArgumentException(
                $"a {width}×{height} picture needs {width * height} pixels, not {pixels.Length}",
                nameof(pixels));
        }

        _pixels = width * height == 0 ? [] : pixels[..(width * height)].ToArray();
        PixelWidth = width;
        PixelHeight = height;
        _version++;
    }

    /// <summary>Forgets the picture, leaving the region to whatever draws next.</summary>
    public void Clear()
    {
        _pixels = [];
        PixelWidth = 0;
        PixelHeight = 0;
        _version++;
    }

    /// <summary>
    /// Draws the picture as large as it goes inside the region without stretching it, centred, and
    /// returns an empty region: a picture fills what it is given, so hand it the pane it belongs in.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <returns>An empty region.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (region.IsEmpty)
        {
            return region;
        }

        if (Background is { } behind)
        {
            region.Fill(behind);
        }

        if (IsEmpty)
        {
            return region.Rows(region.Height, 0);
        }

        var protocol = Protocol ?? Glyphs.Picture;
        var cellWidth = Math.Max(1, Glyphs.CellWidth);
        var cellHeight = Math.Max(1, Glyphs.CellHeight);

        var perCell = protocol == ImageProtocol.Blocks
            ? PixelsPerCell
            : (double)cellHeight / cellWidth;

        var scale = Math.Min(
            (double)region.Width / PixelWidth,
            region.Height * perCell / PixelHeight);

        var columns = Math.Clamp((int)Math.Round(PixelWidth * scale), 1, region.Width);
        var rows = Math.Clamp(
            (int)Math.Round(PixelHeight * scale / perCell),
            1,
            region.Height);

        var left = (region.Width - columns) / 2;
        var top = (region.Height - rows) / 2;

        if (protocol != ImageProtocol.Blocks)
        {
            var made = (protocol, columns, rows, cellWidth, cellHeight, _version);

            if (_made != made)
            {
                _payload = protocol == ImageProtocol.Kitty
                    ? Kitty(_pixels, PixelWidth, PixelHeight, columns, rows)
                    : Sixel(_pixels, PixelWidth, PixelHeight, columns * cellWidth, rows * cellHeight);

                _made = made;
            }

            region.Surface.Passthrough(region.Top + top, region.Left + left, _payload);

            return region.Rows(region.Height, 0);
        }

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var upper = At(column, columns, (row * PixelsPerCell) + 0, rows * PixelsPerCell);
                var lower = At(column, columns, (row * PixelsPerCell) + 1, rows * PixelsPerCell);

                region.Write(
                    top + row,
                    left + column,
                    UpperHalf.ToString(),
                    new RgbTermColor { Foreground = upper, Background = lower });
            }
        }

        return region.Rows(region.Height, 0);
    }

    /// <summary>
    /// Builds the kitty graphics escape sequence: the pixels as they are, base64 across chunks of the
    /// size the protocol allows, told which cell rectangle to scale into. Responses are suppressed with
    /// <c>q=2</c>, since a reply from the terminal would arrive at the input reader as a stray escape
    /// sequence.
    /// </summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="columns">Cells to fill across.</param>
    /// <param name="rows">Cells to fill down.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    private static string Kitty(Rgb[] pixels, int width, int height, int columns, int rows)
    {
        const int chunk = 4096;

        var bytes = new byte[width * height * 3];

        for (var index = 0; index < width * height; index++)
        {
            bytes[(index * 3) + 0] = pixels[index].Red;
            bytes[(index * 3) + 1] = pixels[index].Green;
            bytes[(index * 3) + 2] = pixels[index].Blue;
        }

        var encoded = Convert.ToBase64String(bytes);
        var sequence = new StringBuilder(encoded.Length + 128);
        var sent = 0;

        while (sent < encoded.Length)
        {
            var take = Math.Min(chunk, encoded.Length - sent);
            var more = sent + take < encoded.Length ? 1 : 0;

            sequence.Append("\e_G");

            if (sent == 0)
            {
                sequence
                    .Append("a=T,q=2,f=24,s=").Append(width)
                    .Append(",v=").Append(height)
                    .Append(",c=").Append(columns)
                    .Append(",r=").Append(rows)
                    .Append(',');
            }

            sequence.Append("m=").Append(more).Append(';').Append(encoded, sent, take).Append("\e\\");

            sent += take;
        }

        return sequence.ToString();
    }

    /// <summary>
    /// Builds the sixel escape sequence. Two things make it unlike kitty: the format draws from colour
    /// registers, so the picture is brought down to a palette of at most 256 by
    /// <see cref="IndexedImage"/>, and it is measured in pixels rather than cells, so it is resampled to
    /// however many pixels the cells it was given come to.
    ///
    /// The pixels go out in bands of six rows, one pass per colour in the band, with runs of the same
    /// column collapsed — without that a photograph would weigh several times what it needs to.
    /// </summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="across">Pixels to fill across.</param>
    /// <param name="down">Pixels to fill down.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    private static string Sixel(Rgb[] pixels, int width, int height, int across, int down)
    {
        const int registers = 256;
        const int band = 6;

        var image = IndexedImage.From(pixels, width, height, Math.Max(1, across), Math.Max(1, down), registers);
        var sixel = new StringBuilder(image.Width * image.Height / 4);

        sixel.Append("\ePq\"1;1;").Append(image.Width).Append(';').Append(image.Height);

        for (var index = 0; index < image.Palette.Length; index++)
        {
            var color = image.Palette[index];

            sixel
                .Append('#').Append(index).Append(";2;")
                .Append(Percent(color.Red)).Append(';')
                .Append(Percent(color.Green)).Append(';')
                .Append(Percent(color.Blue));
        }

        var here = new bool[image.Palette.Length];

        for (var top = 0; top < image.Height; top += band)
        {
            Array.Clear(here);

            for (var row = top; row < Math.Min(top + band, image.Height); row++)
            {
                for (var column = 0; column < image.Width; column++)
                {
                    here[image.At(column, row)] = true;
                }
            }

            var written = false;

            for (var index = 0; index < here.Length; index++)
            {
                if (!here[index])
                {
                    continue;
                }

                if (written)
                {
                    sixel.Append('$');
                }

                written = true;
                sixel.Append('#').Append(index);

                AppendBand(sixel, image, top, index);
            }

            sixel.Append('-');
        }

        return sixel.Append("\e\\").ToString();
    }

    private static int Percent(byte value) => ((value * 100) + 127) / 255;

    private static void AppendBand(StringBuilder sixel, IndexedImage image, int top, int index)
    {
        var run = 0;
        var previous = '\0';

        for (var column = 0; column < image.Width; column++)
        {
            var bits = 0;

            for (var bit = 0; bit < 6 && top + bit < image.Height; bit++)
            {
                if (image.At(column, top + bit) == index)
                {
                    bits |= 1 << bit;
                }
            }

            var symbol = (char)(63 + bits);

            if (symbol == previous)
            {
                run++;
                continue;
            }

            AppendRun(sixel, previous, run);
            previous = symbol;
            run = 1;
        }

        AppendRun(sixel, previous, run);
    }

    private static void AppendRun(StringBuilder sixel, char symbol, int run)
    {
        if (run == 0)
        {
            return;
        }

        if (run > 3)
        {
            sixel.Append('!').Append(run);
        }
        else
        {
            for (var again = 1; again < run; again++)
            {
                sixel.Append(symbol);
            }
        }

        sixel.Append(symbol);
    }

    private Rgb At(int column, int columns, int row, int lines)
    {
        var x = Math.Clamp(column * PixelWidth / columns, 0, PixelWidth - 1);
        var y = Math.Clamp(row * PixelHeight / lines, 0, PixelHeight - 1);

        return _pixels[(y * PixelWidth) + x];
    }
}
