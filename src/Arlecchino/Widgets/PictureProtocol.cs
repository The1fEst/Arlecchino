using System;
using System.Globalization;
using System.Text;
using System.Threading;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

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
internal readonly record struct Placed(
    int Left,
    int Top,
    int Columns,
    int Rows,
    int CellWidth,
    int CellHeight,
    int Version);

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
    /// Draws the picture where it was placed. It draws rather than returning something to draw,
    /// because cells are not bytes: the block protocol writes into the cell grid and the other two
    /// hand the terminal a payload, and no single answer covers both.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <param name="pixels">The picture, row by row from the top left.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="placed">Where it ended up and how large.</param>
    public abstract void Draw(SurfaceRegion region, Rgb[] pixels, int width, int height, Placed placed);
}

/// <summary>
/// The picture as cells, two pixels to each: the upper half block painted in the colour of the pixel
/// above and its background in the colour of the pixel below.
/// </summary>
internal sealed class BlockPicture : PictureProtocol
{
    private const char UpperHalf = '▀';
    private const int PixelsPerCell = 2;

    private RgbTermColor[] _blocks = [];
    private (int Columns, int Rows, int Version) _composed;

    /// <inheritdoc/>
    public override ImageProtocol Kind => ImageProtocol.Blocks;

    /// <inheritdoc/>
    public override double PerCell(int cellWidth, int cellHeight) => PixelsPerCell;

    /// <inheritdoc/>
    public override void Draw(SurfaceRegion region, Rgb[] pixels, int width, int height, Placed placed)
    {
        Compose(pixels, width, height, placed);

        for (var row = 0; row < placed.Rows; row++)
        {
            for (var column = 0; column < placed.Columns; column++)
            {
                region.Write(
                    placed.Top + row,
                    placed.Left + column,
                    UpperHalf.ToString(),
                    _blocks[(row * placed.Columns) + column]);
            }
        }
    }

    /// <summary>
    /// Works out the colour of every cell and keeps the objects, so the next frame hands the surface the
    /// same instances rather than equal ones.
    ///
    /// That is what lets the frame diff do its job: it tells a cell apart from the one before it by
    /// reference, so a picture built fresh each frame looks changed in every cell and is written out in
    /// full however still it is. Rebuilding costs one pass over the cells and only when the picture or the
    /// room it is drawn in changes.
    /// </summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="placed">Where it ended up and how large.</param>
    private void Compose(Rgb[] pixels, int width, int height, Placed placed)
    {
        if (_composed == (placed.Columns, placed.Rows, placed.Version))
        {
            return;
        }

        if (_blocks.Length != placed.Columns * placed.Rows)
        {
            _blocks = new RgbTermColor[placed.Columns * placed.Rows];
        }

        var lines = placed.Rows * PixelsPerCell;

        for (var row = 0; row < placed.Rows; row++)
        {
            for (var column = 0; column < placed.Columns; column++)
            {
                _blocks[(row * placed.Columns) + column] = new()
                {
                    Foreground = At(pixels, width, height, column, placed.Columns, (row * PixelsPerCell) + 0, lines),
                    Background = At(pixels, width, height, column, placed.Columns, (row * PixelsPerCell) + 1, lines),
                };
            }
        }

        _composed = (placed.Columns, placed.Rows, placed.Version);
    }

    private static Rgb At(Rgb[] pixels, int width, int height, int column, int columns, int row, int lines)
    {
        var x = Math.Clamp(column * width / columns, 0, width - 1);
        var y = Math.Clamp(row * height / lines, 0, height - 1);

        return pixels[(y * width) + x];
    }
}

/// <summary>
/// The picture as pixels handed to the terminal rather than as cells. What the two protocols that do
/// this share is all of it but the bytes: the same shape of cell, a payload rebuilt only when the
/// picture or the room it is drawn in changes, and an undraw handed over beside it so the surface can
/// remove the picture once the frame stops offering it.
/// </summary>
internal abstract class PixelPicture : PictureProtocol
{
    /// <summary>
    /// What numbers are written with. An escape sequence is read by the terminal rather than by a
    /// person, so the digits in it must be the same whatever the machine is set to.
    /// </summary>
    protected static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private string _payload = "";
    private (int Columns, int Rows, int CellWidth, int CellHeight, int Version) _made;

    /// <inheritdoc/>
    public sealed override double PerCell(int cellWidth, int cellHeight) => (double)cellHeight / cellWidth;

    /// <inheritdoc/>
    public sealed override void Draw(SurfaceRegion region, Rgb[] pixels, int width, int height, Placed placed)
    {
        var made = (placed.Columns, placed.Rows, placed.CellWidth, placed.CellHeight, placed.Version);

        if (_made != made)
        {
            _payload = Build(pixels, width, height, placed);
            _made = made;
        }

        region.Surface.Passthrough(
            region.Top + placed.Top,
            region.Left + placed.Left,
            _payload,
            Undraw(placed));
    }

    /// <summary>Builds what is handed to the terminal.</summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="placed">Where it ended up and how large.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    protected abstract string Build(Rgb[] pixels, int width, int height, Placed placed);

    /// <summary>Builds what removes it again, or an empty string when nothing can.</summary>
    /// <param name="placed">Where it ended up and how large.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    protected abstract string Undraw(Placed placed);
}

/// <summary>
/// The kitty graphics protocol: the pixels as they are, base64 across chunks of the size the protocol
/// allows, told which cell rectangle to scale into.
/// </summary>
internal sealed class KittyPicture : PixelPicture
{
    private const int Chunk = 4096;

    private static int _lastImage = Random.Shared.Next(1, 1 << 24);

    private readonly int _image = Interlocked.Increment(ref _lastImage);

    /// <inheritdoc/>
    public override ImageProtocol Kind => ImageProtocol.Kitty;

    /// <summary>
    /// Builds the kitty graphics escape sequence. Responses are suppressed with <c>q=2</c>, since a reply
    /// from the terminal would arrive at the input reader as a stray escape sequence.
    ///
    /// It carries an image number of its own, so a picture that changes replaces the one the terminal is
    /// holding instead of adding to it. Without that every new set of pixels would be another image kept
    /// in the terminal's memory for as long as the session lasts.
    /// </summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="placed">Where it ended up and how large.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    protected override string Build(Rgb[] pixels, int width, int height, Placed placed)
    {
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
            var take = Math.Min(Chunk, encoded.Length - sent);
            var more = sent + take < encoded.Length ? 1 : 0;

            sequence.Append("\e_G");

            if (sent == 0)
            {
                sequence
                    .Append(Invariant, $"a=T,q=2,f=24,i={_image}")
                    .Append(Invariant, $",s={width}")
                    .Append(Invariant, $",v={height}")
                    .Append(Invariant, $",c={placed.Columns}")
                    .Append(Invariant, $",r={placed.Rows},");
            }

            sequence.Append(Invariant, $"m={more};").Append(encoded, sent, take).Append("\e\\");

            sent += take;
        }

        return sequence.ToString();
    }

    /// <summary>
    /// Builds the sequence that tells the terminal to let go of the image it was handed. Only kitty has
    /// one: sixel writes pixels into the screen rather than into a registry of images, so there is
    /// nothing there to name and nothing to delete.
    /// </summary>
    /// <param name="placed">Where it ended up and how large, which kitty does not need to be told.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    protected override string Undraw(Placed placed) => $"\e_Ga=d,d=i,i={_image},q=2\e\\";
}

/// <summary>
/// Sixel: the older protocol. Two things make it unlike kitty — it draws from colour registers, so the
/// picture is brought down to a palette of at most 256 by <see cref="IndexedImage"/>, and it is measured
/// in pixels rather than cells, so it is resampled to however many pixels the cells it was given come to.
/// </summary>
internal sealed class SixelPicture : PixelPicture
{
    private const int Registers = 256;
    private const int Band = 6;

    /// <inheritdoc/>
    public override ImageProtocol Kind => ImageProtocol.Sixel;

    /// <summary>
    /// Builds the sixel escape sequence. The pixels go out in bands of six rows, one pass per colour in
    /// the band, with runs of the same column collapsed — without that a photograph would weigh several
    /// times what it needs to.
    /// </summary>
    /// <param name="pixels">The picture.</param>
    /// <param name="width">Its width in pixels.</param>
    /// <param name="height">Its height in pixels.</param>
    /// <param name="placed">Where it ended up and how large.</param>
    /// <returns>The sequence to hand to the terminal.</returns>
    protected override string Build(Rgb[] pixels, int width, int height, Placed placed)
    {
        var image = IndexedImage.From(
            pixels,
            width,
            height,
            Math.Max(1, placed.Columns * placed.CellWidth),
            Math.Max(1, placed.Rows * placed.CellHeight),
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

        var here = new bool[image.Palette.Length];

        for (var top = 0; top < image.Height; top += Band)
        {
            Array.Clear(here);

            for (var row = top; row < Math.Min(top + Band, image.Height); row++)
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
                sixel.Append(Invariant, $"#{index}");

                AppendBand(sixel, image, top, index);
            }

            sixel.Append('-');
        }

        return sixel.Append("\e\\").ToString();
    }

    /// <summary>
    /// Builds a sixel that paints a rectangle in the colour the terminal said was behind its text, which
    /// is the only way to undraw one: sixel writes pixels into the screen, so what was drawn is gone only
    /// once something else is drawn over it.
    ///
    /// Empty when the terminal never said what colour that is — see
    /// <see cref="TerminalCapabilities.Background"/> — because painting a guessed colour leaves a
    /// rectangle anyone can see, which is worse than the pixels it was meant to remove.
    ///
    /// The last band paints only the rows the picture actually had. Sixel bands are six rows whatever the
    /// picture's height, so painting all six would reach up to five rows past it, and a terminal that does
    /// not clip to the raster size would show that as a line under the picture.
    /// </summary>
    /// <param name="placed">Where it ended up and how large.</param>
    /// <returns>The sequence to hand to the terminal, or an empty string.</returns>
    protected override string Undraw(Placed placed)
    {
        var across = placed.Columns * placed.CellWidth;
        var down = placed.Rows * placed.CellHeight;

        if (TerminalCapabilities.Background is not { } behind || across <= 0 || down <= 0)
        {
            return "";
        }

        var painted = new StringBuilder(64);

        painted
            .Append(Invariant, $"\ePq\"1;1;{across};{down}")
            .Append(Invariant, $"#0;2;{Percent(behind.Red)};{Percent(behind.Green)};{Percent(behind.Blue)}");

        for (var band = 0; band < down; band += Band)
        {
            var rows = Math.Min(Band, down - band);

            painted.Append(Invariant, $"#0!{across}{(char)(63 + ((1 << rows) - 1))}-");
        }

        return painted.Append("\e\\").ToString();
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
