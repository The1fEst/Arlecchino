using System;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Rendering.Terminals;

namespace Arlecchino.Widgets.Pictures;

/// <summary>
/// An image drawn in cells, two pixels to each, or in a graphics protocol where <see cref="Protocol"/> names
/// one. The pixels are handed over rather than read from a file, since decoding belongs to the application.
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
    private Rgb[] _pixels = [];
    private PictureProtocol? _drawing;
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
    /// How many pixels a protocol that hands pixels over may write at most, whatever the pane comes to.
    /// The ceiling trades a little sharpness for a picture that appears at once; nought lifts it.
    /// </summary>
    public int Detail { get; set; } = 512 * 1024;

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

    /// <summary>
    /// Forgets the picture, leaving the region to whatever draws next. What the terminal was handed as
    /// pixels is undrawn on the next frame — see how <see cref="Surface.Passthrough"/> takes a payload
    /// back — so this needs no more than forgetting them.
    /// </summary>
    public void Clear()
    {
        _pixels = [];
        PixelWidth = 0;
        PixelHeight = 0;
        _version++;
    }

    /// <summary>
    /// Draws the picture as large as it goes inside the region without stretching it, centered, and
    /// returns an empty region: a picture fills what it is given, so hand over the pane it belongs in.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <returns>An empty region.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (region.IsEmpty)
        {
            return region;
        }

        if (Background is not null)
        {
            region.Fill(Background);
        }

        if (IsEmpty)
        {
            return region.Rows(region.Height, 0);
        }

        var drawing = Chosen(TerminalCapabilities.Resolve(Protocol ?? Glyphs.Picture));
        var cellWidth = Math.Max(1, Glyphs.CellWidth);
        var cellHeight = Math.Max(1, Glyphs.CellHeight);

        var perCell = drawing.PerCell(cellWidth, cellHeight);

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

        drawing.Draw(
            region,
            _pixels,
            PixelWidth,
            PixelHeight,
            new(left, top, columns, rows, cellWidth, cellHeight, _version, Detail));

        return region.Rows(region.Height, 0);
    }

    /// <summary>
    /// Picks the way of drawing that was asked for, which is the one place the protocol is named. Only the
    /// one in use is built, and it is kept until the protocol changes.
    /// </summary>
    /// <param name="protocol">Which protocol the terminal and the application settled on.</param>
    /// <returns>The one that draws it.</returns>
    private PictureProtocol Chosen(ImageProtocol protocol)
    {
        if (_drawing?.Kind == protocol)
        {
            return _drawing;
        }

        return _drawing = protocol switch
        {
            ImageProtocol.Kitty => new KittyPicture(),
            ImageProtocol.Sixel => new SixelPicture(),
            _ => new BlockPicture(),
        };
    }
}
