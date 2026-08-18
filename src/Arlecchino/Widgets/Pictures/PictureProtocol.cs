using System;
using System.Globalization;
using System.Text;
using System.Threading;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Terminals;

namespace Arlecchino.Widgets.Pictures;

/// <summary>
/// Where a picture ended up and how large, worked out once by <see cref="Picture"/> because the
/// arithmetic is the same whichever way the pixels reach the terminal.
/// </summary>
/// <param name="Left">Cells from the left of the region.</param>
/// <param name="Top">Cells from the top of the region.</param>
/// <param name="Columns">Cells across.</param>
/// <param name="Rows">Cells down.</param>
/// <param name="CellWidth">Pixels across a cell.</param>
/// <param name="CellHeight">Pixels down a cell.</param>
/// <param name="Version">
/// Which set of pixels this is. It changes whenever the picture does, so anything cached against it
/// is thrown away at the same moment.
/// </param>
/// <param name="Detail">
/// How many pixels may be handed to the terminal at most, whatever the cells come to.
/// </param>
internal readonly record struct Placed(
    int Left,
    int Top,
    int Columns,
    int Rows,
    int CellWidth,
    int CellHeight,
    int Version,
    int Detail);

/// <summary>
/// One way of getting a picture onto the screen. Each way keeps only what it needs and draws only its
/// own code, so a picture drawn in cells carries nothing a sixel would want and neither knows the
/// other exists.
/// </summary>
internal abstract class PictureProtocol
{
    /// <summary>
    /// Which protocol this one is. It says so itself so that nothing holding one has to remember what
    /// it asked for alongside what it got.
    /// </summary>
    public abstract ImageProtocol Kind { get; }

    /// <summary>
    /// How many pixels tall a cell counts for, against one wide. Cells hold two stacked pixels
    /// whatever the terminal; the pixel protocols are told the real shape of a cell and use it.
    /// </summary>
    /// <param name="cellWidth">Pixels across a cell.</param>
    /// <param name="cellHeight">Pixels down a cell.</param>
    /// <returns>Pixels down for every one across.</returns>
    public abstract double PerCell(int cellWidth, int cellHeight);

    /// <summary>
    /// Draws the picture where it was placed. It draws rather than returning something to draw, since one
    /// protocol writes into the cell grid and the others hand the terminal a payload.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <param name="pixels">The picture, row by row from the top left.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="placement">Where it ended up and how large.</param>
    public abstract void Draw(SurfaceRegion region, Rgb[] pixels, int width, int height, Placed placement);
}

/// <summary>
/// The picture as cells, two pixels to each: the upper half block painted in the color of the pixel
/// above and its background in the color of the pixel below.
/// </summary>
internal sealed class BlockPicture : PictureProtocol
{
    private const char UpperHalf = '▀';
    private const int PixelsPerCell = 2;

    private RgbTermColor[] _blocks = [];
    private (int Columns, int Rows, int Version) _drawnAt;

    /// <inheritdoc/>
    public override ImageProtocol Kind => ImageProtocol.Blocks;

    /// <inheritdoc/>
    public override double PerCell(int cellWidth, int cellHeight) => PixelsPerCell;

    /// <inheritdoc/>
    public override void Draw(SurfaceRegion region, Rgb[] pixels, int width, int height, Placed placement)
    {
        Compose(pixels, width, height, placement);

        for (var row = 0; row < placement.Rows; row++)
        {
            for (var column = 0; column < placement.Columns; column++)
            {
                region.Write(
                    placement.Top + row,
                    placement.Left + column,
                    UpperHalf.ToString(),
                    _blocks[(row * placement.Columns) + column]);
            }
        }
    }

    /// <summary>
    /// Works out the color of every cell and keeps the objects, so the next frame hands the surface the same
    /// instances. The frame diff tells cells apart by reference, and rebuilt ones would all look changed.
    /// </summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="placement">Where it ended up and how large.</param>
    private void Compose(Rgb[] pixels, int width, int height, Placed placement)
    {
        if (_drawnAt == (placement.Columns, placement.Rows, placement.Version))
        {
            return;
        }

        if (_blocks.Length != placement.Columns * placement.Rows)
        {
            _blocks = new RgbTermColor[placement.Columns * placement.Rows];
        }

        var lines = placement.Rows * PixelsPerCell;

        for (var row = 0; row < placement.Rows; row++)
        {
            for (var column = 0; column < placement.Columns; column++)
            {
                _blocks[(row * placement.Columns) + column] = new()
                {
                    Foreground = At(pixels, width, height, column, placement.Columns, (row * PixelsPerCell) + 0, lines),
                    Background = At(pixels, width, height, column, placement.Columns, (row * PixelsPerCell) + 1, lines),
                };
            }
        }

        _drawnAt = (placement.Columns, placement.Rows, placement.Version);
    }

    private static Rgb At(Rgb[] pixels, int width, int height, int column, int columns, int row, int lines)
    {
        var x = Math.Clamp(column * width / columns, 0, width - 1);
        var y = Math.Clamp(row * height / lines, 0, height - 1);

        return pixels[(y * width) + x];
    }
}

/// <summary>
/// The picture as pixels handed to the terminal rather than as cells, which the two protocols that do it
/// share. Each rebuilds its payload only when the picture changes, and hands over a way to undraw it.
/// </summary>
internal abstract class PixelPicture : PictureProtocol
{
    /// <summary>
    /// What numbers are written with. An escape sequence is read by the terminal rather than by a
    /// person, so the digits in it must be the same whatever the machine is set to.
    /// </summary>
    protected static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private string _payload = "";
    private (int Columns, int Rows, int CellWidth, int CellHeight, int Version) _drawnFor;

    /// <inheritdoc/>
    public sealed override double PerCell(int cellWidth, int cellHeight) => (double)cellHeight / cellWidth;

    /// <inheritdoc/>
    public sealed override void Draw(SurfaceRegion region, Rgb[] pixels, int width, int height, Placed placement)
    {
        var state = (placement.Columns, placement.Rows, placement.CellWidth, placement.CellHeight, placement.Version);

        if (_drawnFor != state)
        {
            _payload = Build(pixels, width, height, placement);
            _drawnFor = state;
        }

        region.Surface.Passthrough(
            region.Top + placement.Top,
            region.Left + placement.Left,
            _payload,
            Undraw(placement));
    }

    /// <summary>Builds what is handed to the terminal.</summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="placement">Where it ended up and how large.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    protected abstract string Build(Rgb[] pixels, int width, int height, Placed placement);

    /// <summary>Builds what removes it again, or an empty string when nothing can.</summary>
    /// <param name="placement">Where it ended up and how large.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    protected abstract string Undraw(Placed placement);
}

/// <summary>
/// The kitty graphics protocol: the pixels base64 across chunks of the size the protocol allows, told
/// which cell rectangle to scale into. They are brought down to what that rectangle comes to first.
/// </summary>
internal sealed class KittyPicture : PixelPicture
{
    private const int Chunk = 4096;

    private static int _lastImage = Random.Shared.Next(1, 1 << 24);

    private readonly int _image = Interlocked.Increment(ref _lastImage);

    /// <inheritdoc/>
    public override ImageProtocol Kind => ImageProtocol.Kitty;

    /// <summary>
    /// Builds the kitty graphics escape sequence, with replies suppressed and an image number of its own. A
    /// picture that changes then replaces the one the terminal holds instead of adding to it.
    /// </summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="placement">Where it ended up and how large.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    protected override string Build(Rgb[] pixels, int width, int height, Placed placement)
    {
        var pixelWidth = Math.Min(width, Math.Max(1, placement.Columns * placement.CellWidth));
        var pixelHeight = Math.Min(height, Math.Max(1, placement.Rows * placement.CellHeight));

        if (placement.Detail > 0 && (long)pixelWidth * pixelHeight > placement.Detail)
        {
            var share = Math.Sqrt((double)placement.Detail / ((long)pixelWidth * pixelHeight));

            pixelWidth = Math.Max(1, (int)(pixelWidth * share));
            pixelHeight = Math.Max(1, (int)(pixelHeight * share));
        }

        var smaller = PictureScale.To(pixels, width, height, pixelWidth, pixelHeight);

        pixels = smaller.Pixels;
        width = smaller.Width;
        height = smaller.Height;

        var bytes = new byte[width * height * 3];

        for (var index = 0; index < width * height; index++)
        {
            bytes[(index * 3) + 0] = pixels[index].Red;
            bytes[(index * 3) + 1] = pixels[index].Green;
            bytes[(index * 3) + 2] = pixels[index].Blue;
        }

        var payload = Convert.ToBase64String(bytes);
        var sequence = new StringBuilder(payload.Length + 128);
        var at = 0;

        while (at < payload.Length)
        {
            var take = Math.Min(Chunk, payload.Length - at);
            var moreChunks = at + take < payload.Length ? 1 : 0;

            sequence.Append("\e_G");

            if (at == 0)
            {
                sequence
                    .Append(Invariant, $"a=T,q=2,f=24,i={_image}")
                    .Append(Invariant, $",s={width}")
                    .Append(Invariant, $",v={height}")
                    .Append(Invariant, $",c={placement.Columns}")
                    .Append(Invariant, $",r={placement.Rows},");
            }

            sequence.Append(Invariant, $"m={moreChunks};").Append(payload, at, take).Append("\e\\");

            at += take;
        }

        return sequence.ToString();
    }

    /// <summary>
    /// Builds the sequence that tells the terminal to let go of the image it was handed. Only kitty has one,
    /// since sixel writes pixels into the screen rather than into a registry.
    /// </summary>
    /// <param name="placement">Where it ended up and how large, which kitty does not need to be told.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    protected override string Undraw(Placed placement) => $"\e_Ga=d,d=i,i={_image},q=2\e\\";
}

/// <summary>
/// Sixel: the older protocol, drawn from color registers and measured in pixels. The picture is brought down
/// to a palette of 256 by <see cref="IndexedImage"/> and resampled to the pixels its cells come to.
/// </summary>
internal sealed class SixelPicture : PixelPicture
{
    private const int Registers = 256;
    private const int Band = 6;

    /// <inheritdoc/>
    public override ImageProtocol Kind => ImageProtocol.Sixel;

    /// <summary>
    /// Builds the sixel escape sequence. The pixels go out in bands of six rows, one pass per color in
    /// the band, with runs of the same column collapsed — without that a photograph would weigh several
    /// times what it needs to.
    /// </summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="placement">Where it ended up and how large.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    protected override string Build(Rgb[] pixels, int width, int height, Placed placement)
    {
        var image = IndexedImage.From(
            pixels,
            width,
            height,
            Math.Max(1, placement.Columns * placement.CellWidth),
            Math.Max(1, placement.Rows * placement.CellHeight),
            Registers);

        var sixel = new StringBuilder(image.Width * image.Height / 4);

        sixel.Append(Invariant, $"\ePq\"1;1;{image.Width};{image.Height}");

        for (var index = 0; index < image.Palette.Length; index++)
        {
            var color = image.Palette[index];

            sixel.Append(
                Invariant,
                $"#{index};2;{Percent(color.Red)};{Percent(color.Green)};{Percent(color.Blue)}");
        }

        var inBand = new bool[image.Palette.Length];

        for (var top = 0; top < image.Height; top += Band)
        {
            Array.Clear(inBand);

            for (var row = top; row < Math.Min(top + Band, image.Height); row++)
            {
                for (var column = 0; column < image.Width; column++)
                {
                    inBand[image.At(column, row)] = true;
                }
            }

            var written = false;

            for (var index = 0; index < inBand.Length; index++)
            {
                if (!inBand[index])
                {
                    continue;
                }

                if (written)
                {
                    sixel.Append('$');
                }

                written = true;
                sixel.Append(Invariant, $"#{index}");

                AppendBand(sixel, image, top, index);
            }

            sixel.Append('-');
        }

        return sixel.Append("\e\\").ToString();
    }

    /// <summary>
    /// Builds a sixel that paints over the picture in the color behind the text, which is the only way to
    /// remove one. It is empty where <see cref="TerminalCapabilities.Background"/> went unreported.
    /// </summary>
    /// <param name="placement">Where it ended up and how large.</param>
    /// <returns>The sequence to hand to the terminal, or an empty string.</returns>
    protected override string Undraw(Placed placement)
    {
        var pixelWidth = placement.Columns * placement.CellWidth;
        var pixelHeight = placement.Rows * placement.CellHeight;

        if (TerminalCapabilities.Background is not { } behind || pixelWidth <= 0 || pixelHeight <= 0)
        {
            return "";
        }

        var sequence = new StringBuilder(64);

        sequence
            .Append(Invariant, $"\ePq\"1;1;{pixelWidth};{pixelHeight}")
            .Append(Invariant, $"#0;2;{Percent(behind.Red)};{Percent(behind.Green)};{Percent(behind.Blue)}");

        for (var band = 0; band < pixelHeight; band += Band)
        {
            var rows = Math.Min(Band, pixelHeight - band);

            sequence.Append(Invariant, $"#0!{pixelWidth}{(char)(63 + ((1 << rows) - 1))}-");
        }

        return sequence.Append("\e\\").ToString();
    }

    private static int Percent(byte value) => ((value * 100) + 127) / 255;

    private static void AppendBand(StringBuilder sixel, IndexedImage image, int top, int index)
    {
        var run = 0;
        var previous = '\0';

        for (var column = 0; column < image.Width; column++)
        {
            var bits = 0;

            for (var bit = 0; bit < Band && top + bit < image.Height; bit++)
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
            sixel.Append(Invariant, $"!{run}");
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
}
